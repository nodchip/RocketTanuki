using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocketTanuki;

namespace RocketTanukiTests
{
    [TestClass]
    public class MoveGeneratorTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Program.Initialize();
        }

        [TestMethod]
        public void GenerateMove_Hirate()
        {
            var position = new Position();
            position.Set(Position.StartposSfen);
            int actual = MoveGenerator.Generate(position).ToList().Count;
            Assert.AreEqual(30, actual);
        }

        [TestMethod]
        public void GenerateMove_Matsuri()
        {
            var position = new Position();
            position.Set(Position.MatsuriSfen);
            int actual = MoveGenerator.Generate(position).ToList().Count;
            // 自殺手を含む。
            Assert.AreEqual(208, actual);
        }

        [TestMethod]
        public void GenerateMove_Max()
        {
            var position = new Position();
            position.Set(Position.MaxSfen);
            int actual = MoveGenerator.Generate(position).ToList().Count;
            Assert.AreEqual(593, actual);
        }
    }
}
