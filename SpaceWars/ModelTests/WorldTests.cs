using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpaceWars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWars.Tests
{
    [TestClass()]
    public class WorldTests
    {
        private static Vector2D zeroZeroVec = new Vector2D(0, 0);

        [TestMethod()]
        public void WorldDefaultConstructorTest()
        {
            World world = new World();
            Assert.AreEqual(0, world.getWorldSize());
        }

        [TestMethod()]
        public void WorldSizeConstructorTest()
        {
            World world = new World(457);
            Assert.AreEqual(457, world.getWorldSize());
        }

        [TestMethod()]
        public void getShipsTest()
        {
            World world = new World(457);
            Assert.AreEqual(0, world.getShips().Count);

            //This part only works if ship update works.
            Ship ship = new Ship(0, "ship", zeroZeroVec, zeroZeroVec);
            ship.SetHp(1);
            world.Update(ship);
            Assert.AreEqual(1, world.getShips().Count);
        }

        [TestMethod()]
        public void getExplosionsTest()
        {
            World world = new World(457);
            Assert.AreEqual(0, world.getExplosions().Count);

            //This part only works if ship update works.
            Ship ship = new Ship(0, "ship", zeroZeroVec, zeroZeroVec);
            ship.SetHp(0);
            world.Update(ship);
            Assert.AreEqual(1, world.getExplosions().Count);
        }

        [TestMethod()]
        public void getProjectileTest()
        {
            World world = new World(457);
            Assert.AreEqual(0, world.getProjectile().Count);

            //This part only works if projectile update works
            Projectile proj = new Projectile(0, 0, zeroZeroVec, zeroZeroVec);
            world.Update(proj);
            Assert.AreEqual(1, world.getProjectile().Count);
        }

        [TestMethod()]
        public void getStarTest()
        {
            World world = new World(457);
            Assert.AreEqual(0, world.getStar().Count);

            //This part only work if star update works
            Star star = new Star(0, zeroZeroVec, 1);
            world.Update(star);
            Assert.AreEqual(1, world.getStar().Count);
        }

        [TestMethod()]
        public void GetShipTest()
        {
            World world = new World(457);
            Assert.AreEqual(0, world.getShips().Count);

            //This part only works if ship update works.
            Ship ship = new Ship(0, "ship", zeroZeroVec, zeroZeroVec);
            ship.SetHp(1);
            world.Update(ship);
            //Only possible because ship is the same ship
            Assert.AreEqual(ship, world.GetShip(0));
        }

        [TestMethod()]
        public void getWorldSizeTest()
        {
            World world = new World(457);
            Assert.AreEqual(457, world.getWorldSize());
        }

        [TestMethod()]
        public void setWorldSizeTest()
        {
            World world = new World(457);
            world.setWorldSize(2);
            Assert.AreEqual(2, world.getWorldSize());
        }

        [TestMethod()]
        public void UpdateShipSingleTest()
        {
            World world = new World(750);
            Ship ship = new Ship();
            ship.SetHp(1);
            world.Update(ship);
            Assert.AreEqual(1, world.getShips().Count);
            Assert.AreEqual(0, world.getExplosions().Count);

        }

        [TestMethod()]
        public void UpdateShipNoHPTest()
        {
            World world = new World(750);

            Ship ship = new Ship(0, "ship", zeroZeroVec, zeroZeroVec);
            ship.SetHp(0);
            world.Update(ship);
            Assert.AreEqual(0, world.getShips().Count);
            Assert.AreEqual(1, world.getExplosions().Count);
        }

        [TestMethod()]
        public void UpdateShipSortedListTest()
        {
            World world = new World(750);

            Ship ship0 = new Ship(0, "ship0", zeroZeroVec, zeroZeroVec);
            ship0.SetHp(1);
            ship0.SetScore(0);
            world.Update(ship0);

            Ship ship1 = new Ship(1, "ship1", zeroZeroVec, zeroZeroVec);
            ship0.SetHp(1);
            ship0.SetScore(1);
            world.Update(ship1);

            //Both ships die
            ship0.SetHp(0);
            ship1.SetHp(0);
            world.Update(ship0);
            world.Update(ship1);

            world.getExplosions()[0].Dead = true;
            world.getExplosions()[1].Dead = true;

            //Both ships respawn
            ship0.SetHp(1);
            ship1.SetHp(1);
            world.Update(ship0);
            world.Update(ship1);

            //Ship1 scored another point
            ship1 = new Ship(1, "ship1", zeroZeroVec, zeroZeroVec);
            ship1.SetHp(1);
            ship1.SetScore(2);
            world.Update(ship1);

            //Sorted list of ships based on score from greatest to least
            List<Ship> expectedList = new List<Ship>();
            expectedList.Add(ship0);
            expectedList.Add(ship1); //Bigger score

            //Make sure explosions are properly removed
            Assert.AreEqual(0, world.getExplosions().Count);

            int index = 1;
            foreach (Ship s in world.GetList())
            {
                Assert.AreEqual("ship" + index, s.GetName());
                index--;
            }
        }

        [TestMethod()]
        public void UpdateStarTest()
        {
            World world = new World(750);
            Star star = new Star(0, zeroZeroVec, 1.0);
            world.Update(star);
            Assert.AreEqual(1, world.getStar().Count);
        }

        [TestMethod()]
        public void GetListTest()
        {
            World world = new World(750);
            Assert.AreEqual(0, world.GetList().Count);
        }

        [TestMethod()]
        public void UpdateProjectileAliveTest()
        {

            World world = new World(750);
            Projectile proj = new Projectile(0, 0, zeroZeroVec, zeroZeroVec);
            world.Update(proj);
            Assert.AreEqual(1, world.getProjectile().Count);
        }

        [TestMethod()]
        public void UpdateProjectileDeadTest()
        {
            World world = new World(750);
            Projectile proj = new Projectile(0, 0, zeroZeroVec, zeroZeroVec);
            proj.SetAlive(false);
            world.Update(proj);
            Assert.AreEqual(0, world.getProjectile().Count);
        }

        [TestMethod()]
        public void UpdateExplosionsTest()
        {
            World world = new World(750);

            Ship ship = new Ship(0, "ship", zeroZeroVec, zeroZeroVec);
            ship.SetHp(0);
            world.Update(ship);

            Assert.AreEqual(1, world.getExplosions().Count);

            Explosion explo = world.getExplosions()[0];
            explo.Dead = true;
            world.Update(explo);
            Assert.AreEqual(0, world.getExplosions().Count);
        }
    }
}