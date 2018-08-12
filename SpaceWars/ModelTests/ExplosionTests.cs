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
    public class ExplosionTests
    {
        [TestMethod()]
        public void ExplosionConstructorTest()
        {
            Explosion explo = new Explosion(1, 2.4, 5.6);
            Assert.AreEqual(1, explo.getID());
            Assert.AreEqual(2.4, explo.X_coord);
            Assert.AreEqual(5.6, explo.Y_coord);
            Assert.IsFalse(explo.Dead);
            Assert.AreEqual(2, explo.Radius);
        }

        [TestMethod()]
        public void UpdateRadiusTest()
        {
            Explosion explo = new Explosion(1, 2.4, 5.6);

            for(int i = 4; i < 86; i += 2)
            {
                explo.UpdateRadius();

                if(i < 81)
                    Assert.AreEqual(i, explo.Radius);
                else
                    Assert.AreEqual(82, explo.Radius);
            }

            Assert.IsTrue(explo.Dead);
        }

        [TestMethod()]
        public void SetCoordinatesTest()
        {
            Explosion explo = new Explosion(1, 2.4, 5.6);

            explo.X_coord = 22;
            explo.Y_coord = 44;

            Assert.AreEqual(22, explo.X_coord);
            Assert.AreEqual(44, explo.Y_coord);
        }

    }
}