using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 터미널 화면의 값. 화면이 열려 있는 동안 HUD와 같은 값(도착까지 시간, 크레딧, 안정도, 주문 3칸)을 그리고,
    // 안정화 물질 탭에서 크레딧으로 안정도를 산다 (CoreConfig의 StabilizerPrice, StabilizerRecovery)
    // 화면을 열고 닫는 것은 Terminal이 맡는다
    public class TerminalUI : MonoBehaviour
    {
        // 주문 보드 한 줄의 칸
        [Serializable]
        private class OrderRowView
        {
            public TMP_Text itemsValue;     // "고철 1/2 · 크리스탈 0/1". 빈 칸이면 "새 주문"
            public TMP_Text timeValue;      // "60초". 빈 칸이면 새 주문까지 남은 시간
            public TMP_Text rewardValue;    // "30 크레딧"
        }

        [SerializeField] private GameObject terminalPanel;      // Terminal이 켜고 끄는 화면
        [SerializeField] private AccountManager accountManager;
        [SerializeField] private OrderManager orderManager;

        [Header("설정값 (StabilizerPrice로 가격, StabilizerRecovery로 회복량)")]
        [SerializeField] private CoreConfig coreConfig;

        [Header("상태 줄")]
        [SerializeField] private TMP_Text timeValue;            // "항해는 계속 진행 중 · 도착까지 3:58"
        [SerializeField] private TMP_Text creditValue;
        [SerializeField] private TMP_Text stabilityValue;

        [Header("주문 보드 (OrderBoard의 칸 순서대로)")]
        [SerializeField] private OrderRowView[] orderRows;

        [Header("안정화 물질")]
        [SerializeField] private TMP_Text recoveryValue;        // "+10%"
        [SerializeField] private TMP_Text priceValue;           // "60"
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyLabel;             // "구매" / "크레딧 부족" / "안정도 최대"

        private void Start()
        {
            buyButton.onClick.AddListener(BuyStabilizer);
            recoveryValue.text = "+" + coreConfig.StabilizerRecovery + "%";
            priceValue.text = coreConfig.StabilizerPrice.ToString();
        }

        // 열려 있는 동안만 그린다 (값이 계속 바뀌므로 매 프레임)
        // 같은 프레임의 Update에서 화면이 열려도 첫 화면부터 실제 값이 보이도록 LateUpdate에서 그린다
        private void LateUpdate()
        {
            if (terminalPanel.activeSelf == false) return;

            DrawStatus();
            DrawOrders();
            DrawStabilizer();
        }

        private void DrawStatus()
        {
            GameManager gameManager = GameManager.Instance;
            timeValue.text = "항해는 계속 진행 중 · 도착까지 " + UIText.ToMinuteSecond(gameManager.RemainingTime);
            creditValue.text = gameManager.Credit.ToString();
            stabilityValue.text = UIText.ToStability(gameManager.Stability);
        }

        private void DrawOrders()
        {
            OrderBoard board = orderManager.Board;
            IReadOnlyList<ActiveOrder> orders = board.Orders;
            for (int slotIndex = 0; slotIndex < orderRows.Length; slotIndex++)
            {
                OrderRowView orderRow = orderRows[slotIndex];
                ActiveOrder order = orders[slotIndex];
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

        // 살 수 없으면 버튼을 막고 이유를 보인다
        private void DrawStabilizer()
        {
            GameManager gameManager = GameManager.Instance;
            buyButton.interactable = gameManager.CanBuyStabilizer;
            if (gameManager.IsStabilityFull) buyLabel.text = "안정도 최대";
            else if (gameManager.Credit < coreConfig.StabilizerPrice) buyLabel.text = "크레딧 부족";
            else buyLabel.text = "구매";
        }

        private void BuyStabilizer()
        {
            accountManager.BuyStabilizer();
        }
    }
}
