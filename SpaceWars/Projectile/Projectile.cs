using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty]
        private int proj;
        [JsonProperty]
        private Vector2D loc;
        [JsonProperty]
        private Vector2D dir;
        [JsonProperty]
        private bool alive;
        [JsonProperty]
        private int owner;

        /// <summary>
        /// Default constructor for projectile - necessary for JSON
        /// </summary>
        public Projectile()
        {

        }

        /// <summary>
        /// Make a projectile object with the given parameters.
        /// </summary>
        /// <param name="projID">The projectile's ID</param>
        /// <param name="owner">ID of the projectile's owner</param>
        /// <param name="loc">The projectile's location</param>
        /// <param name="dir">The projectile's direction</param>
        public Projectile(int projID, int owner, Vector2D loc, Vector2D dir)
        {
            this.loc = loc;
            this.dir = dir;
            this.proj = projID;
            this.owner = owner;
            alive = true;
        }

        /// <summary>
        /// Returns true if the object is alive, else false.
        /// </summary>
        /// <returns></returns>

        public bool GetActive()
        {
            return alive;
        }

        public int GetID()
        {
            return proj;
        }

        public Vector2D GetLocation()
        {
            return loc;
        }
        public Vector2D GetDirection()
        {
            return dir;
        }

        public int GetOwner()
        {
            return owner;
        }

        public void SetLocation(Vector2D l)
        {
            loc = l;
        }
        public void SetDirection(Vector2D d)
        {
            dir = d;
        }

        public void SetAlive(bool a)
        {
            alive = a;
        }

    }
}
