using System;
using System.Collections.Generic;

namespace OverFixed
{
    public enum ModuleType
    {
        Central,
        Airlock,
        Delivery,
        Refinery,
        RepairBay,
        DroneBay,
        Comm,
        Storage,
        FireControl,
        Observatory,
        Stabilizer,
        Fabricator,
    }
    
    [Serializable]
    public class SpaceModule
    {
        public ModuleType ModuleID;
        public string Name;
        public string ShortName;
        public int MaxDurability;       // 중앙 모듈은 0 (파괴되지 않음)
        public int Price;
        public string EffectType;
        public float EffectValue;
        public string EffectScope;      // Self, Adjacent, Global
        public string Desc;

        public static SpaceModule FromCells(Dictionary<string, string> cells)
        {
            return new SpaceModule
            {
                ModuleID = TsvParser.ToEnum<ModuleType>(cells["ModuleID"]),
                Name = cells["Name"],
                ShortName = cells["ShortName"],
                MaxDurability = TsvParser.ToInt(cells["MaxDurability"]),
                Price = TsvParser.ToInt(cells["Price"]),
                EffectType = cells["EffectType"],
                EffectValue = TsvParser.ToFloat(cells["EffectValue"]),
                EffectScope = cells["EffectScope"],
                Desc = cells["Desc"],
            };
        }
    }
}
