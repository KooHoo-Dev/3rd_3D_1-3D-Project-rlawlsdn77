using System;
using System.Collections.Generic;

namespace OverFixed
{
    // OrderTable 한 줄 (주문). OrderID는 다른 테이블이 참조하지 않는 행 번호라 문자열 그대로 둔다
    [Serializable]
    public class OrderRow
    {
        public string OrderID;
        public List<ItemCount> RequireItems;      // "Scrap:2|Crystal:1"
        public float TimeLimit;                   // 제한시간 (초)
        public int Reward;                        // 보상 크레딧
        public List<ModuleType> RequireModule;    // "Refinery;Fabricator". 비어 있으면 조건 없음

        public static OrderRow FromCells(Dictionary<string, string> cells)
        {
            return new OrderRow
            {
                OrderID = cells["OrderID"],
                RequireItems = TsvParser.ToItemCounts(cells["RequireItems"]),
                TimeLimit = TsvParser.ToFloat(cells["TimeLimit"]),
                Reward = TsvParser.ToInt(cells["Reward"]),
                RequireModule = TsvParser.ToEnumList<ModuleType>(cells["RequireModule"]),
            };
        }
    }
}
