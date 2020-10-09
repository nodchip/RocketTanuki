using System;
using System.Diagnostics;
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

        [TestMethod]
        public void FromUsiString_DropMove()
        {
            var position = new Position();
            position.Set(Position.StartposSfen);
            foreach (var moveString in new[] { "5i4h", "4a3b", "1g1f", "6a5b", "1f1e", "5a4b", "1e1d", "1c1d", "1i1h", "1d1e", "1h1g", "1e1f", "1g1f" })
            {
                var move = Move.FromUsiString(position, moveString);
                position.DoMove(move);
            }

            var expected = new Move
            {
                FileFrom = -1,
                RankFrom = -1,
                PieceFrom = Piece.WhitePawn,
                FileTo = 0,
                RankTo = 4,
                PieceTo = Piece.NoPiece,
                Drop = true,
                Promotion = false,
                SideToMove = Color.White,
            };

            Assert.AreEqual(expected, Move.FromUsiString(position, "P*1e"));
        }
    }
}
