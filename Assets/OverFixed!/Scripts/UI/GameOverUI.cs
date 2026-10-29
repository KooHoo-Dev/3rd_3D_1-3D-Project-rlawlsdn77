using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 게임오버 화면. 코어 안정도가 0이 되면(GameManager.GameOverHappened) 완료한 항해, 누적 크레딧, 최고 기록을 보여 주고,
    // 다시 시작 버튼으로 새 게임을, 타이틀로 버튼으로 타이틀을 연다. 화면이 열린 동안 이동, 시점, 상호작용을 잠그고 커서를 보인다
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;      // 켜고 끄는 게임오버 화면
        [SerializeField] private Button restartButton;
        [SerializeField] private Button titleButton;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Terminal terminal;             // 열려 있으면 먼저 닫는다
        [SerializeField] private MiniGameUI miniGameUI;         // 열려 있으면 먼저 닫는다

        [Header("값 칸")]
        [SerializeField] private TMP_Text voyageCountValue;     // 완료한 항해
        [SerializeField] private TMP_Text totalCreditValue;     // 누적 크레딧
        [SerializeField] private TMP_Text recordValue;          // 최고 기록 "항해 n회 · n 크레딧"
        [SerializeField] private GameObject newBestMark;        // "최고 기록 갱신" 표시

        // 씬이 바뀌어도 살아남는 GameManager. 이 화면이 사라질 때 구독을 풀려고 들고 있는다
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
            playerController.SetControlLock(true);
            playerController.SetLookLock(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            voyageCountValue.text = gameManager.CompletedVoyageCount + "회";
            totalCreditValue.text = gameManager.TotalEarnedCredit.ToString();
            recordValue.text = "항해 " + gameManager.Record.BestVoyageCount + "회 · " + gameManager.Record.BestTotalCredit + " 크레딧";
            newBestMark.SetActive(gameManager.IsNewRecord);
            gameOverPanel.SetActive(true);
        }
    }
}
