using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocketTanuki;

namespace RocketTanukiTests
{
    [TestClass]
    public class MoveTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Program.Initialize();
        }

        [TestMethod]
        public void ToUsiString_NormalMove()
        {
            var move = new Move
            {
                FileFrom = 6,
                RankFrom = 6,
                PieceFrom = Piece.BlackPawn,
                FileTo = 6,
                RankTo = 5,
                PieceTo = Piece.NoPiece,
                Drop = false,
                Promotion = false,
            };

            Assert.AreEqual("7g7f", move.ToUsiString());
        }

        [TestMethod]
        public void ToUsiString_PromotionMove()
        {
            var move = new Move
            {
                FileFrom = 7,
                RankFrom = 7,
                PieceFrom = Piece.BlackBishop,
                FileTo = 1,
                RankTo = 1,
                PieceTo = Piece.WhiteBishop,
                Drop = false,
                Promotion = true,
            };

            Assert.AreEqual("8h2b+", move.ToUsiString());
        }

        [TestMethod]
        public void ToUsiString_DropMove()
        {
            var move = new Move
            {
                FileFrom = -1,
                RankFrom = -1,
                PieceFrom = Piece.BlackPawn,
                FileTo = 1,
                RankTo = 1,
                PieceTo = Piece.NoPiece,
                Drop = true,
                Promotion = false,
            };

            Assert.AreEqual("P*2b", move.ToUsiString());
        }
    }
}
