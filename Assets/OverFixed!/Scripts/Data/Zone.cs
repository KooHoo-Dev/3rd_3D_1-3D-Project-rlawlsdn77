using System;
using System.Collections.Generic;

namespace OverFixed
{
    public enum ZoneType
    {
        ResourceField,
        DebrisField,
        Wreck,
        Station,
        Calm,
        TradeRoute,
        AsteroidBelt,
        IonStorm,
        Nebula,
        Rapids,
    }

    public enum ZoneCategory
    {
        Good,
        Mixed,
        Bad,
    }

    // ZoneTable 한 줄 (경로 지대 한 종류)
    [Serializable]
    public class ZoneRow
    {
        public ZoneType ZoneID;
        public string Name;                 // 후보 카드, 지대 예고
        public string Desc;
        public ZoneCategory Category;       // Bad면 VoyageTable BadZoneWeight를 SpawnWeight에 곱한다
        public List<ZoneEffect> Effects;    // 빈 목록이면 효과 없음
        public float SpawnWeight;           // 경로에 뽑히는 비중 (0이면 뽑지 않음)
        public int MinVoyage;               // 이 항해 번호부터 뽑힌다

        // EffectType과 EffectValue 칸은 ;로 나눈 같은 순서의 목록이다 ("SpawnRate;CrystalRate", "2;1.5")
        public static ZoneRow FromCells(Dictionary<string, string> cells)
        {
            List<string> effectTypes = TsvParser.ToList(cells["EffectType"]);
            List<string> effectValues = TsvParser.ToList(cells["EffectValue"]);
            if (effectTypes.Count != effectValues.Count) throw new FormatException("ZoneTable " + cells["ZoneID"] + ": EffectType과 EffectValue의 개수가 다릅니다.");

            var effects = new List<ZoneEffect>();
            for (int index = 0; index < effectTypes.Count; index++)
            {
                effects.Add(new ZoneEffect { EffectType = TsvParser.ToEnum<EffectType>(effectTypes[index]), Value = TsvParser.ToFloat(effectValues[index]) });
            }

            return new ZoneRow
            {
                ZoneID = TsvParser.ToEnum<ZoneType>(cells["ZoneID"]),
                Name = cells["Name"],
                Desc = cells["Desc"],
                Category = TsvParser.ToEnum<ZoneCategory>(cells["Category"]),
                Effects = effects,
                SpawnWeight = TsvParser.ToFloat(cells["SpawnWeight"]),
                MinVoyage = TsvParser.ToInt(cells["MinVoyage"]),
            };
        }
    }

    [Serializable]
    public class ZoneEffect
    {
        public EffectType EffectType;
        public float Value;
    }
}
