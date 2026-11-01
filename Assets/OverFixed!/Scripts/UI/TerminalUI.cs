using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 터미널 화면의 값. 열려 있는 동안 상태 줄과 주문을 그리고, 할당량 납부와 안정화 물질 구매를 받는다
    // 화면을 열고 닫는 것은 Terminal
    public class TerminalUI : MonoBehaviour
    {
        [Serializable]
        private class OrderRowView
        {
            public TMP_Text itemsValue;     // "고철 1/2 · 크리스탈 0/1". 빈 칸이면 "새 주문"
            public TMP_Text timeValue;      // 빈 칸이면 새 주문까지 남은 시간
            public TMP_Text rewardValue;
        }

        [Serializable]
        private class PayStepButton
        {
            public Button button;
            public int step;                // 누르면 낼 금액에 더하는 값 (빼려면 음수)
        }

        [SerializeField] private GameObject terminalPanel;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private CoreConfig coreConfig;

        [Header("상태 줄")]
        [SerializeField] private TMP_Text timeValue;
        [SerializeField] private TMP_Text creditValue;
        [SerializeField] private TMP_Text quotaValue;
        [SerializeField] private TMP_Text stabilityValue;

        [Header("주문 보드 (OrderBoard의 칸 순서대로)")]
        [SerializeField] private OrderRowView[] orderRows;

        [Header("할당량 납부")]
        [SerializeField] private TMP_Text remainingQuotaValue;
        [SerializeField] private TMP_Text quotaCreditValue;
        [SerializeField] private TMP_Text payAmountValue;
        [SerializeField] private PayStepButton[] payStepButtons;
        [SerializeField] private Button payMaxButton;
        [SerializeField] private Button payButton;
        [SerializeField] private TMP_Text payLabel;

        [Header("안정화 물질")]
        [SerializeField] private TMP_Text recoveryValue;
        [SerializeField] private TMP_Text priceValue;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyLabel;

        private int payAmount;  // 납부 탭에서 고른 낼 금액

        private void Start()
        {
            recoveryValue.text = "+" + coreConfig.StabilizerRecovery + "%";
            priceValue.text = coreConfig.StabilizerPrice.ToString();
            buyButton.onClick.AddListener(() => GameManager.Instance.BuyStabilizer());

            foreach (PayStepButton payStepButton in payStepButtons)
            {
                int step = payStepButton.step;
                payStepButton.button.onClick.AddListener(() => payAmount += step);
            }
            payMaxButton.onClick.AddListener(() => payAmount = GameManager.Instance.MaxQuotaPayment);
            payButton.onClick.AddListener(PayQuota);
        }

        // 같은 프레임 Update에서 화면이 열려도 첫 화면부터 값이 보이도록 LateUpdate에서 그린다
        private void LateUpdate()
        {
            if (terminalPanel.activeSelf == false) return;

            GameManager gameManager = GameManager.Instance;
            timeValue.text = "항해는 계속 진행 중 · 도착까지 " + UIText.ToMinuteSecond(gameManager.RemainingTime);
            creditValue.text = gameManager.Credit.ToString();
            quotaValue.text = UIText.ToQuota(gameManager.PaidQuota, gameManager.Quota);
            stabilityValue.text = UIText.ToStability(gameManager.Stability);

            DrawOrders();
            DrawQuota(gameManager);

            buyButton.interactable = gameManager.CanBuyStabilizer;
            buyLabel.text = UIText.ToStabilizerBuyLabel(gameManager);
        }

        private void DrawOrders()
        {
            OrderBoard board = orderManager.Board;
            for (int slotIndex = 0; slotIndex < orderRows.Length; slotIndex++)
            {
                OrderRowView orderRow = orderRows[slotIndex];
                ActiveOrder order = board.Orders[slotIndex];
                if (order == null)
                {
                    orderRow.itemsValue.text = "새 주문";
                    orderRow.timeValue.text = Mathf.CeilToInt(board.GetRefillTime(slotIndex)) + "초";
                    orderRow.rewardValue.text = "";
                    continue;
                }
                orderRow.itemsValue.text = UIText.ToOrderItems(order);
                orderRow.timeValue.text = Mathf.CeilToInt(order.RemainingTime) + "초";
                orderRow.rewardValue.text = order.Row.Reward + " 크레딧";
            }
        }

        // 낼 금액은 낼 수 있는 만큼으로 자른다 (크레딧을 쓰거나 내서 줄면 같이 준다)
        private void DrawQuota(GameManager gameManager)
        {
            payAmount = Mathf.Clamp(payAmount, 0, gameManager.MaxQuotaPayment);

            remainingQuotaValue.text = gameManager.RemainingQuota.ToString();
            quotaCreditValue.text = gameManager.Credit.ToString();
            payAmountValue.text = payAmount.ToString();
            payButton.interactable = payAmount > 0;
            payLabel.text = gameManager.IsQuotaPaid ? "완납" : payAmount + " 크레딧 납부";
        }

        private void PayQuota()
        {
            GameManager.Instance.PayQuota(payAmount);
            payAmount = 0;
        }
    }
}
