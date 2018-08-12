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

    public class PlayerPanel : Panel
    {

        private World theWorld;


        public PlayerPanel(World w)
        {
            DoubleBuffered = true;
            theWorld = w;
        }

         
        /// <summary>
        /// Draws a box in the right hand side of the screen that will display a players name, score, and hp.
        /// Count determines where on the screeen the box will be drawn. Each box is 50 pixels high, so the
        /// height of each box is 50 times count, where count begins at 0 for the first player

        /// </summary>
        /// <param name="ship">ship that contains the player information</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        /// <param name="count">tracks how many player boxes have been drawn</param>
        private void PlayerDrawer(PaintEventArgs e, Ship ship, int count)
        {

            int height = 50 * count; // determines how far down to draw each player box

            string name = ship.GetName() + ": " + ship.GetScore().ToString();

            using (System.Drawing.SolidBrush whiteBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            using (System.Drawing.SolidBrush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                e.Graphics.DrawString(name, new Font("Arial", 12), blackBrush, 10, 2 + height);

                // Rectangles are drawn starting from the top-left corner.
                // So if we want the rectangle centered on the player's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(5, (20 + height), 188, 20);
                Rectangle r2 = new Rectangle(7, (22 + height), 184, 16);
                Rectangle r3 = new Rectangle(9, (24 + height), 36 * ship.GetHp(), 12);
                e.Graphics.FillRectangle(blackBrush, r);
                e.Graphics.FillRectangle(whiteBrush, r2);
                e.Graphics.FillRectangle(greenBrush, r3);
            }
        }

        /// <summary>
        /// this method is invoked when the panel needs to be redrawn. 
        /// Runs through each ship in the sorted player list and draws it
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (theWorld)
            {
                int count = 0;
                foreach (Ship ship in theWorld.GetList())
                {
                    PlayerDrawer(e, ship, count);
                    count++;
                }

            }

            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

    }
}
