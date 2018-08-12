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
    public class ProjectileTests
    {
        private static Vector2D zeroZeroVec = new Vector2D(0, 0);

        [TestMethod()]
        public void ProjectileDfaultConstructorTest()
        {
            Projectile proj = new Projectile();
            Assert.IsNull(proj.GetLocation());
            Assert.IsNull(proj.GetDirection());
        }

        [TestMethod()]
        public void ProjectileConstructorTest()
        {
            Projectile proj = new Projectile(0, 1, zeroZeroVec, zeroZeroVec);

            Assert.IsTrue(proj.GetActive());
            Assert.AreEqual(0, proj.GetID());
            Assert.AreEqual(zeroZeroVec, proj.GetLocation());
            Assert.AreEqual(zeroZeroVec, proj.GetDirection());
            Assert.AreEqual(1, proj.GetOwner());

        }

        [TestMethod()]
        public void SetLocationTest()
        {
            Projectile proj = new Projectile(0, 1, zeroZeroVec, zeroZeroVec);

            Vector2D vec = new Vector2D(22, 500);
            proj.SetLocation(vec);

            Assert.AreEqual(vec, proj.GetLocation());
        }

        [TestMethod()]
        public void SetDirectionTest()
        {
            Projectile proj = new Projectile(0, 1, zeroZeroVec, zeroZeroVec);

            Vector2D vec = new Vector2D(5, 100);
            proj.SetDirection(vec);

            Assert.AreEqual(vec, proj.GetDirection());
        }

        [TestMethod()]
        public void SetAliveTest()
        {
            Projectile proj = new Projectile(0, 1, zeroZeroVec, zeroZeroVec);

            proj.SetAlive(false);

            Assert.IsFalse(proj.GetActive());
        }
    }
}