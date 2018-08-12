using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceWars
{
    public class DrawingPanel : Panel
    {
        Image[] shipImages = new Image[24];//stores all ship image files
        Image star; // stores the star image
        Pen whitePen = new Pen(Brushes.White);

        private World theWorld;


        public DrawingPanel(World w)
        {
            DoubleBuffered = true;
            theWorld = w;
            LoadImages();

        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // Perform the transformation
            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            // Draw the object 
            drawer(o, e);
            // Then undo the transformation
            e.Graphics.ResetTransform();
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method.
        /// Draws a ship with the images that are loaded in the LoadImages() method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ShipDrawer(object o, PaintEventArgs e)
        {
            Ship ship = o as Ship;

            int width = 35;
            int height = 35;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
            int shipID = ship.GetShipID() % 8;

            if (!ship.GetThrust())
                e.Graphics.DrawImage(shipImages[shipID], r);
            else
                e.Graphics.DrawImage(shipImages[shipID + 8], r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// Draws a projectile accroding to the projectile images in the LoadImages method.
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile p = o as Projectile;

            int width = 20;
            int height = 20;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the projectiles's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            Rectangle r = new Rectangle(-(width / 2), -height, width, height);

            int colorID = (p.GetOwner() % 8) + 16;
            e.Graphics.DrawImage(shipImages[colorID], r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method.
        /// Draws a star according to the star image loaded in LoadImages file
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void StarDrawer(object o, PaintEventArgs e)
        {
            Star s = o as Star;

            int width = 50;
            int height = 50;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // Rectangles are drawn starting from the top-left corner.
            // So if we want the rectangle centered on the player's location, we have to offset it
            // by half its size to the left (-width/2) and up (-height/2)
            Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

            e.Graphics.DrawImage(star, r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// Draws an explosion as a circle with raidus passed in as o.
        /// </summary>
        /// <param name="o">The radius of the explosion</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            int radius = (int)o;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

            Rectangle r = new Rectangle(-(radius / 2), -(radius / 2), radius, radius);
            e.Graphics.DrawEllipse(whitePen, r);
        }

        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn, it will
        /// traverse through the ship, projectile, star and explosion structures in theWorld
        /// and redraw them using DrawWithTransform and the respective drawing delegates
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (theWorld)
            {
                // Draw the players
                foreach (Ship ship in theWorld.getShips().Values)
                {
                    DrawObjectWithTransform(e, ship, this.Size.Width, ship.GetLocation().GetX(), ship.GetLocation().GetY(), ship.GetDirection().ToAngle(), ShipDrawer);
                }

                // Draw the projectiles
                foreach (Projectile pro in theWorld.getProjectile().Values)
                {
                    DrawObjectWithTransform(e, pro, this.Size.Width, pro.GetLocation().GetX(), pro.GetLocation().GetY(), pro.GetDirection().ToAngle(), ProjectileDrawer);
                }

                foreach (Star star in theWorld.getStar().Values)
                {
                    DrawObjectWithTransform(e, star, this.Size.Width, star.GetLocation().GetX(), star.GetLocation().GetY(), 0, StarDrawer);
                }

                foreach (Explosion explosion in theWorld.getExplosions().Values)
                {
                    if (!explosion.Dead)
                    {
                        int r = explosion.Radius;
                        DrawObjectWithTransform(e, r, this.Size.Width, explosion.X_coord, explosion.Y_coord, 0, ExplosionDrawer);
                        DrawObjectWithTransform(e, r+8, this.Size.Width, explosion.X_coord, explosion.Y_coord, 0, ExplosionDrawer);
                        DrawObjectWithTransform(e, r+16, this.Size.Width, explosion.X_coord, explosion.Y_coord, 0, ExplosionDrawer);
                        explosion.UpdateRadius();
                    }
                }
            }

            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

        /// <summary>
        /// Loads all of the images needed for this panel from the Resources folder contained in SpaceWars client.
        /// </summary>
        private void LoadImages()
        {
            try
            {
                string parentLocation = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName; //gets directory of project
                string imageLocation = parentLocation + "\\Resources\\Library\\Images\\";

                shipImages[0] = Image.FromFile(imageLocation + "ship-coast-blue.png");
                shipImages[1] = Image.FromFile(imageLocation + "ship-coast-brown.png", true);
                shipImages[2] = Image.FromFile(imageLocation + "ship-coast-green.png", true);
                shipImages[3] = Image.FromFile(imageLocation + "ship-coast-grey.png", true);
                shipImages[4] = Image.FromFile(imageLocation + "ship-coast-red.png", true);
                shipImages[5] = Image.FromFile(imageLocation + "ship-coast-violet.png", true);
                shipImages[6] = Image.FromFile(imageLocation + "ship-coast-white.png", true);
                shipImages[7] = Image.FromFile(imageLocation + "ship-coast-yellow.png", true);
                shipImages[8] = Image.FromFile(imageLocation + "ship-thrust-blue.png", true);
                shipImages[9] = Image.FromFile(imageLocation + "ship-thrust-brown.png", true);
                shipImages[10] = Image.FromFile(imageLocation + "ship-thrust-green.png", true);
                shipImages[11] = Image.FromFile(imageLocation + "ship-thrust-grey.png", true);
                shipImages[12] = Image.FromFile(imageLocation + "ship-thrust-red.png", true);
                shipImages[13] = Image.FromFile(imageLocation + "ship-thrust-violet.png", true);
                shipImages[14] = Image.FromFile(imageLocation + "ship-thrust-white.png", true);
                shipImages[15] = Image.FromFile(imageLocation + "ship-thrust-yellow.png", true);
                shipImages[16] = Image.FromFile(imageLocation + "shot-blue.png", true);
                shipImages[17] = Image.FromFile(imageLocation + "shot-brown.png", true);
                shipImages[18] = Image.FromFile(imageLocation + "shot-green.png", true);
                shipImages[19] = Image.FromFile(imageLocation + "shot-grey.png", true);
                shipImages[20] = Image.FromFile(imageLocation + "shot-red.png", true);
                shipImages[21] = Image.FromFile(imageLocation + "shot-violet.png", true);
                shipImages[22] = Image.FromFile(imageLocation + "shot-white.png", true);
                shipImages[23] = Image.FromFile(imageLocation + "shot-yellow.png", true);

                star = Image.FromFile(imageLocation + "star.jpg", true);

            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("There was an error opening the images." +
                    "Please check the path.");
            }
        }

    }
}

