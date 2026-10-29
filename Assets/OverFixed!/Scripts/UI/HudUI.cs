using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 항해 HUD. 코어 안정도, 크레딧, 도착까지 남은 시간, 주문 3칸, 손에 든 재료, 패널 사고 알림을 그린다
    // 크레딧과 주문 내용은 바뀔 때 알림을 받아 그리고, 계속 줄어드는 시간과 안정도는 Update에서 그린다
    // 막대는 Fill의 anchorMax.x로 채운다 (HoldBarUI와 같은 방식)
    public class HudUI : MonoBehaviour
    {
        // 주문 카드 한 장의 칸
        [Serializable]
        private class OrderCard
        {
            public TMP_Text itemsValue;     // "고철 1/2 · 크리스탈 0/1". 빈 칸이면 "새 주문"
            public TMP_Text rewardValue;    // "30 CR"
            public TMP_Text timeValue;      // 남은 시간. 빈 칸이면 새 주문까지 남은 시간
            public RectTransform timeFill;  // 남은 시간 비율
        }

        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private AccountManager accountManager;
        [SerializeField] private OrderManager orderManager;

        [Header("설정값 (CarryCount로 손 칸 수, MaxStability로 막대 길이, WarningThreshold로 경고 색)")]
        [SerializeField] private PlayerControlConfig playerControlConfig;
        [SerializeField] private CoreConfig coreConfig;

        [Header("코어 안정도, 크레딧")]
        [SerializeField] private TMP_Text stabilityValue;
        [SerializeField] private RectTransform stabilityFill;
        [SerializeField] private Color stabilityWarningColor = Color.red;   // WarningThreshold 이하일 때 안정도 글자와 막대 색
        [SerializeField] private TMP_Text creditValue;

        [Header("도착까지 남은 시간")]
        [SerializeField] private TMP_Text timeValue;
        [SerializeField] private RectTransform routeFill;   // 항해 진행 비율

        [Header("주문 (OrderBoard의 칸 순서대로)")]
        [SerializeField] private OrderCard[] orderCards;

        [Header("손")]
        [SerializeField] private TMP_Text handsTitleValue;  // "손 0/1"
        [SerializeField] private TMP_Text handsValue;       // 재료 이름. 빈손이면 "빈손"

        [Header("패널 사고 알림 (사고 중에만 보인다)")]
        [SerializeField] private Panel panel;
        [SerializeField] private GameObject accidentBox;
        [SerializeField] private TMP_Text accidentValue;    // "패널 고장 · 25s"

        private ItemObject shownItem;   // 지금 손 칸에 그려진 재료. 바뀔 때만 다시 그린다

        // 경고가 아닐 때 안정도 글자와 막대 색 (프리팹에 정해 둔 색)
        private Image stabilityFillImage;
        private Color stabilityTextColor;
        private Color stabilityFillColor;

        private void Start()
        {
            stabilityFillImage = stabilityFill.GetComponent<Image>();
            stabilityTextColor = stabilityValue.color;
            stabilityFillColor = stabilityFillImage.color;

            accountManager.CreditChanged += DrawCredit;
            orderManager.Board.OrdersChanged += DrawOrders;

            DrawCredit(accountManager.CurrentCredit);
            DrawOrders();
            DrawHands(localPlayer.Item);
        }

        private void OnDestroy()
        {
            accountManager.CreditChanged -= DrawCredit;
            orderManager.Board.OrdersChanged -= DrawOrders;
        }

        private void Update()
        {
            DrawStability();
            DrawRemainingTime();
            DrawOrderTimes();
            if (localPlayer.Item != shownItem) DrawHands(localPlayer.Item);
            DrawAccident();
        }

        private void DrawCredit(int credit)
        {
            creditValue.text = credit.ToString();
        }

        // MaxStability가 가득 찬 막대. WarningThreshold 이하면 경고 색
        private void DrawStability()
        {
            float stability = GameManager.Instance.Stability;
            stabilityValue.text = UIText.ToStability(stability);
            SetFill(stabilityFill, stability / coreConfig.MaxStability);

            bool isWarning = stability <= coreConfig.WarningThreshold;
            stabilityValue.color = isWarning ? stabilityWarningColor : stabilityTextColor;
            stabilityFillImage.color = isWarning ? stabilityWarningColor : stabilityFillColor;
        }

        private void DrawRemainingTime()
        {
            float remainingTime = GameManager.Instance.RemainingTime;
            timeValue.text = "도착까지 " + UIText.ToMinuteSecond(remainingTime);
            SetFill(routeFill, 1f - remainingTime / GameManager.Instance.Voyage.VoyageTime);
        }

        // 주문 내용(재료, 보상). 주문이 생기거나, 채워지거나, 끝났을 때
        private void DrawOrders()
        {
            IReadOnlyList<ActiveOrder> orders = orderManager.Board.Orders;
            for (int slotIndex = 0; slotIndex < orderCards.Length; slotIndex++)
            {
                OrderCard card = orderCards[slotIndex];
                ActiveOrder order = orders[slotIndex];
                if (order == null)
                {
                    card.itemsValue.text = "새 주문";
                    card.rewardValue.text = "";
                    continue;
                }
                card.itemsValue.text = UIText.ToOrderItems(order);
                card.rewardValue.text = order.Row.Reward + " CR";
            }
        }

        // 주문 시간. 빈 칸은 새 주문이 들어오기까지 남은 시간
        private void DrawOrderTimes()
        {
            OrderBoard board = orderManager.Board;
            for (int slotIndex = 0; slotIndex < orderCards.Length; slotIndex++)
            {
                OrderCard card = orderCards[slotIndex];
                ActiveOrder order = board.Orders[slotIndex];
                if (order == null)
                {
                    card.timeValue.text = Mathf.CeilToInt(board.GetRefillTime(slotIndex)) + "s";
                    SetFill(card.timeFill, 0f);
                    continue;
                }
                card.timeValue.text = Mathf.CeilToInt(order.RemainingTime) + "s";
                SetFill(card.timeFill, order.RemainingTime / order.Row.TimeLimit);
            }
        }

        private void DrawHands(ItemObject heldItem)
        {
            shownItem = heldItem;
            int heldCount = heldItem == null ? 0 : 1;
            handsTitleValue.text = "손 " + heldCount + "/" + playerControlConfig.CarryCount;
            handsValue.text = heldItem == null ? "빈손" : heldItem.Row.Name;
        }

        // 사고 중에만 알림 칸을 켜고 제한시간까지 남은 시간을 그린다
        private void DrawAccident()
        {
            accidentBox.SetActive(panel.IsAccident);
            if (panel.IsAccident) accidentValue.text = "패널 고장 · " + Mathf.CeilToInt(panel.RemainingTime) + "s";
        }

        private static void SetFill(RectTransform fill, float rate)
        {
            fill.anchorMax = new Vector2(Mathf.Clamp01(rate), fill.anchorMax.y);
        }
    }
}
