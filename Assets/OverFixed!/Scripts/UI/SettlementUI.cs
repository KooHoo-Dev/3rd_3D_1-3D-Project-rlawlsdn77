using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 정산 화면. 항해 시간이 다 되면(GameManager.VoyageEnded) 이번 항해 기록을 보여 주고, 다음 버튼으로 다음 항해를 시작한다
    // 화면이 열린 동안 이동, 시점, 상호작용을 잠그고 커서를 보인다
    public class SettlementUI : MonoBehaviour
    {
        [SerializeField] private GameObject settlementPanel;    // 켜고 끄는 정산 화면
        [SerializeField] private Button nextButton;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Terminal terminal;             // 열려 있으면 먼저 닫는다

        [Header("값 칸")]
        [SerializeField] private TMP_Text earnedCreditValue;    // 이번 항해에 번 크레딧
        [SerializeField] private TMP_Text orderValue;           // 주문 성공 / 실패
        [SerializeField] private TMP_Text stabilityValue;       // 남은 안정도
        [SerializeField] private TMP_Text voyageCountValue;     // 완료한 항해

        // 씬이 바뀌어도 살아남는 GameManager. 이 화면이 사라질 때 구독을 풀려고 들고 있는다
        private GameManager gameManager;

        private void Start()
        {
            gameManager = GameManager.Instance;
            gameManager.VoyageEnded += Show;
            nextButton.onClick.AddListener(gameManager.StartNextVoyage);
            settlementPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.VoyageEnded -= Show;
        }

        private void Show()
        {
            terminal.CloseTerminal();
            playerController.SetControlLock(true);
            playerController.SetLookLock(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            earnedCreditValue.text = "+" + gameManager.VoyageEarnedCredit + " 크레딧";
            orderValue.text = gameManager.OrderSuccessCount + " / " + gameManager.OrderFailCount;
            stabilityValue.text = Mathf.CeilToInt(gameManager.Stability) + "%";     // HUD와 같은 표기 (올림 정수)
            voyageCountValue.text = gameManager.VoyageIndex.ToString();
            settlementPanel.SetActive(true);
        }
    }
}
