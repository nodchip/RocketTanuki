using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketTanuki
{
    public class TimeManager
    {
        private const int MinimumThinkingTimeMs = 2000;
        private const int NetworkDelayMs = 200;

        public void Start(bool ponder, int btime, int wtime, int byoyomi, int binc, int winc, bool infinite, Color sideToMove)
        {
            this.ponder = ponder;
            this.btime = btime;
            this.wtime = wtime;
            this.byoyomi = byoyomi;
            this.binc = binc;
            this.winc = winc;
            this.infinite = infinite;
            this.sideToMove = sideToMove;

            goTime = DateTime.Now;
            if (!ponder)
            {
                timeConsumptionStartTime = goTime;
                CalculateEndTime();
            }
        }

        public bool IsThinking()
        {
            return ponder || infinite || DateTime.Now < endTime;
        }

        public void PonderHit()
        {
            timeConsumptionStartTime = DateTime.Now;
            CalculateEndTime();
            ponder = false;
        }

        private void CalculateEndTime()
        {
            int time;
            int inc;
            switch (sideToMove)
            {
                case Color.Black:
                    time = btime;
                    inc = binc;
                    break;

                case Color.White:
                    time = wtime;
                    inc = winc;
                    break;

                default:
                    throw new Exception($"Unsupported color: sideToMove={sideToMove}");
            }

            int thinkingTimeMs;
            if (byoyomi == 0 && inc == 0)
            {
                //切れ負け
                // 残り時間の1/8を使う
                thinkingTimeMs = time / 8;

            }
            else if (byoyomi > 0)
            {
                // 秒読み
                // 残り時間+秒読みの1/8を使う
                thinkingTimeMs = (time + byoyomi) / 8;
                // 思考時間が秒読みの時間より短い場合、秒読みの時間使う
                thinkingTimeMs = Math.Max(thinkingTimeMs, byoyomi);

            }
            else if (inc > 0)
            {
                // フィッシャークロック
                // 残り時間+秒読みの1/8を使う
                thinkingTimeMs = (time + inc) / 8;
            }
            else
            {
                throw new Exception($"Unsupported time mode. time={time} byoyomi={byoyomi} inc={inc}");
            }

            // 最小思考時間以上思考する
            thinkingTimeMs = Math.Max(thinkingTimeMs, MinimumThinkingTimeMs);

            // ネットワーク遅延の分引く
            thinkingTimeMs -= NetworkDelayMs;

            endTime = timeConsumptionStartTime + TimeSpan.FromMilliseconds(thinkingTimeMs);
        }

        /// <summary>
        /// goコマンドを受信してからの経過ミリ秒を返す
        /// </summary>
        /// <returns></returns>
        public int ElapsedMs()
        {
            return Math.Max(1, (int)(DateTime.Now - goTime).TotalMilliseconds);
        }

        private volatile bool ponder;
        private int btime;
        private int wtime;
        private int byoyomi;
        private int binc;
        private int winc;
        private bool infinite;
        private Color sideToMove;
        private DateTime goTime;
        private DateTime timeConsumptionStartTime;
        private DateTime endTime;
        public static TimeManager Instance { get; } = new TimeManager();
    }
}
