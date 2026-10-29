using System.Collections.Generic;
using UnityEngine;

namespace OverFixed
{
    // HUD, 터미널, 정산 화면이 같은 값을 같은 모양으로 보이도록 모은 표기
    public static class UIText
    {
        // 초 를 매개변수로 받아서 "분 : 초" 형태로 리턴한다.
        public static string ToMinuteSecond(float seconds)
        {
            int remainingSeconds = Mathf.CeilToInt(seconds);
            return remainingSeconds / 60 + ":" + (remainingSeconds % 60).ToString("00");
        }

        // 정수로 올림. ex.) 코어 안정도 "83%" 
        public static string ToStability(float stability)
        {
            return Mathf.CeilToInt(stability) + "%";
        }

        // 주문 재료 "고철 1/2 · 크리스탈 0/1"
        public static string ToOrderItems(ActiveOrder order)
        {
            var itemTexts = new List<string>();
            foreach (ItemCount requireItem in order.Row.RequireItems)
            {
                string itemName = DataManager.GetItem(requireItem.ItemID).Name;
                itemTexts.Add(itemName + " " + order.GetFilledCount(requireItem.ItemID) + "/" + requireItem.Count);
            }
            return string.Join(" · ", itemTexts);
        }
    }
}
