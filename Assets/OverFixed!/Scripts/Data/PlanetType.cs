using System;
using System.Collections.Generic;

namespace OverFixed
{
    public enum PlanetType
    {
        Mining,
        Crystal,
        Industry,
        Research,
        Trade,
    }

    // PlanetTypeTable 한 줄 (후보 행성 한 종류). 할당량, 항해 시간, 도착 보상은 행성과 상관없이 VoyageTable
    [Serializable]
    public class PlanetTypeRow
    {
        public PlanetType PlanetTypeID;
        public string Name;                         // 후보 카드 제목
        public string TrendDesc;                    // 후보 카드 "주문 경향"
        public string BenefitDesc;                  // 후보 카드 "정비 혜택"
        public List<ItemWeight> OrderItemWeights;   // 그 재료가 들어간 주문을 뽑는 비중 배율 (주문 안 재료 중 최댓값). 빈 목록이면 고르게
        public float OrderRewardRate;               // 주문 보상 배율
        public float OrderTimeRate;                 // 주문 제한시간 배율
        public EffectType MaintenanceEffectType;    // 도착한 뒤 정비에 적용하는 혜택
        public float MaintenanceEffectValue;

        public static PlanetTypeRow FromCells(Dictionary<string, string> cells)
        {
            return new PlanetTypeRow
            {
                PlanetTypeID = TsvParser.ToEnum<PlanetType>(cells["PlanetTypeID"]),
                Name = cells["Name"],
                TrendDesc = cells["TrendDesc"],
                BenefitDesc = cells["BenefitDesc"],
                OrderItemWeights = TsvParser.ToItemWeights(cells["OrderItemWeights"]),
                OrderRewardRate = TsvParser.ToFloat(cells["OrderRewardRate"]),
                OrderTimeRate = TsvParser.ToFloat(cells["OrderTimeRate"]),
                MaintenanceEffectType = TsvParser.ToEnum<EffectType>(cells["MaintenanceEffectType"]),
                MaintenanceEffectValue = TsvParser.ToFloat(cells["MaintenanceEffectValue"]),
            };
        }
    }
}
