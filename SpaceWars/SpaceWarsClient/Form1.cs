using Network_Support;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;

namespace SpaceWars
{
    public partial class Form1 : Form
    {
        // World is a simple container for Players and Powerups
        private World theWorld;

        GameController gc;

        DrawingPanel drawingPanel;
        PlayerPanel playerPanel;
        System.Timers.Timer frameTimer;


/// <summary>
/// When a player hits a key, a corresponding character is added to input, which is converted to a string.
/// the boolean values keep track of whether or not the string already contains a character so that it isn't
/// added multiple times
/// </summary>
        StringBuilder input; //tracks keyboard input
        string normalizedInput; //keyboard input is converted to a string before it is sent
        bool thrust; 
        bool fire;
        bool left;
        bool right;

        public Form1()
        {
            InitializeComponent();
            input = new StringBuilder("");
            nameBox.KeyDown += new KeyEventHandler(MessageEnterHandler);
            nameBox.KeyUp += new KeyEventHandler(MessageEnterHandler);

            // Start a new timer that will redraw the game every 10 milliseconds 
            // This should correspond to about 67 frames per second.
            frameTimer = new System.Timers.Timer();
            frameTimer.Interval = 10;
            frameTimer.Elapsed += Redraw;
            frameTimer.Elapsed += SendInput; //player keyboard input
            frameTimer.Start();
            this.AcceptButton = connectButton;

            thrust=false;
            fire=false;
            left=false;
            right=false;

        }

        /// <summary>
        /// This is the event handler when the enter key is pressed in the messageToSend box
        /// </summary>
        /// <param name="sender">The Form control that fired the event</param>
        /// <param name="e">The key event arguments</param>
        private void MessageEnterHandler(object sender, KeyEventArgs e)
        {


        }

        /// <summary>
        /// Keyboard input is sent to the server every 10 milliseconds
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendInput(object sender, ElapsedEventArgs e)
        {
            if (gc != null)
            {
                if (normalizedInput != "")
                {
                    gc.SendInput(normalizedInput);
                }
            }

        }


        /// <summary>
        /// tracks whether or not the connection to the server has been lost
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckConnection(object sender, ElapsedEventArgs e)
        {
            if (gc.Connectionlost)
            {
                MessageBox.Show("Server has been closed while game is runninng. Game will reset");
                
            }
        }

        private void Redraw(object sender, ElapsedEventArgs e)
        {
            // Invalidate this form and all its children (true)
            // This will cause the form to redraw as soon as it can
            MethodInvoker redraw = new MethodInvoker(() =>
            {
                this.Invalidate(true);
            });
            this.Invoke(redraw);
        }


        /// <summary>
        /// When the connect button is clicked, it calls this function.
        /// This function will check the texts boxes to determine if they are valid and then sends the information to 
        /// the game controller to establish a connection.
        /// 
        /// Once a connection has been established, the text boxes and connect button will gray out so they cannot be used again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectButton_Click_1(object sender, EventArgs e)
        {
            if (serverAddress.Text == "")
            {
                MessageBox.Show("Please enter a server address");
                return;
            }
            if (nameBox.Text == "")
            {
                MessageBox.Show("Please enter a server address");
                return;
            }

            // Disable the controls and try to connect
            connectButton.Enabled = false;
            serverAddress.Enabled = false;
            nameBox.Enabled = false;


            theWorld = new World();
            gc = new GameController(theWorld);
            if (!gc.ConnectToServer(serverAddress.Text, nameBox.Text))
            {
                MessageBox.Show("Could not connect to server.\nInvalid server address entered or server connect timed out.");
                connectButton.Enabled = true;
                serverAddress.Enabled = true;
                nameBox.Enabled = true;
                return;
            }


            while (theWorld.getWorldSize() == 0) { } //world size is 0 until it the correct size is received from the server
            
            //once the player name and world size is receive from the server, the main game is created and drawn in drawingPanel
            // while the scoreboard is created and drawn to the right of themain game 
            ClientSize = new Size(theWorld.getWorldSize() + 200, theWorld.getWorldSize() + 25);
            drawingPanel = new DrawingPanel(theWorld);
            drawingPanel.Location = new Point(0, 25);
            drawingPanel.Size = new Size(theWorld.getWorldSize(), theWorld.getWorldSize());
            this.Controls.Add(drawingPanel);

            drawingPanel.BackColor = Color.Black;

            playerPanel = new PlayerPanel(theWorld);
            playerPanel.Location = new Point(theWorld.getWorldSize(), 25);
            playerPanel.Size = new Size(200, theWorld.getWorldSize());
            this.Controls.Add(playerPanel);

            playerPanel.BackColor = Color.White;

        }


        /// <summary>
        /// function is called when the keyboar detects a key is down. s 
        /// Once a key down is detected, a character is added to a string builder that is converted to a string
        /// function also checks whether or not the character has already been added to the string builder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gc != null)
            {


                if (e.KeyCode == Keys.Left)
                {
                    if (!left)
                    {
                        input.Append("L");
                        left = true;
                        normalizedInput = input.ToString();
                        gc.SendInput(normalizedInput);
                    }

                }
                if (e.KeyCode == Keys.Right)
                {
                    if (!right)
                    {
                        input.Append("R");
                        right = true;
                        normalizedInput = input.ToString();
                        gc.SendInput(normalizedInput);
                    }
                }

                if (e.KeyCode == Keys.Up)
                {
                    if (!thrust)
                    {
                        thrust = true;
                        input.Append("T");
                        normalizedInput = input.ToString();
                        gc.SendInput(normalizedInput);
                    }
                }

                if (e.KeyCode == Keys.Space)
                {
                    if (!fire)
                    {
                        input.Append("F");
                        fire = true;
                        normalizedInput = input.ToString();
                        gc.SendInput(normalizedInput);

                    }
                }

                e.SuppressKeyPress = true;

            }
        }

        /// <summary>
        /// When a key is depressed, it's cooresponding character is removed from the string builder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (gc != null)
            {


                if (e.KeyCode == Keys.Left)
                {
                    input.Replace("L", "");
                    left = false;
                }
                if (e.KeyCode == Keys.Right)
                {
                    input.Replace("R", "");
                    right = false;
                }

                if (e.KeyCode == Keys.Up)
                {
                    input.Replace("T", "");
                    thrust = false;
                }

                if (e.KeyCode == Keys.Space)
                {
                    input.Replace("F", "");
                    fire = false;
                }
                normalizedInput = input.ToString();
            }
        }

        /// <summary>
        /// Help box shows a message box that contains instructions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void controlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("UP: Fire Thrusters\n" +
                            "LEFT: Rotate Left\n" +
                            "RIGHT: Rotate Right\n" +
                            "SPACE: Fire"
                           );
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Solution developed by Andrew Russell and Bryan Hatasaka.\n" +
                            "CS 3500 Fall 2017, University of Utah\n" +
                            "Ship and projectile artwork by Alex Smith");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            frameTimer.Stop();
            // Proecesses the remaining event on the Form thread before closing.
            // This stops race conditions from the method invoker from occuring and throwing
            // an exception for attempting to access a disposed object.
            Application.DoEvents();
        }
    }
}
