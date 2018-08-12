using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace SpaceWars
{
    /// <summary>
    /// A star is an object that contains an ID (star), location and mass.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Star
    {
        [JsonProperty]
        private int star; 
        [JsonProperty]
        private Vector2D loc;
        [JsonProperty]
        private double mass;
        private Vector2D velocity;


        /// <summary>
        /// Constuct a star with the given ID, location and mass
        /// </summary>
        /// <param name="star">ID for the object</param>
        /// <param name="loc">Vector2D object's location</param>
        /// <param name="mass">The star's mass</param>
        public Star(int star, Vector2D loc, double mass)
        {
            this.star = star;
            this.loc = loc;
            this.mass = mass;
            velocity = new Vector2D(0, 0);
        }

        public Vector2D GetLocation()
        {
            return loc;
        }
        public void SetLocation(Vector2D location)
        {
            loc = location;
        }

        public Vector2D GetVelocity()
        {
            return velocity;
        }
        public void SetVelocity(Vector2D vel)
        {
            velocity = vel;
        }



        public double GetMass()
        {
            return mass;
        }

        public int getID()
        {
            return star;
        }

    }
}
