using MySql.Data.MySqlClient;
using Network_Support;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SpaceWars
{
    public class ServerController
    {
        World world;
        Settings settings;
        int playerCount; //tracks the number of players. used to set player IDs
        Dictionary<int, Player> players; //tracks player stats
        Dictionary<int, SocketState> sockets; // list of all connected players
        Stack<int> deadSocketsIDs;
        Stopwatch watch = new Stopwatch();
        Stopwatch ExtraFeatureWatch = new Stopwatch(); //if extra features turned on, it is updated every second
        Stopwatch GameDuration = new Stopwatch(); //if extra features turned on, it is updated every second

        bool gameStarted; //becomes true as soon as the first player connects
        bool statsSent;
        bool stopGame;


        int projectilecount; //tracks the number of projectiles on the screen

        public delegate void ExtraFeature(); //function contains code for extra game modes


        /// <summary>
        /// The server controller is passed the game world and settings.
        /// After it initializes some controller specific trackers, it will initiate port listeners and then 
        /// wait for the first player to join. Once a player has joined, the main server class will tell the controller to
        /// start the game.
        /// 
        /// </summary>
        /// <param name="theSettings">game settings</param>
        /// <param name="theWorld">the game world</param>
        public ServerController(Settings theSettings, World theWorld)
        {
            settings = theSettings;
            world = theWorld;
            playerCount = 0;
            projectilecount = 0;

            sockets = new Dictionary<int, SocketState>();
            deadSocketsIDs = new Stack<int>();
            players = new Dictionary<int, Player>();
            gameStarted = false;
            statsSent = false;

            Networking.ServerAwaitingClientLoop(HandleNewClient);
            Networking.ServerAwaitingClientLoop(HandleHttpConnection, Networking.HTTP_PORT);


            

            while (sockets.Count == 0 && world.getShips().Count < 1) { }


        }


        /// <summary>
        /// As long as stopGame is false, the StartGame will continuously run the Update function.
        /// Every second the game will generate a random number that will decide whether or not to turn extra
        /// features on, off, or just change the star's velocity. 
        /// 
        /// the game duration will be tracked with a stop watch.
        /// </summary>
        public void StartGame()
        {
            watch.Start();
            GameDuration.Start();
            stopGame = false;

            ExtraFeatureWatch.Start();
            ExtraFeature features = NoExtraFeatures; //features is passed to the update function to run the extra features code
            while (!stopGame)
            {
                
                //If the moving star extra feature is turned on, then this code is ran
                //every second, the extra feature has a 1/8 chance to turn on, a 1/8 chance to turn off
                //otherwise, the velocity of the stars are updated.
                if (settings.MovingStars && ExtraFeatureWatch.ElapsedMilliseconds > 1000)
                {
                    ExtraFeatureWatch.Restart();

                    Random random = new Random();
                    int results = random.Next(8);
                    if (results == 1)
                    {
                        features = MovingStars; //turns moving stars on
                    }
                    else if (results == 0)
                    {
                        features = NoExtraFeatures; //turns moving stars off
                    }
                    else
                    {
                        ChangeStarVelocity(); //updates the velocity of all stars
                    }
                }

                Update(features);

            }

            // PS9 Code - when the stopGame bool is set to true, the while loop will be broken out of
            // and this code called to send the stats to the server.
            if (gameStarted == true && stopGame==true && !statsSent)
            {
                if (players.Count > 0)
                {
                    Console.WriteLine("Server was stopped. Sending stats to database.");
                    GameDuration.Stop();

                    SendStats(GameDuration.Elapsed);
                    statsSent = true;

                    Console.WriteLine("Stats sent.\n");
                }
                else
                {
                    Console.WriteLine("No players joined the name. There are no stats to send.");
                }
                Console.WriteLine("Press any key to close this window");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Tells the server to stop the game
        /// </summary>
        public void StopGame()
        {
            stopGame = true;
        }


        /// <summary>
        /// helper function for the Moving Stars extra feature.
        /// The velocity of each star is randomly generated.
        /// </summary>
        public void ChangeStarVelocity()
        { 
            lock (world)
            {
                Random rand = new Random();
                Dictionary<int, Star> stars = world.getStar().ToDictionary(entry => entry.Key,
                                               entry => entry.Value); ;

                foreach (Star star in stars.Values)
                {
                    Vector2D vel = new Vector2D(2 * rand.NextDouble() - 1, 2 * rand.NextDouble() - 1);
                    star.SetVelocity(vel);

                    world.Update(WithinBounds(star));

                }
            }

        }

        public void MovingStars()
        {
            lock (world)
            {

                Dictionary<int, Star> stars = world.getStar().ToDictionary(entry => entry.Key,
                                               entry => entry.Value); ;

                foreach (Star star in stars.Values)
                {
                    star.SetLocation(star.GetLocation() + star.GetVelocity());
                    world.Update(star);
                }
            }
        }

        /// <summary>
        /// No extra features is empty to turn off Extra Features
        /// </summary>
        public void NoExtraFeatures()
        {

        }


        /// <summary>
        /// Each time Update is ran, it cooresponds with a single frame of the game.
        /// 
        /// It will look at each ship and determine if the player has requested that their ship turn, fire, or thrust.
        /// After, it will update the position of the ship and determine if a collision between the ship and a projectile
        /// or star has occurred.
        /// 
        /// Projectiles will have the projectile speed added to the projectile's location.
        /// 
        /// All updates are added to a string builder and sent to each player.
        /// </summary>
        /// <param name="Feature">Extra Feature Code</param>
        public void Update(ExtraFeature Feature)
        {

            while (watch.ElapsedMilliseconds < settings.MSPerFrame) { Thread.Yield(); }
            watch.Restart();
            lock (world)
            {
                Feature(); //runs code for extra features

                StringBuilder frameString = new StringBuilder(); //updates for all objects are added and send in bulk

                //PLAYERS =========================
                Dictionary<int, Ship> ships = world.getShips();
                lock (players)
                {

                    for (int ii = 0; ii < playerCount; ii++)
                    {
                        if (ships.ContainsKey(ii)) // ship not contained if it has been destroyed or player disconnected
                        {
                            Vector2D totalAccelleration = new Vector2D(0, 0);

                            //Calculate the effect of gravity from stars
                            foreach (Star star in world.getStar().Values)
                            {
                                Vector2D gravity = star.GetLocation() - ships[ii].GetLocation();
                                gravity.Normalize();
                                gravity = gravity * star.GetMass();

                                totalAccelleration = totalAccelleration + gravity;
                            }

                            //ROTATING ===========================
                            if (players[ii].GetRecentLeft()) //checks to see if the player sent a "turn left" request
                            {

                                ships[ii].Rotate(-1, settings.TurningRate);

                                players[ii].SetRecentLeft(false);  //both included if player presses both left and right 
                                players[ii].SetRecentRight(false); // on the keyboard
                            }
                            if (players[ii].GetRecentRight())
                            {
                                ships[ii].Rotate(1, settings.TurningRate);

                                players[ii].SetRecentRight(false);

                            }

                            //THRUST ============================
                            if (players[ii].GetRecentThrust())
                            {
                                ships[ii].SetThrust(true);

                                Vector2D thrust = ships[ii].GetDirection();     //calculate thrust from engines and
                                thrust = thrust * settings.EngineStrength; //add it to total accelleration
                                totalAccelleration = totalAccelleration + thrust;

                                players[ii].SetRecentThrust(false);
                            }
                            else
                                ships[ii].SetThrust(false);

                            //FIRING PROJECTILES ======================
                            if (players[ii].GetRecentProjectile() && players[ii].ReadyToFire())
                            {
                                Vector2D loc = ships[ii].GetLocation(); //new vectors must be created so that the projectile's location
                                                                        // and direction don't reference the same object in memory as 
                                                                        //the ship
                                Vector2D dir = ships[ii].GetDirection();

                                Projectile proj = new Projectile(projectilecount, ii, new Vector2D(loc.GetX(), loc.GetY()), new Vector2D(dir.GetX(), dir.GetY()));
                                world.Update(proj);
                                players[ii].ProjectileFired();
                                projectilecount = (projectilecount + 1) % 1000;

                                players[ii].AddShotsFired();
                                players[ii].SetRecentProjectile(false);
                            }
                            else
                            {
                                players[ii].AddProjectileFrame(); //ran every time a shot isn't fired
                                                                  //increments counter that determines how many frames
                                                                  //it has been since the last shot
                                players[ii].SetRecentProjectile(false);
                            }

                            //SCORE==================
                            if (players[ii].GetRecentScore())
                            {
                                int score = ships[ii].GetScore();
                                score++;
                                ships[ii].SetScore(score);

                                players[ii].SetRecentScore(false);
                            }




                            ships[ii].UpdateLocation(totalAccelleration);   // Adds acceleration to velocity
                                                                            // which is added to location

                            ships[ii] = WithinBounds(ships[ii]);

                            //COLLISION DETECTION FOR SHIPS/STARS ===============
                            foreach (Star star in world.getStar().Values)
                            {
                                Vector2D distanceBetween = star.GetLocation() - ships[ii].GetLocation();
                                if (distanceBetween.Length() < settings.StarSize)
                                {
                                    ships[ii].SetHp(0);
                                }
                            }

                            //COLLISION DETECTION FOR SHIPS/PROJECTILES ===============
                            foreach (Projectile proj in world.getProjectile().Values)
                            {
                                int projID = proj.GetOwner();
                                if (projID != ii)
                                {
                                    Vector2D distanceBetween = proj.GetLocation() - ships[ii].GetLocation();
                                    if ((distanceBetween.Length() < settings.ShipSize) && proj.GetActive())
                                    {
                                        proj.SetAlive(false);
                                        players[projID].AddHit();
                                        int newHP = ships[ii].GetHp() - 1;

                                        if (newHP <= 0)
                                        {

                                            players[projID].AddScore();
                                            players[projID].SetRecentScore(true);
                                        }

                                        ships[ii].SetHp(ships[ii].GetHp() - 1);
                                    }
                                }
                            }


                            frameString.Append(JsonConvert.SerializeObject(ships[ii]) + '\n');

                            world.Update(ships[ii]);
                        }
                        else
                        {
                            //determine if player is still connected
                            //then
                            //ship respawn code
                            if (sockets.ContainsKey(ii))
                            {
                                Player player = players[ii];
                                if (player.GetFramesSinceDead() <= settings.RespawnRate)
                                {
                                    player.IncrementFramesSinceDead();
                                }
                                else
                                {
                                    player.ResetFramesSinceDead();
                                    getRandomLocAndDir(out Vector2D location, out Vector2D direction);
                                    Ship newShip = new Ship(player.GetID(), player.GetName(), location, direction);
                                    newShip.SetHp(settings.StartingHitPoints);
                                    newShip.SetScore(players[ii].GetScore());
                                    world.Update(newShip);
                                }
                            }
                        }
                    }
                }

                //STARS ===================================
                foreach (Star star in world.getStar().Values)
                {
                    //COLLISION DETECTION FOR PROJECTILES/STARS
                    foreach (Projectile proj in world.getProjectile().Values)
                    {
                        Vector2D distanceBetween = proj.GetLocation() - star.GetLocation();
                        if ((distanceBetween.Length() < settings.StarSize) && proj.GetActive())
                        {
                            proj.SetAlive(false);
                        }
                    }

                    frameString.Append(JsonConvert.SerializeObject(star) + '\n');
                }


                //PROJECTILES ==============================
                Dictionary<int, Projectile> projectiles = world.getProjectile().ToDictionary(entry => entry.Key,
                                               entry => entry.Value); //copy of projectiles so dead projectiles can be removed
                foreach (Projectile proj in projectiles.Values)
                {
                    Vector2D velocity = proj.GetDirection() * settings.ProjectileSpeed;
                    proj.SetLocation(proj.GetLocation() + velocity);


                    if (proj.GetLocation().Length() > world.getWorldSize()) //checks distance from world
                    {

                        proj.SetAlive(false); //if to far away, it is tagged for deletion

                    }

                    frameString.Append(JsonConvert.SerializeObject(proj) + '\n');

                    world.Update(proj); // gives world updated location and deletes tagged projectiles

                }




                //Check to see if players are still connected. if not, they are removed from the list of sockets
                foreach (SocketState ss in sockets.Values)
                {
                    if (!Networking.Send(ss.theSocket, frameString.ToString()))
                    {
                        deadSocketsIDs.Push(ss.ID);
                    }
                }

                for (int socket = 0; socket < deadSocketsIDs.Count; socket++)
                {
                    int ID = deadSocketsIDs.Pop();
                    sockets.Remove(ID);
                    if(world.getShips().ContainsKey(ID))
                        world.GetShip(ID).SetHp(0);

                    Console.Out.WriteLine("Player " + players[ID].GetName() + " has disconnected.");


                }
            }


        }


        /// <summary> 
        /// Passed a ship and determines if it is still on the world map.
        /// If a ship has gone off the map, it is sent to the other side of the map.
        /// </summary>
        /// <param name="ship">ship to check</param>
        /// <returns>the ship that has been wrapped around the map if needed</returns>
        public Ship WithinBounds(Ship ship)
        {
            int size = (world.getWorldSize() / 2) - 1; //adjusts world size so that 0 is in the middle of the map
            Vector2D location = ship.GetLocation();
            if (location.GetX() > size) //far right side
            {
                Vector2D newLocation = new Vector2D((-1 * location.GetX() + 1), location.GetY());
                ship.SetLocation(newLocation);
            }
            if (location.GetX() < (-1 * size))//far left side
            {
                Vector2D newLocation = new Vector2D((-1 * location.GetX() - 1), location.GetY());
                ship.SetLocation(newLocation);
            }
            if (location.GetY() > size)//top of map
            {
                Vector2D newLocation = new Vector2D(location.GetX(), (-1 * location.GetY()) + 1);
                ship.SetLocation(newLocation);
            }
            if (location.GetY() < (-1 * size)) //bottom of map
            {
                Vector2D newLocation = new Vector2D(location.GetX(), -1 * location.GetY() - 1);
                ship.SetLocation(newLocation);
            }
            return ship;
        }

        /// <summary>
        /// Same as WithinBounds for a ship, but adjusted given the larger size of the star graphic
        /// </summary>
        /// <param name="star">star to check</param>
        /// <returns></returns>
        public Star WithinBounds(Star star)
        {
            int size = (world.getWorldSize() / 2) - 1;
            Vector2D location = star.GetLocation();
            if (location.GetX() > size)
            {
                Vector2D newLocation = new Vector2D((-1 * location.GetX() + 10), location.GetY());
                star.SetLocation(newLocation);
            }
            if (location.GetX() < (-1 * size))
            {
                Vector2D newLocation = new Vector2D((-1 * location.GetX() - 10), location.GetY());
                star.SetLocation(newLocation);
            }
            if (location.GetY() > size)
            {
                Vector2D newLocation = new Vector2D(location.GetX(), (-1 * location.GetY()) + 10);
                star.SetLocation(newLocation);
            }
            if (location.GetY() < (-1 * size))
            {
                Vector2D newLocation = new Vector2D(location.GetX(), -1 * location.GetY() - 10);
                star.SetLocation(newLocation);
            }
            return star;
        }



        /// <summary>
        /// This method sets the new callMe to ReceiveName and starts the event loop again to get data
        /// </summary>
        /// <param name="ss"></param>
        public void HandleNewClient(SocketState ss)
        {
            ss.callMe = ReceiveName;
            Networking.GetData(ss);

        }

        /// <summary>
        /// This method is invoked when the socket state callme is set to this method.
        /// Receives a string with the client's name terminated by a newline per the SpaceWars
        /// protocol. Sends the client's id then the world size, both terminated by newlines to
        /// the client.
        /// Sets the socket state's callme to RecieveCommands and starts the event loop by calling
        /// GetData
        /// </summary>
        /// <param name="ss"></param>
        public void ReceiveName(SocketState ss)
        {
            Console.WriteLine("New Player Has Connected.");
            string totalData = ss.sb.ToString();
            if (totalData == null || totalData.Length == 0)
            {
                Console.WriteLine("Bad/incorrect data recieved from client, terminating connection");
                ss.theSocket.Close();
            }

            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            string trimmedString = parts[0].Replace("\n", "");

            if(parts.Length == 0)
            {
                Console.WriteLine("Client not following protocol, terminating connection");
                ss.theSocket.Close();
            }

            ss.ID = CreatePlayer(trimmedString);
            playerCount++;

            lock (world)
                sockets.Add(ss.ID, ss);


            Networking.Send(ss.theSocket, ss.ID.ToString() + '\n' + world.getWorldSize().ToString() + '\n');
            // Then remove it from the SocketState's growable buffer
            ss.sb.Remove(0, parts[0].Length);


            ss.callMe = ReceiveCommands;
            Networking.GetData(ss);

        }

        /// <summary>
        /// This method is called anytime data is recieved from the server, after
        /// contact has been made with the server and the world size and playerid set.
        /// This method will parse the data according to the SpaceWars protocol and update
        /// theWorld accordingly provided that at least one full JSon object is recieved from the server.
        /// </summary>
        /// <param name="ss"></param>
        public void ReceiveCommands(SocketState ss)
        {

            string totalData = ss.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                {
                    continue;
                }
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;
                lock (players)
                {

                    //Scans the string to look for requests.
                    //if a request is true, then the server will use it the next time the Update function is ran, which
                    //sets everything back to false;

                    if (p.Contains("T"))
                    {
                        players[ss.ID].SetRecentThrust(true);
                    }
                    if (p.Contains("F"))
                    {
                        players[ss.ID].SetRecentProjectile(true);
                    }
                    if (p.Contains("L"))
                    {
                        players[ss.ID].SetRecentLeft(true);
                    }
                    if (p.Contains("R"))
                    {
                        players[ss.ID].SetRecentRight(true);
                    }
                }

                // Then remove it from the SocketState's growable buffer
                ss.sb.Remove(0, p.Length);

            }

            try
            {
                Networking.GetData(ss);
            }
            catch (Exception)
            {
                //                connectionLost = true;
            }

        }

        /// <summary>
        /// Create player generates a new ship in a valid location (not too close to the sun or another player)
        /// Sets the HP of the player to the correct starting HP, and then adds the ship to the world. 
        /// 
        /// The player is then added to the list of all players
        ///
        /// </summary>
        /// <param name="name">name of player</param>
        /// <returns>player ID</returns>
        public int CreatePlayer(string name)
        {
            lock (world)
            {
                gameStarted = true;
                Console.WriteLine("Creating Player: " + name + "\n");

                getRandomLocAndDir(out Vector2D location, out Vector2D direction);
                
                
                Ship ship = new Ship(playerCount, name, location, direction);

                ship.SetHp(settings.StartingHitPoints);

                world.Update(ship);

                players.Add(playerCount, new Player(name, playerCount, settings.FramesPerShot));

            }

            return playerCount; 
        }


        /// <summary>
        /// Ran each time a new player joins the game or a dead ship needs to respawn.
        /// 
        /// Ships location and direction is randomly generated and then passed to the IsGoodLocation function to make sure
        /// the ship isn't too close to stars or other players. If it is too close, a new location is randomly generated until
        /// it is good.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="direction"></param>
        private void getRandomLocAndDir(out Vector2D location, out Vector2D direction)
        {
            Random rand = new Random();
            int worldSize = world.getWorldSize();
            rand.NextDouble();

            location = new Vector2D((worldSize / 2) - rand.NextDouble() * worldSize, (worldSize / 2) - rand.NextDouble() * worldSize);
            bool goodLocation = IsGoodLocation(location);

            while (!goodLocation) //loops until a good location is found
            {
                location = new Vector2D((worldSize / 2) - rand.NextDouble() * worldSize, (worldSize / 2) - rand.NextDouble() * worldSize);
                goodLocation = IsGoodLocation(location);
            }

            direction = new Vector2D((rand.NextDouble() * 2) - 1, (rand.NextDouble() * 2 - 1));
        }

        /// <summary>
        /// helper function that determines if the spawn location for a ship is too close to a star or another ship.
        /// If it is, function will return a false
        /// </summary>
        /// <param name="location">Spawn location of new ship</param>
        /// <returns>true if location is good, false if it is too close to another object</returns>
        public bool IsGoodLocation(Vector2D location)
        {
            int worldSize = world.getWorldSize();


            //Compare location to all stars
            foreach (Star star in world.getStar().Values)
            {
                if ((location - star.GetLocation()).Length() < worldSize / 5)
                {
                    return false;
                }
            }

            //compare location to all other ships
            foreach (Ship ship in world.getShips().Values)
            {
                if ((location - ship.GetLocation()).Length() < worldSize / 10)
                {
                    return false;
                }
            }
            return true;

        }


















        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************
        ///*************PS9 CODE****************










        /// <summary>
        /// The connection string.
        /// Your uID login name serves as both your database name and your uid
        /// </summary>
        public const string connectionString = "server=atr.eng.utah.edu;" +
          "database=cs3500_u0323727;" +
          "uid=cs3500_u0323727;" +
          "password=CS3500SecurePassword";









     

        /// <summary>
        /// Called as soon as the server admin enters stop.
        /// Stats are sent to a SQL database
        /// </summary>

        public void SendStats(System.TimeSpan duration)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    // Open a connection
                    conn.Open();

                    // Adds Duration to the Duration database which will generate an auto-incremented game ID
                    MySqlCommand command = conn.CreateCommand();
                    command.CommandText = "INSERT INTO Duration(duration) VALUES('" + duration + "');";

                    command.ExecuteNonQuery();



                    //grabs the game ID from the Duration table
                    command.CommandText = "SELECT max(gameID) FROM Duration;";
                    int gameID = Convert.ToInt32(command.ExecuteScalar());

                    //Runs through each player and sends it to the Players table. a unique id is generated to track
                    //individual stats. unique ID is added to the GameTrack table to determine which unique IDs played
                    //in a game so that the correct stats can be found for that game
                    foreach (Player play in players.Values)
                    {
                        string commandText = "INSERT INTO Players(playerName, score, accuracy) VALUES('" + play.GetName() + "', "
                            + play.GetScore() + ", ";
                        if (play.GetShotsFired() == 0) //check to ensure no division by zero if players never fired a shot
                            commandText = commandText + "0);";
                        else
                            commandText = commandText += (double)play.GetScore()/(double)play.GetShotsFired() + ");";

                        command.CommandText = commandText;
                        command.ExecuteNonQuery();
                        command.CommandText = "SELECT max(uniqueID) FROM Players;"; //grabs the unique ID
                        int uniqueID = Convert.ToInt32(command.ExecuteScalar());


                        //Takes the gameID and adds all players who have played in that game
                        command.CommandText = "INSERT INTO GameTracker(gameID, uniqueID) VALUES(" + gameID.ToString() + ", "
                                                + uniqueID + ");"; 

                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }


        }



        // The server must follow the HTTP protocol when replying to a client (web browser).
        // That involves sending an HTTP header before any web content.

        // This is the header to use when the server recognizes the browser's request
        private const string httpOkHeader =
          "HTTP/1.1 200 OK\r\n" +
          "Connection: close\r\n" +
          "Content-Type: text/html; charset=UTF-8\r\n" +
          "\r\n";

        // This is the header to use when the server does not recognize the browser's request
        private const string httpBadHeader =
          "HTTP/1.1 404 Not Found\r\n" +
          "Connection: close\r\n" +
          "Content-Type: text/html; charset=UTF-8\r\n" +
          "\r\n";



        /// <summary>
        /// This is the delegate for when a new socket is accepted
        /// The Networking_Support library will invoke this method when a browser connects
        /// </summary>
        /// <param name="state"></param>
        public static void HandleHttpConnection(SocketState state)
        {
            // Before receiving data from the browser, we need to change what we do when network activity occurs.
            state.callMe = ServeHttpRequest;

            Networking.GetData(state);
        }


        /// <summary>
        /// This method parses the HTTP request sent by the broswer, and serves the appropriate web page.
        /// </summary>
        /// <param name="state">The socket state representing the connection with the client</param>
        private static void ServeHttpRequest(SocketState state)
        {
            Regex regex = new Regex(".*?HTTP/1.1");


            var v = regex.Match(state.sb.ToString());
            string request = v.Groups[0].ToString();


            // Print it for debugging/examining
            Console.WriteLine("received http request: " + request);


            // If the browser requested the home page (e.g., localhost/)
            if (request.Contains("GET / HTTP/1.1"))
            {
                Networking.SendAndClose(state.theSocket, httpOkHeader + "<head><title>SpaceWars!</title></head>\r\n"
                + "<body><center><h1>SpaceWars!</h1>\r\n"
                + "<body><h2>Contents</h2>\r\n"





                + "<h2>For Overall Stats, click <a href=\"http://localhost/scores \" > here</a></h2><br><br>\r\n"
                + "For stats on a single game or player, enter the game ID and hit send:<br>\r\n"

                + "<script type=\"text/javascript\">\r\n" //Javascript to do very basic input error correction

     + "<!--\r\n"
         + "function Validator()\n"
        + "{\r\n"
                    + "if (text_form.gameID.value && text_form.playerName.value)\r\n"
                    + "{\n"
                        + "if (isNaN(text_form.gameID.value))\n"
                        + "{\n"
                            + "alert(\"Error: Game ID must be a number\");\n"
                        + "}\n"
                        + "else\n"
                        + "{\n"

                            + "window.location.href='http://localhost/game?id=' + text_form.gameID.value + '&player=' + text_form.playerName.value;\n"
                        + "}\n"
                   + " }\n"
                    + "else if (text_form.gameID.value)\n"
                    + "{\n"
                        + "if (isNaN(text_form.gameID.value))\n"
                + "{\n"
                + "alert(\"Error: Game ID must be a number\");\n"
                        + "}\n"
                        + "else\n"
                        + "{\n"
                            + "window.location.href='http://localhost/game?id=' + text_form.gameID.value;\n"
                        + "}\n"

                    + "}\n"
                    + "else if (text_form.playerName.value)\n"
                    + "{\n"
                        + "window.location.href='http://localhost/games?player=' + text_form.playerName.value;\n"

                    + "}\n"

                + "}\n"
                + "-->\n"
        + "</script>\n"
                + "<form name=\"text_form\" method=\"get\">\n"
           + "<p> Game ID <input type = \"text\" name = \"gameID\">\n"
                 + "Player Name: <input type=\"text\" name=\"playerName\">\n"

                        + "<input type=\"button\" value=\"Submit\" onClick=\"Validator()\"></ p >\n"
                         + "</form>\n"











                + "</center></body></html>\r\n");

            }
            else if (request.Contains("GET /scores HTTP/1.1"))
            {
                string send = httpOkHeader + "<head><title>SpaceWars!</title></head>\r\n"
                    + "<body><h1>SpaceWars!</h1>"
                    + "<body><h2>Overall Server Stats</h2>";

                // Connect to the DB
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        // Open a connection
                        conn.Open();

                        // Create a command
                        MySqlCommand command = conn.CreateCommand();
                        command.CommandText = "select * from GameTracker natural join Players natural join Duration ORDER BY gameID ASC;";

                        int gameID = -1;
                        bool first = true;
                        
                        // Execute the command and cycle through the DataReader object
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int readID = Convert.ToInt32(reader["gameID"]);
                                if (readID != gameID)
                                {
                                    gameID = readID;
                                    if (!first)
                                        send = send + "</table>";
                                    first = false;
                                    send = send + "<h2>Stats for Game " + gameID + "</h2>\n " +
                                        "Duration of " + reader["duration"] + "<table border=1>\r\n"
                                        + "<tr><td><b>Player Name</b></td><td><b>Score</b></td><td><b>Accuracy</b></td></tr>";
                                }
                                string name = "" +reader["playerName"];
                                send = send + "<tr><td><a href=http://localhost/games?player=" + name +">" + name + "</td><td> " + reader["score"] 
                                    + "</td><td>" + String.Format("{0:0.##}%", Convert.ToDouble(reader["accuracy"])) 
                                    + "</td></tr>";
                            }
                            send = send + "</table>";
                        }
                        

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                send = send+ "<br><br>"
                    + "Click <a href=\"http://localhost\">here</a> to go back.<br><br>\r\n";

                Networking.SendAndClose(state.theSocket, send);


            }
            else if (request.Contains("GET /games") && request.Contains("HTTP/1.1"))
            {

                regex = new Regex("player=(.*?) HTTP");
                v = regex.Match(request);
                string requestName = v.Groups[1].ToString();

                string send = httpOkHeader + "<head><title>SpaceWars!</title></head>\r\n"
                    + "<body><h2>SpaceWars!</h2>\r\n";
                    



                // Connect to the DB
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        // Open a connection
                        conn.Open();
                        MySqlCommand command = conn.CreateCommand();


                        command.CommandText = "Select count(*) from Players where playerName='" + requestName + "';";
                        int entriesFound = Convert.ToInt32(command.ExecuteScalar());

                        if (entriesFound > 0)
                        {






                            // Create a command

                            command.CommandText = "select * from GameTracker natural join Players natural join Duration WHERE playerName='" + requestName + "' ORDER BY gameID ASC;";

                            // Execute the command and cycle through the DataReader object
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                send = send + "<body><h1>" + requestName + "'s Stats</h1>\r\n";
                                send = send + "<table border=1>\r\n<tr><td><b>Game</b></td><td><b>Score</b></td><td><b>Accuracy</b></td></tr>\r\n";

                                while (reader.Read())
                                {



                                    int gameID = Convert.ToInt32(reader["gameID"]);
                                    send = send + "<tr><td><a href='http://localhost/game?id=" + gameID + "'>" + gameID + "</td><td> " + reader["score"]
                                        + "</td><td>" + String.Format("{0:0.##}%", Convert.ToDouble(reader["accuracy"]))
                                        + "</td></tr>\r\n";
                                }
                                send = send + "</table><br><br>\r\n";
                            }
                        }
                        else
                        {
                            send = send + requestName + " was not found. Try another name or click below to return to the front page.<br><br>\r\n";
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }








                send = send + "Click <a href=\"http://localhost\">here</a> to go back.<br><br>\r\n";

                Networking.SendAndClose(state.theSocket, send);




            }

            //if the search contains game, it will determine if the user is looking for both an ID and a player
            //or just one
            else if (request.Contains("GET /game?") && request.Contains("HTTP/1.1"))
            {

                if (request.Contains("&player="))
                {
                    regex = new Regex("id=(.*?)(&player=)(.*?) HTTP");
                    v = regex.Match(request);
                    string requestID = v.Groups[1].ToString();
                    string requestName = v.Groups[3].ToString();

                    string send = httpOkHeader + "<head><title>SpaceWars!</title></head>\r\n"
                        + "<body><h2>SpaceWars!</h2>"
                        + "<body><h1>" + requestName + "'s Stats in Game " + requestID + "</h1>";


                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            // Open a connection
                            conn.Open();
                            MySqlCommand command = conn.CreateCommand();


                            command.CommandText = "Select count(*) from Players where playerName='" + requestName + "';";
                            int entriesFound = Convert.ToInt32(command.ExecuteScalar());

                            if (entriesFound > 0)
                            {
                                command.CommandText = "Select count(*) from Duration where gameID=" + requestID + ";";
                                entriesFound = Convert.ToInt32(command.ExecuteScalar());

                                if (entriesFound > 0)
                                {


                                    // Create a command

                                    command.CommandText = "select * from GameTracker natural join Players natural join Duration WHERE playerName='" + requestName + "' ORDER BY gameID ASC;";

                                // Execute the command and cycle through the DataReader object
                                using (MySqlDataReader reader = command.ExecuteReader())
                                {
                                    send = send + "<table border=1>\r\n<tr><td><b>Game</b></td><td><b>Score</b></td><td><b>Accuracy</b></td></tr>\r\n";

                                    while (reader.Read())
                                    {
                                        send = send + "<tr><td>" + reader["gameID"] + "</td><td>" + reader["score"] + "</td><td>" + String.Format("{0:0.##}%", Convert.ToDouble(reader["accuracy"]))
                                            + "</td></tr>\r\n";
                                    }
                                    send = send + "</table><br><br>\r\n";
                                }
                                }
                                else
                                {
                                    send = send + requestID + " was not found. Try another game number or click below to return to the front page.<br><br>";

                                }
                            }
                            else
                            {
                                send = send + requestName + " was not found. Try another name or click below to return to the front page.<br><br>\r\n";

                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }




                    send = send + "Click <a href=\"http://localhost\">here</a> to go to the front page.<br><br>\r\n";

                    Networking.SendAndClose(state.theSocket, send);

                }
                else
                {
                    regex = new Regex("id=(.*?) HTTP");
                    v = regex.Match(request);
                    string requestID = v.Groups[1].ToString();


                    string send = httpOkHeader + "<head><title>SpaceWars!</title></head>\r\n"
                        + "<body><h2>SpaceWars!</h2>"
                        + "<body><h1>Stats for Game " + requestID + " </h1>";


                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            // Open a connection
                            conn.Open();

                            // Create a command
                            MySqlCommand command = conn.CreateCommand();

                            command.CommandText = "Select count(*) from Duration where gameID=" + requestID + ";";
                            int entriesFound = Convert.ToInt32(command.ExecuteScalar());

                            if (entriesFound > 0)
                            {


                                command.CommandText = "select * from GameTracker natural join Players natural join Duration WHERE gameID=" + requestID + " ORDER BY gameID ASC;";

                                // Execute the command and cycle through the DataReader object
                                using (MySqlDataReader reader = command.ExecuteReader())
                                {
                                    send = send + "<table border=1>\r\n<tr><td><b>Player</b></td><td><b>Score</b></td><td><b>Accuracy</b></td></tr>\r\n";

                                    while (reader.Read())
                                    {



                                        string name = "" + reader["playerName"];

                                        send = send + "<tr><td><a href=http://localhost/games?player=" + name + ">" + name + "</td><td> " + reader["score"]
                                            + "</td><td>" + String.Format("{0:0.##}%", Convert.ToDouble(reader["accuracy"]))
                                            + "</td></tr>\r\n";
                                    }
                                    send = send + "</table><br><br>\r\n";
                                }
                            }
                            else
                            {
                                send = send + requestID + " was not found. Try another game number or click below to return to the front page.<br><br>";

                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }





                    send = send + "Click <a href=\"http://localhost\">here</a> to go to the front page.<br><br>\r\n";

                    Networking.SendAndClose(state.theSocket, send);

                }
            }

            // Otherwise, our very simple web server doesn't recognize any other URL
            else
                Networking.SendAndClose(state.theSocket, httpBadHeader + "<h2>page not found</h2>"
                                        + "Click <a href=\"http://localhost\">here</a> to go to the front page.<br><br>\r\n");


            Networking.ServerAwaitingClientLoop(HandleHttpConnection, Networking.HTTP_PORT);
        }

        // NOTE: The above SendAndClose calls are an addition to the Networking_Support_Support library.
        //       The only difference from the basic Send method is that this method uses a callback
        //       that closes the socket after ending the send.
















    }




    /// <summary>
    /// This class is a wrapper class for all of the modifiable settings for the SpaceWars server.
    /// This is an immutable class.
    /// </summary>
    public class Settings
    {
        //Data that is modifiable on the sample server
        int worldSize;
        int msPerFrame;
        int framesPerShot;
        int respawnRate;

        //Basic data
        int startingHitPoints;
        int projectileSpeed; //Units per frame
        double engineStrength; //Units per frame
        double turningRate; //Degrees
        int shipSize; //Radius
        int starSize; //Radius
        bool movingStars; //Extra feature


        public Settings(int worldSize, int MSPerFrame, int FramesPerShot, int RespawnRate, int startingHitPoints,
            int projectileSpeed, double engineStrength, double turningRate, int shipSize, int starSize, bool movingStars)
        {
            this.worldSize = worldSize;
            this.msPerFrame = MSPerFrame;
            this.framesPerShot = FramesPerShot;
            this.respawnRate = RespawnRate;

            this.startingHitPoints = startingHitPoints;
            this.projectileSpeed = projectileSpeed;
            this.engineStrength = engineStrength;
            this.turningRate = turningRate;
            this.shipSize = shipSize;
            this.starSize = starSize;
            this.movingStars = movingStars;
        }

        public int WorldSize { get => worldSize; }
        public int MSPerFrame { get => msPerFrame; }
        public int FramesPerShot { get => framesPerShot; }
        public int RespawnRate { get => respawnRate; }

        public int StartingHitPoints { get => startingHitPoints; }
        public int ProjectileSpeed { get => projectileSpeed; }
        public double EngineStrength { get => engineStrength; }
        public double TurningRate { get => turningRate; }
        public int ShipSize { get => shipSize; }
        public int StarSize { get => starSize; }
        public bool MovingStars { get => movingStars; }
    }
}
