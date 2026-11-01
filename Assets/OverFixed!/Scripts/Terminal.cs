using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 터미널 화면을 열고 닫는다. 화면 안의 값은 TerminalUI가 그린다
    public class Terminal : MonoBehaviour
    {
        [SerializeField] private GameObject terminalPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private PlayerController playerController;

        // 따로 변수를 두지 않고 화면이 켜져 있는지로 본다 (다른 곳에서 화면을 꺼도 어긋나지 않게)
        public bool IsTerminalOpen => terminalPanel.activeSelf;

        private void Start()
        {
            closeButton.onClick.AddListener(CloseTerminal);
            terminalPanel.SetActive(false);
        }

        // 빈손으로 E
        public void OpenTerminal()
        {
            if (IsTerminalOpen) return;

            terminalPanel.SetActive(true);
            playerController.LockForScreen();
        }

        // 닫기 버튼, Esc(PauseUI), 정산·게임오버 화면
        public void CloseTerminal()
        {
            if (IsTerminalOpen == false) return;

            terminalPanel.SetActive(false);
            playerController.UnlockForScreen();
        }
    }
}
