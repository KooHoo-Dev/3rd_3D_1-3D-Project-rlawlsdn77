using System;
using System.Collections.Generic;

namespace OverFixed
{
    // OrderTable 한 줄 (주문)
    [Serializable]
    public class OrderRow
    {
        public List<ItemCount> RequireItems;      // "Scrap:2|Crystal:1"
        public float TimeLimit;                   // 제한시간 (초)
        public int Reward;                        // 보상 크레딧
        public List<string> RequireModule;        // 가공품 주문에 필요한 모듈 "Refinery;Fabricator". 원재료 주문은 비어 있다

        public static OrderRow FromCells(Dictionary<string, string> cells)
        {
            return new OrderRow
            {
                RequireItems = TsvParser.ToItemCounts(cells["RequireItems"]),
                TimeLimit = TsvParser.ToFloat(cells["TimeLimit"]),
                Reward = TsvParser.ToInt(cells["Reward"]),
                RequireModule = TsvParser.ToList(cells["RequireModule"]),
            };
        }
    }
}
