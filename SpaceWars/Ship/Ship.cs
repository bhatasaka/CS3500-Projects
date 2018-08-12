using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceWars
{
    /// <summary>
    /// A ship is an object that contains an ID, name, location, direction, thrust boolean, hp and score.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Ship
    {
        [JsonProperty]
        private int ship; //Ship ID
        [JsonProperty]
        private string name;
        [JsonProperty]
        private Vector2D loc;
        [JsonProperty]
        private Vector2D dir;
        [JsonProperty]
        private bool thrust;
        [JsonProperty]
        private int hp;
        [JsonProperty]
        private int score;
        private Vector2D velocity; //tracks velocity. not send to players

        /// <summary>
        /// Default constructor for Ship - necessary for JSON
        /// </summary>
        public Ship()
        {
            
        }

        /// <summary>
        /// Constructor for ship
        /// </summary>
        /// <param name="ship">Ship ID</param>
        /// <param name="name">Players Name</param>
        /// <param name="loc">Ship's Location</param>
        /// <param name="dir">Ships Direction</param>
        public Ship(int ship, string name, Vector2D loc, Vector2D dir)
        {
            this.loc = loc;
            this.dir = dir;
            this.ship = ship;
            this.name = name;
            velocity = new Vector2D(0, 0);
        }

        /// <summary>
        /// Helper function that determines if the ship is currently alive
        /// </summary>
        /// <returns>returns true if ship is alive</returns>
        public bool GetActive()
        {
            if (hp == 0)
                return false;
            else
                return true;
        }

        public int GetShipID()
        {
            return ship;
        }


        public string GetName()
        {
            return name;
        }

        public Vector2D GetLocation()
        {
            return loc;
        }
        public Vector2D GetDirection()
        {
            return dir;
        }

        public bool GetThrust()
        {
            return thrust;
        }

        public int GetHp()
        {
            return hp;
        }

        public int GetScore()
        {
            return score;
        }

        public void UpdateShip(Ship s)
        {
            hp = s.GetHp();
            score = s.GetScore();
        }

        public void SetLocation(Vector2D l)
        {
            loc = l;
        }
        public void SetDirection(Vector2D d)
        {
            dir = d;
        }

        public void SetThrust(bool t)
        {
            thrust = t;
        }

        public void SetHp(int h)
        {
            hp = h;
        }

        public void SetScore(int s)
        {
            score = s;
        }

        public void UpdateLocation(Vector2D acc)
        {
            velocity = velocity + acc;
            loc = loc + velocity;
        }

        public void Rotate(int num, double turnRate)
        {
            if(dir.Length() != 0)
                dir.Normalize();
            dir.Rotate(num * turnRate);
        }
    }
}
