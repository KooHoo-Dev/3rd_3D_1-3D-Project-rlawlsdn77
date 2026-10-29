using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OverFixed
{
    // 일시정지 화면. 항해 중 Esc로 열고 닫는다. 열린 동안 시간을 멈추고(timeScale 0) 이동, 시점, 상호작용을 잠그고 커서를 보인다
    // Esc는 여기 한곳에서 처리한다. 터미널이나 미니게임 화면이 열려 있으면 그 화면만 닫는다. 정산, 게임오버 화면에서는 열지 않는다
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;     // 켜고 끄는 일시정지 화면
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button abandonButton;      // 항해 포기 · 타이틀로
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Terminal terminal;
        [SerializeField] private MiniGameUI miniGameUI;

        public bool IsPaused => pausePanel.activeSelf;

        // 일시정지 전 잠금과 커서 상태. 계속하기를 누르면 되돌린다 (에어락 감압 중이면 잠금이 그대로 남는다)
        private bool previousControlLocked;
        private bool previousLookLocked;
        private CursorLockMode previousCursorLockMode;
        private bool previousCursorVisible;

        private void Start()
        {
            resumeButton.onClick.AddListener(Resume);
            abandonButton.onClick.AddListener(AbandonVoyage);
            pausePanel.SetActive(false);
        }

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Escape) == false) return;

            if (terminal.IsTerminalOpen) terminal.CloseTerminal();
            else if (miniGameUI.IsOpen) miniGameUI.Close();
            else if (IsPaused) Resume();
            else if (GameManager.Instance.IsPlaying) Pause();
        }

        private void Pause()
        {
            previousControlLocked = playerController.IsControlLocked;
            previousLookLocked = playerController.IsLookLocked;
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            Time.timeScale = 0f;
            playerController.SetControlLock(true);
            playerController.SetLookLock(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pausePanel.SetActive(true);
        }

        private void Resume()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            playerController.SetControlLock(previousControlLocked);
            playerController.SetLookLock(previousLookLocked);
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }

        // 시간을 되돌린 뒤 최고 기록을 갱신하고 타이틀로 간다
        private void AbandonVoyage()
        {
            Time.timeScale = 1f;
            GameManager.Instance.AbandonVoyage();
        }
    }
}
