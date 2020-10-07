using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    class Move
    {
        public int FromFile { get; set; }
        public int FromRank { get; set; }
        public Piece FromPiece { get; set; }
        public int ToFile { get; set; }
        public int ToRank { get; set; }
        public Piece ToPiece { get; set; }
        public bool Drop { get; set; }
        public bool Promotion { get; set; }
    }
}
