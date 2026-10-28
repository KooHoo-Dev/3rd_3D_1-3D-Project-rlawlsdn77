using System;
using System.Collections.Generic;

namespace OverFixed
{
    // VoyageTable 한 줄 (항해 횟수별 난이도). 항해 번호는 1부터, 표에 빠짐없이 차례대로 적는다
    [Serializable]
    public class VoyageRow
    {
        public int VoyageIndex;
        public int QuotaBase;               // 할당량 (크레딧)
        public float VoyageTime;            // 항해 시간 (초)
        public int ArrivalReward;           // 도착 보상 (크레딧)
        public float BaseDecay;             // 시간이 지나며 초당 줄어드는 코어 안정도 (%)
        public float EventIntervalMin;      // 사고 간격 최소 (초)
        public float EventIntervalMax;      // 사고 간격 최대 (초)
        public float BadZoneWeight;         // 불리한 지대의 뽑힘 가중치 배율

        public static VoyageRow FromCells(Dictionary<string, string> cells)
        {
            return new VoyageRow
            {
                VoyageIndex = TsvParser.ToInt(cells["VoyageIndex"]),
                QuotaBase = TsvParser.ToInt(cells["QuotaBase"]),
                VoyageTime = TsvParser.ToFloat(cells["VoyageTime"]),
                ArrivalReward = TsvParser.ToInt(cells["ArrivalReward"]),
                BaseDecay = TsvParser.ToFloat(cells["BaseDecay"]),
                EventIntervalMin = TsvParser.ToFloat(cells["EventIntervalMin"]),
                EventIntervalMax = TsvParser.ToFloat(cells["EventIntervalMax"]),
                BadZoneWeight = TsvParser.ToFloat(cells["BadZoneWeight"]),
            };
        }
    }
}
