using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWars
{
    /// <summary>
    /// An explosion is an object that contains a position, radius and id.
    /// An explosion is dead after its radius is greater than 80.
    /// </summary>
    public class Explosion
    {
        private int radius;
        private double x_coord;
        private double y_coord;
        private bool dead;
        private int id;

        /// <summary>
        /// Explosion constructor
        /// </summary>
        /// <param name="id">Identification number of an explosion.</param>
        /// <param name="x_coord"> X Posistion of the explosion</param>
        /// <param name="y_coord">Y Positino of the explosion</param>
        public Explosion(int id, double x_coord, double y_coord)
        {
            Radius = 2;
            this.x_coord = x_coord;
            this.y_coord = y_coord;
            Dead = false;
            this.id = id;
        }
        
        /// <summary>
        /// Adds 2 to the radius of the explosion object.
        /// If the explosion has a raidus larger than 80, the explosion is set to dead and 
        /// the radius will no longer be updated.
        /// </summary>
        public void UpdateRadius()
        {
            if(radius < 81)
            {
                radius += 2;
            }
            else
            {
                Dead = true;
            }
        }

        /// <summary>
        /// Returns the ID of the explosion
        /// </summary>
        /// <returns></returns>
        public int getID()
        {
            return id;
        }
        
        //Properties of an explosion
        public int Radius { get => radius; set => radius = value; }
        public double X_coord { get => x_coord; set => x_coord = value; }
        public double Y_coord { get => y_coord; set => y_coord = value; }
        public bool Dead { get => dead; set => dead = value; }
    }
}
