using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 타이틀 화면 (MainMenu 씬). 출항 시작, 종료, 최고 기록
    public class TitleUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        [Header("최고 기록")]
        [SerializeField] private TMP_Text recordValue;          // "항해 n회"
        [SerializeField] private TMP_Text totalCreditValue;     // "누적 크레딧 n"

        private void Start()
        {
            GameManager gameManager = GameManager.Instance;
            startButton.onClick.AddListener(gameManager.StartGame);
            quitButton.onClick.AddListener(Application.Quit);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            recordValue.text = "항해 " + gameManager.Record.BestVoyageCount + "회";
            totalCreditValue.text = "누적 크레딧 " + gameManager.Record.BestTotalCredit;
        }
    }
}
