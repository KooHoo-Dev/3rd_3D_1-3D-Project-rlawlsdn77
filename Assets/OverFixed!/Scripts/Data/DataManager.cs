using System.Collections.Generic;
using UnityEngine;

namespace OverFixed
{
    // 밸런스 테이블(TSV) 창구. 씬에 놓지 않는 정적 클래스로, 처음 부를 때 Resources/Tsv에서 읽는다
    // 다른 스크립트는 숫자를 직접 갖지 않고 여기서 행 데이터를 받아 쓴다
    public static class DataManager
    {
        private const string TsvFolder = "Tsv/";

        private static Dictionary<ItemType, ItemRow> itemTable;
        private static List<OrderRow> orderTable;
        private static List<VoyageRow> voyageTable;

        // 재료 한 종류의 행. 테이블에 없는 재료면 예외가 나서 TSV 누락을 바로 알 수 있다
        public static ItemRow GetItem(ItemType itemId)
        {
            if (itemTable == null) LoadItemTable();
            return itemTable[itemId];
        }

        // 재료 테이블 전체 (생성 비율 계산처럼 모든 행이 필요할 때)
        public static IReadOnlyCollection<ItemRow> GetItemRows()
        {
            if (itemTable == null) LoadItemTable();
            return itemTable.Values;
        }

        private static void LoadItemTable()
        {
            itemTable = new Dictionary<ItemType, ItemRow>();
            foreach (ItemRow itemRow in TsvParser.ParseRows(LoadTsv("ItemTable"), ItemRow.FromCells))
            {
                itemTable[itemRow.ItemID] = itemRow;
            }
        }

        // 주문 테이블 전체 (주문을 뽑을 때 후보 목록으로 쓴다)
        public static IReadOnlyList<OrderRow> GetOrderRows()
        {
            if (orderTable == null) orderTable = TsvParser.ParseRows(LoadTsv("OrderTable"), OrderRow.FromCells);
            return orderTable;
        }

        // 항해 번호(1부터)의 행. 표의 마지막 항해보다 뒤면 마지막 행을 쓴다
        public static VoyageRow GetVoyage(int voyageIndex)
        {
            if (voyageTable == null) voyageTable = TsvParser.ParseRows(LoadTsv("VoyageTable"), VoyageRow.FromCells);
            int rowIndex = Mathf.Clamp(voyageIndex, 1, voyageTable.Count) - 1;
            return voyageTable[rowIndex];
        }

        private static string LoadTsv(string tableName)
        {
            TextAsset tsvAsset = Resources.Load<TextAsset>(TsvFolder + tableName);
            if (tsvAsset == null) throw new KeyNotFoundException("Resources/" + TsvFolder + tableName + ".tsv 파일이 없습니다.");
            return tsvAsset.text;
        }
    }
}
