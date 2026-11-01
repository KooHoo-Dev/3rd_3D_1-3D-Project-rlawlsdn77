using System;
using System.Collections.Generic;

namespace OverFixed
{
    // Unity는 enum을 숫자로 저장하므로 순서를 바꾸면 프리팹의 itemId가 다른 재료를 가리킨다 (새 재료는 맨 뒤에)
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

    // ItemTable 한 줄 (재료 한 종류)
    [Serializable]
    public class ItemRow
    {
        public ItemType ItemID;
        public string Name;
        public float SpawnWeight;           // 선외 생성 비중

        public static ItemRow FromCells(Dictionary<string, string> cells)
        {
            return new ItemRow
            {
                ItemID = TsvParser.ToEnum<ItemType>(cells["ItemID"]),
                Name = cells["Name"],
                SpawnWeight = TsvParser.ToFloat(cells["SpawnWeight"]),
            };
        }
    }

    [Serializable]
    public class ItemCount
    {
        public ItemType ItemID;
        public int Count;
    }
}
