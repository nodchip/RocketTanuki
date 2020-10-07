using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    enum Color
    {
        Black,
        White,
    }

    enum Piece
    {
        NoPiece,
        BlackPawn,
        BlackLance,
        BlackKnight,
        BlackSilver,
        BlackGold,
        BlackBishop,
        BlackRook,
        BlackKing,
        BlackPromotedPawn,
        BlackPromotedLance,
        BlackPromotedKnight,
        BlackPromotedSilver,
        BlackHorse,
        BlackDragon,
        WhitePawn,
        WhiteLance,
        WhiteKnight,
        WhiteSilver,
        WhiteGold,
        WhiteBishop,
        WhiteRook,
        WhiteKing,
        WhitePromotedPawn,
        WhitePromotedLance,
        WhitePromotedKnight,
        WhitePromotedSilver,
        WhiteHorse,
        WhiteDragon,
        NumPieces,
    }

    class Types
    {
        public static Piece ToPromoted(Piece piece)
        {
            Debug.Assert(NonPromotedToPromoted[(int)piece] != Piece.NoPiece);
            return NonPromotedToPromoted[(int)piece];
        }

        private static Piece[] NonPromotedToPromoted = {
            Piece.NoPiece,
            Piece.BlackPromotedPawn,
            Piece.BlackPromotedLance,
            Piece.BlackPromotedKnight,
            Piece.BlackPromotedSilver,
            Piece.NoPiece,
            Piece.BlackHorse,
            Piece.BlackDragon,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.WhitePromotedPawn,
            Piece.WhitePromotedLance,
            Piece.WhitePromotedKnight,
            Piece.WhitePromotedSilver,
            Piece.NoPiece,
            Piece.WhiteBishop,
            Piece.WhiteRook,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
            Piece.NoPiece,
        };

    }
}
