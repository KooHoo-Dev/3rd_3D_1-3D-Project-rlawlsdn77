using System.Collections.Generic;
using UnityEngine;

namespace OverFixed
{
    // 여러 화면이 같은 값을 같은 모양으로 보이도록 모은 표기
    public static class UIText
    {
        // "3:58"
        public static string ToMinuteSecond(float seconds)
        {
            int remainingSeconds = Mathf.CeilToInt(seconds);
            return remainingSeconds / 60 + ":" + (remainingSeconds % 60).ToString("00");
        }

        // 올림한 정수 "83%"
        public static string ToStability(float stability)
        {
            return Mathf.CeilToInt(stability) + "%";
        }

        // 올림한 초 "29초"
        public static string ToSeconds(float seconds)
        {
            return Mathf.CeilToInt(seconds) + "초";
        }

        // "75 크레딧"
        public static string ToCredit(int credit)
        {
            return credit + " 크레딧";
        }

        // 낸 할당량 / 할당량 "150 / 200"
        public static string ToQuota(int paidQuota, int quota)
        {
            return paidQuota + " / " + quota;
        }

        // "고철 1/2 · 크리스탈 0/1"
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

        // 안정화 물질 구매 버튼 (터미널, PLANET 정비)
        public static string ToStabilizerBuyLabel(GameManager gameManager)
        {
            if (gameManager.IsStabilityFull) return "안정도 최대";
            if (gameManager.CanBuyStabilizer == false) return "크레딧 부족";
            return "구매";
        }
    }
}
