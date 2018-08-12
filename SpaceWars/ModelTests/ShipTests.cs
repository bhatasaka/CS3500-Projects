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
    public class ShipTests
    {
        private static Vector2D zeroZeroVec = new Vector2D(0, 0);

        [TestMethod()]
        public void ShipConstructorTest()
        {
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);

            Assert.AreEqual(zeroZeroVec, ship.GetLocation());
            Assert.AreEqual(zeroZeroVec, ship.GetDirection());
            Assert.AreEqual(1, ship.GetShipID());
            Assert.AreEqual("ship", ship.GetName());
        }

        [TestMethod()]
        public void ShipDefaultConstructorTest()
        {
            Ship ship = new Ship();

            Assert.IsNull(ship.GetLocation());
            Assert.IsNull(ship.GetDirection());
        }

        [TestMethod()]
        public void UpdateShipTest()
        {
            //Will only work if setScore and setHp work
            Vector2D vec = new Vector2D(22, 50);
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);
            Ship ship1 = new Ship(5, "ship1", vec, vec);

            ship.SetScore(0);
            ship.SetHp(0);
            ship1.SetScore(5);
            ship1.SetHp(6);

            ship.UpdateShip(ship1);

            Assert.AreEqual(5, ship.GetScore());
            Assert.AreEqual(6, ship.GetHp());
        }

        [TestMethod()]
        public void SetLocationTest()
        {
            Vector2D vec = new Vector2D(22, 50);
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);
            ship.SetLocation(vec);

            Assert.AreEqual(vec, ship.GetLocation());
        }

        [TestMethod()]
        public void SetDirectionTest()
        {
            Vector2D vec = new Vector2D(22, 50);
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);
            ship.SetDirection(vec);

            Assert.AreEqual(vec, ship.GetDirection());
        }

        [TestMethod()]
        public void SetThrustTest()
        {
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);

            ship.SetThrust(true);
            Assert.IsTrue(ship.GetThrust());

            ship.SetThrust(false);
            Assert.IsFalse(ship.GetThrust());
        }

        [TestMethod()]
        public void SetHpTest()
        {
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);

            ship.SetHp(6);
            Assert.AreEqual(6, ship.GetHp());
        }

        [TestMethod]
        public void GetActiveTest()
        {
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);

            ship.SetHp(6);
            Assert.IsTrue(ship.GetActive());

            ship.SetHp(0);
            Assert.IsFalse(ship.GetActive());
        }

        [TestMethod()]
        public void SetScoreTest()
        {
            Ship ship = new Ship(1, "ship", zeroZeroVec, zeroZeroVec);

            ship.SetScore(6);
            Assert.AreEqual(6, ship.GetScore());
        }

        [TestMethod()]
        public void UpdateLocationTest()
        {
            Vector2D vec = new Vector2D(0, 0);
            Vector2D vec1 = new Vector2D(0, 4);
            Ship ship = new Ship(1, "ship", vec, vec);

            //Ship accelerates 4 in the +y
            //New location should be 0,4

            ship.UpdateLocation(vec1);
            Assert.AreEqual(new Vector2D(0, 4), ship.GetLocation());

        }

        [TestMethod()]
        public void RotateTest()
        {
            Vector2D vec = new Vector2D(1, 1);
            vec.Normalize();
            Ship ship = new Ship(1, "ship", vec, vec);

            //Rotate the ship by 45 at a turn rate of 1
            ship.Rotate(45, 1);
            vec.Rotate(45);

            //New direction should be 45 degrees cw
            Assert.AreEqual(vec, ship.GetDirection());

            //Rotate the ship by 45 at a turn rate of 2
            vec.Normalize();
            vec.Rotate(90);
            ship.Rotate(45, 2);

            //New direction should be 90 degrees more cw
            Assert.AreEqual(vec, ship.GetDirection());
        }
    }
}