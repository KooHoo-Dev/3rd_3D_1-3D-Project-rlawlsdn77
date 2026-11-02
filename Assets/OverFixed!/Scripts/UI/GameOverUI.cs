using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 게임오버 화면. 안정도 0%나 할당량 미납 도착(GameManager.GameOverHappened) 때 이유, 완료한 항해, 누적 크레딧, 최고 기록을 보여 준다
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button titleButton;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Terminal terminal;             // 열려 있으면 먼저 닫는다
        [SerializeField] private MiniGameUI miniGameUI;         // 열려 있으면 먼저 닫는다

        [Header("값 칸")]
        [SerializeField] private TMP_Text reasonValue;
        [SerializeField] private TMP_Text voyageCountValue;
        [SerializeField] private TMP_Text totalCreditValue;
        [SerializeField] private TMP_Text recordValue;          // "항해 n회 · n 크레딧"
        [SerializeField] private GameObject newBestMark;        // "최고 기록 갱신"

        // 씬을 넘어 살아남는 GameManager. 이 화면이 사라질 때 구독을 풀려고 들고 있는다
        private GameManager gameManager;

        private void Start()
        {
            gameManager = GameManager.Instance;
            gameManager.GameOverHappened += Show;
            restartButton.onClick.AddListener(gameManager.StartGame);
            titleButton.onClick.AddListener(gameManager.GoToTitle);
            gameOverPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (gameManager != null) gameManager.GameOverHappened -= Show;
        }

        private void Show()
        {
            terminal.CloseTerminal();
            miniGameUI.Close();
            playerController.LockForScreen();

            reasonValue.text = gameManager.LastGameOverReason == GameOverReason.QuotaUnpaid
                ? "할당량을 다 내지 못했습니다 (" + UIText.ToQuota(gameManager.PaidQuota, gameManager.Quota) + ")"
                : "코어 안정도가 0%가 되었습니다";
            voyageCountValue.text = gameManager.CompletedVoyageCount + "회";
            totalCreditValue.text = gameManager.TotalEarnedCredit.ToString();
            recordValue.text = "항해 " + gameManager.Record.BestVoyageCount + "회 · " + UIText.ToCredit(gameManager.Record.BestTotalCredit);
            newBestMark.SetActive(gameManager.IsNewRecord);
            gameOverPanel.SetActive(true);
        }
    }
}
