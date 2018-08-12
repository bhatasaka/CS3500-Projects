using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using Network_Support;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpaceWars
{
    /// <summary>
    /// GameController is an object that is used to connect and recieve updates from a server
    /// and update a world object accordingly, following the SpaceWars protocol.
    /// This can also be used to send keystrokes to the server.
    /// </summary>
    public class GameController
    {
        private SocketState theServer;
        // World is a simple container for Players, projectiles, stars and explosions
        private World theWorld;
        private string playerName;
        private int playerID;
        private bool connectionLost;

        public bool Connectionlost { get => connectionLost;}

        /// <summary>
        /// Single constructor for a GameController object.
        /// </summary>
        /// <param name="world">World that this controller will be updating</param>
        public GameController(World world)
        {
            theWorld = world;
            connectionLost = false;
        }

        /// <summary>
        /// Called when the server first makes contact. Sends the playername to the server
        /// and sets the new callMe method to be RecieveStartup.
        /// </summary>
        /// <param name="ss"></param>
        public void FirstContact(SocketState ss)
        {
            ss.callMe = ReceiveStartup;
            Networking.Send(ss.theSocket, playerName);

            Networking.GetData(ss);
        }

        /// <summary>
        /// Following to the SpaceWars protocol, this method will recieve the playerID and
        /// world size as strings seperated by new line characters. This method will
        /// set this object's world size and playerID then change the callMe to 
        /// ReceiveWorld.
        /// </summary>
        /// <param name="ss"></param>
        public void ReceiveStartup(SocketState ss)
        {
            string totalData = ss.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            playerID = int.Parse(parts[0]);
            // Then remove it from the SocketState's growable buffer
            ss.sb.Remove(0, parts[0].Length);

            theWorld.setWorldSize(int.Parse(parts[1]));
            // Then remove it from the SocketState's growable buffer
            ss.sb.Remove(0, parts[1].Length);


            ss.callMe = ReceiveWorld;
            Networking.GetData(ss);
        }

        /// <summary>
        /// This method is called anytime data is recieved from the server, after
        /// contact has been made with the server and the world size and playerid set.
        /// This method will parse the data according to the SpaceWars protocol and update
        /// theWorld accordingly provided that at least one full JSon object is recieved from the server.
        /// </summary>
        /// <param name="ss"></param>
        public void ReceiveWorld(SocketState ss)
        {
            string totalData = ss.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                //MessageBox.Show(p);
                JObject obj = JObject.Parse(p);

                Deserialize("star", obj, p);
                Deserialize("ship", obj, p);
                Deserialize("proj", obj, p);

                // Then remove it from the SocketState's growable buffer
                ss.sb.Remove(0, p.Length);
            }

            try
            {
                Networking.GetData(ss);
            }
            catch (Exception)
            {
                connectionLost = true;
            }
        }

        /// <summary>
        /// Deserialize is passed a string that contains ship, star, or proj and a JSON object.
        /// Function determines if the object contains data for a ship, star, or projectille and then sends it to the world so the object can be updated.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="obj"></param>
        public void Deserialize(string s, JObject obj, string message)
        {
            JToken token = obj[s];
            if (token != null)
            {
                lock (theWorld)
                {
                    switch (s)
                    {
                        case "star":
                            theWorld.Update(JsonConvert.DeserializeObject<Star>(message));
                            break;
                        case "ship":
                            theWorld.Update(JsonConvert.DeserializeObject<Ship>(message));
                            break;
                        case "proj":
                            theWorld.Update(JsonConvert.DeserializeObject<Projectile>(message));
                            break;
                    }
                }
            }
        }



        /// <summary>
        /// Start attempting to connect to the server.
        /// Will return true if the server could be connected to.
        /// Else false.
        /// </summary>
        /// <param name="hostName"> server to connect to </param>
        /// <returns></returns>
        public bool ConnectToServer(string hostName, string playerName)
        {
            this.playerName = playerName;
            try
            {
                theServer = Networking.ConnectToServer(FirstContact, hostName);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Send keyboard commands
        /// </summary>
        /// <param name="hostName"> server to connect to </param>
        /// <returns></returns>
        public void SendInput(string input)
        {
            if (theServer != null)
            {
                Networking.Send(theServer.theSocket, "(\"" + input + "\")\n");
            }
        }
    }
}
