using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RocketTanuki.Types;

namespace RocketTanuki
{
    /// <summary>
    /// 指し手を表すデータ構造
    /// </summary>
    public class Move
    {
        public int FileFrom { get; set; }
        public int RankFrom { get; set; }
        public Piece PieceFrom { get; set; }
        public int FileTo { get; set; }
        public int RankTo { get; set; }
        public Piece PieceTo { get; set; }
        public bool Drop { get; set; }
        public bool Promotion { get; set; }

        public string ToUsiString()
        {
            string usiString = "";
            if (Drop)
            {
                usiString += char.ToUpper(PieceToChar[(int)PieceFrom]);
                usiString += "*";
            }
            else
            {
                usiString += (char)(FileFrom + '1');
                usiString += (char)(RankFrom + 'a');
            }

            usiString += (char)(FileTo + '1');
            usiString += (char)(RankTo + 'a');

            if (Promotion)
            {
                usiString += "+";
            }

            return usiString;
        }
    }
}
