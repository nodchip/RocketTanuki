using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocketTanuki;

namespace RocketTanukiTests
{
    [TestClass]
    public class PositionTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Program.Initialize();
        }

        [TestMethod]
        public void Position_Hash()
        {
            foreach (var sfen in new[] { Position.StartposSfen, Position.MatsuriSfen, Position.MaxSfen })
            {
                var position = new Position();
                position.Set(sfen);

                long hash = position.Hash;
                foreach (var move in MoveGenerator.Generate(position, null))
                {
                    using (var mover = new Mover(position, move)) { }
                    Assert.AreEqual(hash, position.Hash);
                }
            }
        }

        [TestMethod]
        public void Position_King()
        {
            var position = new Position();
            position.Set(Position.StartposSfen);

            Assert.AreEqual(4, position.BlackKingFile);
            Assert.AreEqual(8, position.BlackKingRank);
            Assert.AreEqual(4, position.WhiteKingFile);
            Assert.AreEqual(0, position.WhiteKingRank);
        }

        [TestMethod]
        public void Position_ToSfenString()
        {
            foreach (var expected in new[] { Position.StartposSfen, Position.MatsuriSfen, Position.MaxSfen })
            {
                var position = new Position();
                position.Set(expected);
                var actual = position.ToSfenString();
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod]
        public void GenerateMove_OnlyCapture()
        {
            var position = new Position();
            position.Set(Position.MatsuriSfen);
            foreach (var move in MoveGenerator.Generate(position, null, 0, 0))
            {
                Assert.AreNotEqual(Piece.NoPiece, move.PieceTo);
            }
        }
    }
}
