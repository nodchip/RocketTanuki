using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    public class BestMove
    {
        /// <summary>
        /// PVが逆順で格納されている
        /// </summary>
        public List<Move> PrincipalVariation { get; set; }

        /// <summary>
        /// 探索込みの評価値
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 指し手
        /// </summary>
        public Move Move { get; set; }

        /// <summary>
        /// 次の手
        /// </summary>
        public BestMove Next { get; set; }

        /// <summary>
        /// 探索深さ
        /// </summary>
        public int Depth { get; set; }
    }
}
