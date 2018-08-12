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
    public class StarTests
    {
        private static Vector2D zeroZeroVec = new Vector2D(0, 0);

        [TestMethod()]
        public void StarConstructorTest()
        {
            Star star = new Star(0, zeroZeroVec, 1);

            Assert.AreEqual(0, star.getID());
            Assert.AreEqual(zeroZeroVec, star.GetLocation());
            Assert.AreEqual(1, star.GetMass());
            Assert.AreEqual(zeroZeroVec, star.GetVelocity());
        }

        [TestMethod()]
        public void SetLocationTest()
        {
            Vector2D vec = new Vector2D(2, 24);
            Star star = new Star(0, zeroZeroVec, 1);
            star.SetLocation(vec);

            Assert.AreEqual(vec, star.GetLocation());
        }

        [TestMethod()]
        public void SetVelocityTest()
        {
            Vector2D vec = new Vector2D(2, 24);
            Star star = new Star(0, zeroZeroVec, 1);
            star.SetVelocity(vec);

            Assert.AreEqual(vec, star.GetVelocity());
        }
    }
}