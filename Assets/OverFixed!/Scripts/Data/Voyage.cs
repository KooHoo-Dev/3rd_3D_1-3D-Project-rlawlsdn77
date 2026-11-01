using System;
using System.Collections.Generic;

namespace OverFixed
{
    // VoyageTable 한 줄 (항해 횟수별 난이도). 항해 1회부터 빠짐없이 차례대로 적는다
    [Serializable]
    public class VoyageRow
    {
        public int QuotaBase;               // 할당량 (크레딧)
        public float VoyageTime;            // 항해 시간 (초)
        public int ArrivalReward;           // 도착 보상 (크레딧)
        public float BaseDecay;             // 초당 줄어드는 코어 안정도 (%)
        public float EventIntervalMin;      // 패널 사고 간격 (초)
        public float EventIntervalMax;

        public static VoyageRow FromCells(Dictionary<string, string> cells)
        {
            return new VoyageRow
            {
                QuotaBase = TsvParser.ToInt(cells["QuotaBase"]),
                VoyageTime = TsvParser.ToFloat(cells["VoyageTime"]),
                ArrivalReward = TsvParser.ToInt(cells["ArrivalReward"]),
                BaseDecay = TsvParser.ToFloat(cells["BaseDecay"]),
                EventIntervalMin = TsvParser.ToFloat(cells["EventIntervalMin"]),
                EventIntervalMax = TsvParser.ToFloat(cells["EventIntervalMax"]),
            };
        }
    }
}
