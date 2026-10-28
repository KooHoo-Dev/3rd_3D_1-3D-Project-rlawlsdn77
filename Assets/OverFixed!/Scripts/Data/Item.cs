using System;
using System.Collections.Generic;

namespace OverFixed
{
    // Unity는 enum을 숫자로 저장하므로 순서를 바꾸면 프리팹의 itemId가 다른 재료를 가리킨다
    // (새 재료는 맨 뒤에 추가할 것)
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
        public float SpawnWeight;
        public float SpawnDistanceMin; 
        
        public static ItemRow FromCells(Dictionary<string, string> cells)
        {
            return new ItemRow
            {
                ItemID = TsvParser.ToEnum<ItemType>(cells["ItemID"]),
                Name = cells["Name"],
                SpawnWeight = TsvParser.ToFloat(cells["SpawnWeight"]),
                SpawnDistanceMin = TsvParser.ToFloat(cells["SpawnDistanceMin"]),
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
