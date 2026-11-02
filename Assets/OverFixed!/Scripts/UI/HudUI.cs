using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 항해 HUD. 안정도, 크레딧, 할당량, 도착까지 남은 시간, 주문 3칸, 손, 사고 알림(사고 이름, 남은 시간)을 매 프레임 그린다
    // 막대는 Fill의 anchorMax.x로 채운다
    public class HudUI : MonoBehaviour
    {
        [Serializable]
        private class OrderSlotView
        {
            public TMP_Text itemsValue;     // "고철 1/2 · 크리스탈 0/1". 빈 칸이면 "새 주문"
            public TMP_Text rewardValue;
            public TMP_Text timeValue;      // 빈 칸이면 새 주문까지 남은 시간
            public RectTransform timeFill;
        }

        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private AccidentPanel accidentPanel;
        [SerializeField] private PlayerControlConfig playerControlConfig;
        [SerializeField] private CoreConfig coreConfig;

        [Header("안정도, 크레딧, 할당량, 도착까지")]
        [SerializeField] private TMP_Text stabilityValue;
        [SerializeField] private RectTransform stabilityFill;
        [SerializeField] private Color stabilityWarningColor = Color.red;   // WarningThreshold 이하일 때
        [SerializeField] private TMP_Text creditValue;
        [SerializeField] private TMP_Text quotaValue;
        [SerializeField] private RectTransform quotaFill;
        [SerializeField] private TMP_Text timeValue;
        [SerializeField] private RectTransform routeFill;

        [Header("주문 (OrderBoard의 칸 순서대로)")]
        [SerializeField] private OrderSlotView[] orderSlots;

        [Header("손, 사고 알림")]
        [SerializeField] private TMP_Text handsTitleValue;  // "손 0/1"
        [SerializeField] private TMP_Text handsValue;
        [SerializeField] private GameObject accidentBox;
        [SerializeField] private TMP_Text accidentValue;

        // 경고가 아닐 때의 색 (프리팹에 정해 둔 색)
        private Image stabilityFillImage;
        private Color stabilityTextColor;
        private Color stabilityFillColor;

        private void Start()
        {
            stabilityFillImage = stabilityFill.GetComponent<Image>();
            stabilityTextColor = stabilityValue.color;
            stabilityFillColor = stabilityFillImage.color;
        }

        private void Update()
        {
            GameManager gameManager = GameManager.Instance;

            DrawStability(gameManager.Stability);
            creditValue.text = gameManager.Credit.ToString();
            quotaValue.text = UIText.ToQuota(gameManager.PaidQuota, gameManager.Quota);
            SetFill(quotaFill, (float)gameManager.PaidQuota / gameManager.Quota);
            timeValue.text = "도착까지 " + UIText.ToMinuteSecond(gameManager.RemainingTime);
            SetFill(routeFill, 1f - gameManager.RemainingTime / gameManager.Voyage.VoyageTime);

            DrawOrders();
            DrawHands();
            DrawAccident(gameManager.IsPlaying);
        }

        private void DrawStability(float stability)
        {
            stabilityValue.text = UIText.ToStability(stability);
            SetFill(stabilityFill, stability / coreConfig.MaxStability);

            bool isWarning = stability <= coreConfig.WarningThreshold;
            stabilityValue.color = isWarning ? stabilityWarningColor : stabilityTextColor;
            stabilityFillImage.color = isWarning ? stabilityWarningColor : stabilityFillColor;
        }

        private void DrawOrders()
        {
            OrderBoard board = orderManager.Board;
            for (int slotIndex = 0; slotIndex < orderSlots.Length; slotIndex++)
            {
                OrderSlotView orderSlot = orderSlots[slotIndex];
                ActiveOrder order = board.Orders[slotIndex];
                if (order == null)
                {
                    orderSlot.itemsValue.text = "새 주문";
                    orderSlot.rewardValue.text = "";
                    orderSlot.timeValue.text = UIText.ToSeconds(board.GetRefillTime(slotIndex));
                    SetFill(orderSlot.timeFill, 0f);
                    continue;
                }
                orderSlot.itemsValue.text = UIText.ToOrderItems(order);
                orderSlot.rewardValue.text = UIText.ToCredit(order.Row.Reward);
                orderSlot.timeValue.text = UIText.ToSeconds(order.RemainingTime);
                SetFill(orderSlot.timeFill, order.RemainingTime / order.Row.TimeLimit);
            }
        }

        private void DrawHands()
        {
            ItemObject heldItem = localPlayer.Item;
            handsTitleValue.text = "손 " + (heldItem == null ? 0 : 1) + "/" + playerControlConfig.CarryCount;
            handsValue.text = heldItem == null ? "빈손" : heldItem.Row.Name;
        }

        // 항해가 끝나면(정산, 게임오버) 사고가 남아 있어도 화면 뒤에 보이지 않게 끈다
        private void DrawAccident(bool isPlaying)
        {
            bool isShown = accidentPanel.IsAccident && isPlaying;
            accidentBox.SetActive(isShown);
            if (isShown) accidentValue.text = accidentPanel.CurrentAccident.Name + " · " + UIText.ToSeconds(accidentPanel.RemainingTime);
        }

        private static void SetFill(RectTransform fill, float rate)
        {
            fill.anchorMax = new Vector2(Mathf.Clamp01(rate), fill.anchorMax.y);
        }
    }
}
