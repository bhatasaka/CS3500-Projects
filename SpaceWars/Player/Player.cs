using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWars
{
    public class Player
    {
        int ID; //ID of the player
        string name; //players Name
        int shotsFired; //counter for all shots fired
        int hits; //counter for all shots that hit
        int score; //player's score
        int FramesSinceDead;

        int FramesSinceLastShot; //counts how many frames it has been since the last shot
                                 //used to determine if a ship is ready to fire again

        int FramesPerShot; //how many frames per shot

        bool recentThrust; //tracks if a player has sent a request to thrust
        bool recentLeft;//tracks if a player has sent a request to turn left
        bool recentRight; //tracks if a player has sent a request to turn right
        bool recentProjectile; //tracks if the player has sent a request to fire

        bool recentScore; //tracks if a player has recently scored. score will be updated on next frame
                          //if ship is dead when they score, score will be updated when they respawn
        



        public Player(string playerName, int playerID, int frames)
        {
            name = playerName;
            ID = playerID;
            shotsFired = 0;
            hits = 0;
            score = 0;
            FramesSinceLastShot = 0;
            FramesSinceDead = 0;
            FramesPerShot = frames;
            

             recentThrust =false;
             recentLeft = false;
             recentRight = false;
             recentProjectile = false;
             recentScore = false;
        }
        
        
        /// <summary>
        /// Called every time a player scores 
        /// </summary>
        public void AddHit()
        {
            hits+=1;
        }

        /// <summary>
        /// Gets the number of hits
        /// </summary>
        /// <returns>returns the number of hits</returns>
        public int GetHits()
        {
            return hits;
        }

        /// <summary>
        /// increments the player's score
        /// </summary>
        public void AddScore()
        {
            score+=1;
        }

        /// <summary>
        /// Gets the player's score
        /// </summary>
        /// <returns>player's score</returns>
        public int GetScore()
        {
            return score;
        }

        public void AddShotsFired()
        {
            shotsFired++;
        }
        public int GetShotsFired()
        {
            return shotsFired;
        }

        public int GetID()
        {
            return ID;
        }

        public string GetName()
        {
            return name;
        }

        public void SetRecentRight(bool input)
        {
            recentRight = input;
        }

        public bool GetRecentRight()
        {
            return recentRight;
        }
        public void SetRecentLeft(bool input)
        {
            recentLeft = input;
        }

        public bool GetRecentLeft()
        {
            return recentLeft;
        }
        public int GetProjectilesOnScreen()
        {
            return FramesSinceLastShot;
        }
        public void RemoveProjectilesOnScreen()
        {
            FramesSinceLastShot--;
        }


        public void SetRecentThrust(bool input)
        {
             recentThrust = input;
        }

        public bool GetRecentThrust()
        {
            return recentThrust;
        }
        public bool GetRecentProjectile()
        {
            return recentProjectile;
        }

        /// <summary>
        /// Helper function that determines if a ship is ready to fire. There is an integer that 
        /// holds the setting for the number of frames per shot. There is a counter that is incremented each from to determine
        /// how many frames it has been since the last shot.
        /// 
        /// If the number of frames that has passed is greater than the frames per shot, the function return true to say it is ready to fire
        /// </summary>
        /// <returns>returns true if the shit can fire, false if it can't</returns>
        public bool ReadyToFire()
        {
            if (FramesSinceLastShot >= FramesPerShot)
            {
                return true;

            }
            else
                return false;
        }

        public void ProjectileFired()
        {
            FramesSinceLastShot = 0;
        }

        public void AddProjectileFrame()
        {
            if (FramesSinceLastShot<FramesPerShot)
            {
                FramesSinceLastShot++;

            }
        }

        public void SetRecentProjectile(bool input)
        {
            recentProjectile = input;
        }

        public int GetFramesSinceDead()
        {
            return FramesSinceDead;
        }

        /// <summary>
        /// Increments the framesSinceDead by 1
        /// </summary>
        public void IncrementFramesSinceDead()
        {
            FramesSinceDead++;
        }

        public void ResetFramesSinceDead()
        {
            FramesSinceDead = 0;
        }
        public void SetRecentScore(bool input)
        {
            recentScore = input;
        }
        public bool GetRecentScore()
        {
            return recentScore;
        }

    }
}
