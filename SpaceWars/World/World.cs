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

namespace SpaceWars
{
    /// <summary>
    /// A world is a container for ships, players, projectiles, stars and explosions along with 
    /// the world size.
    /// </summary>
    public class World
    {
        private Dictionary<int, Ship> Ships; //tracks ships that will be drawn
        private Dictionary<int, Projectile> Projectiles;
        private Dictionary<int, Star> Stars;

        //        private PlayerList sortedPlayerList;
        private List<Ship> sortedPlayerList;


        private Dictionary<int, Explosion> Explosions;
        private int worldSize;

        /// <summary>
        /// Default and only constructor for a world object.
        /// Creates empty containers for ships, projectiles, stars, players and explosions.
        /// Sets the world size to 0.
        /// </summary>
        public World()
        {
            Ships = new Dictionary<int, Ship>();
            Projectiles = new Dictionary<int, Projectile>();
            Stars = new Dictionary<int, Star>();
            sortedPlayerList = new List<Ship>();

            Explosions = new Dictionary<int, Explosion>();
            worldSize = 0;
        }

        /// <summary>
        /// Default and only constructor for a world object.
        /// Creates empty containers for ships, projectiles, stars, players and explosions.
        /// Sets the world size to worldsSize.
        /// </summary>
        /// <param name="worldSize"> World size</param>
        public World(int worldSize)
        {
            Ships = new Dictionary<int, Ship>();
            Projectiles = new Dictionary<int, Projectile>();
            Stars = new Dictionary<int, Star>();
            sortedPlayerList = new List<Ship>();

            Explosions = new Dictionary<int, Explosion>();
            this.worldSize = worldSize;
        }

        /// <summary>
        /// Returns a dictionary will all of the ships in this world.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Ship> getShips()
        {
            return Ships;
        }

     
        /// <summary>
        /// Returns a dictionary will all of the explosions in this world.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Explosion> getExplosions()
        {
            return Explosions;
        }

        /// <summary>
        /// Returns a dictionary will all of the projectiles in this world.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Projectile> getProjectile()
        {
            return Projectiles;
        }

        /// <summary>
        /// Returns a dictionary will all of the stars in this world.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Star> getStar()
        {
            return Stars;
        }

        /// <summary>
        /// Returns the specified ship from the world
        /// </summary>
        /// <param name="id"> ID of the requested ship </param>
        /// <returns></returns>
        public Ship GetShip(int id)
        {
            return Ships[id];
        }
        /// <summary>
        /// Returns this world's size
        /// </summary>
        /// <returns></returns>
        public int getWorldSize()
        {
            return worldSize;
        }

        /// <summary>
        /// Sets the size of the world
        /// </summary>
        /// <param name="size"></param>
        public void setWorldSize(int size)
        {
            worldSize = size;
        }

        /// <summary>
        /// Updates the the stars dictionary to contain the given star
        /// </summary>
        /// <param name="star"> Star to be added</param>
        public void Update(Star star)
        {
            Stars[star.getID()] = star;
        }

        /// <summary>
        /// Updates the the ships dictionary to contain the given ship
        /// </summary>
        /// <param name="ship"></param>
        public void Update(Ship ship)
        {
            int id = ship.GetShipID();

            if (ship.GetHp() == 0)
            {
                Ships.Remove(ship.GetShipID());
                if (!Explosions.ContainsKey(ship.GetShipID()))
                {
                    Explosions.Add(id, new Explosion(id, ship.GetLocation().GetX(), ship.GetLocation().GetY()));
                }
                
                foreach (Ship s in sortedPlayerList)
                {
                    if (s.GetShipID()==ship.GetShipID())
                        s.UpdateShip(ship);
                }
                
            }

            else
            {
                int oldScore = -1;

                if (Explosions.ContainsKey(id) && Explosions[id].Dead)
                {
                    Explosions.Remove(id);
                }



                bool found = false;

                foreach (Ship s in sortedPlayerList)
                {
                    if (s.GetShipID() == ship.GetShipID())
                    {
                        found = true;
                        oldScore = s.GetScore();

                        s.UpdateShip(ship);
                        break;
                    }
                }

                Ships[id] = ship;

                if (!found)
                {
                    sortedPlayerList.Add(Ships[ship.GetShipID()]);
                    sortedPlayerList.Sort((a, b) => b.GetScore().CompareTo(a.GetScore()));

                }
                if (found && (ship.GetScore() > oldScore))

                    sortedPlayerList.Sort((a, b) => b.GetScore().CompareTo(a.GetScore()));
            }
        }


        public List<Ship> GetList()
        {
            return sortedPlayerList;
        }

        /// <summary>
        /// Updates the the projectiles dictionary to contain the given projectile
        /// </summary>
        /// <param name="proj"></param>
        public void Update(Projectile proj)
        {
            if (!proj.GetActive())
                Projectiles.Remove(proj.GetID());
            else
                Projectiles[proj.GetID()] = proj;
        }

        /// <summary>
        /// Updates the the explosions dictionary to remove the given explosion if dead
        /// </summary>
        /// <param name="explosion"></param>
        public void Update(Explosion explosion)
        {
            if (explosion.Dead)
            {
                Explosions.Remove(explosion.getID());
            }
        }
    }
}
