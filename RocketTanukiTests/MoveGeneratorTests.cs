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
            position.Set("l6nl/5+P1gk/2np1S3/p1p4Pp/3P2Sp1/1PPb2P1P/P5GS1/R8/LN4bKL w GR5pnsg 1");
            int actual = MoveGenerator.Generate(position).ToList().Count;
            // 自殺手を含む。
            Assert.AreEqual(208, actual);
        }

        [TestMethod]
        public void GenerateMove_Max()
        {
            var position = new Position();
            position.Set("8R/kSS1S1K2/4B4/9/9/9/9/9/3L1L1L1 b RBGSNLP3g3n17p 1");
            int actual = MoveGenerator.Generate(position).ToList().Count;
            Assert.AreEqual(593, actual);
        }
    }
}
