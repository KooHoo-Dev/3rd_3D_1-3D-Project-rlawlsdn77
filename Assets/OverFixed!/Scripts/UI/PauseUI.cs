using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OverFixed
{
    // 일시정지 화면. 항해 중 Esc로 열고 닫는다. 열린 동안 시간을 멈춘다(timeScale 0)
    // Esc는 여기 한곳에서 받는다. 터미널이나 미니게임 화면이 열려 있으면 그 화면만 닫는다
    public class PauseUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button abandonButton;      // 항해 포기 · 타이틀로
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Terminal terminal;
        [SerializeField] private MiniGameUI miniGameUI;

        public bool IsPaused => pausePanel.activeSelf;

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
            Time.timeScale = 0f;
            playerController.LockForScreen();
            pausePanel.SetActive(true);
        }

        private void Resume()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            playerController.UnlockForScreen();
        }

        private void AbandonVoyage()
        {
            Time.timeScale = 1f;
            GameManager.Instance.AbandonVoyage();
        }
    }
}
