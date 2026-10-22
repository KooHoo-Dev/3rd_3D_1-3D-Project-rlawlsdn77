using System;
using System.Collections.Generic;

namespace OverFixed
{
    public enum ItemType
    {
        Scrap,
        Fuel,
        Crystal,
        Capsule,
        Alloy,
        Part,
        StabCell,
    }
    
    [Serializable]
    public class ItemRow
    {
        public ItemType ItemID;
        public string Name;
        public float CarrySpeedRate;
        public float SpawnWeight;
        public float SpawnDistanceMin; 
        
        public static ItemRow FromCells(Dictionary<string, string> cells)
        {
            return new ItemRow
            {
                ItemID = CsvParser.ToEnum<ItemType>(cells["ItemID"]),
                Name = cells["Name"],
                CarrySpeedRate = CsvParser.ToFloat(cells["CarrySpeedRate"]),
                SpawnWeight = CsvParser.ToFloat(cells["SpawnWeight"]),
                SpawnDistanceMin = CsvParser.ToFloat(cells["SpawnDistanceMin"]),
            };
        }
    }

    [Serializable]
    public class ItemCount
    {
        public ItemType ItemID;
        public int Count;
    }

    [Serializable]
    public class ItemWeight
    {
        public ItemType ItemID;
        public float Weight;
    }
}
