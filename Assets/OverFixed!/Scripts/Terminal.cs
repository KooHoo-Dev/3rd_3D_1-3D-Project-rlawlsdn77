using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    public class Terminal : MonoBehaviour
    {
        [SerializeField] private GameObject terminalPanel;  // 켜고 끄는 터미널 화면
        [SerializeField] private Button closeButton;
        [SerializeField] private PlayerController playerController; // 화면이 열린 동안 이동, 시점을 잠근다

        // 열림 여부는 화면이 켜져 있는지로 판단한다 (따로 변수를 두면 다른 곳에서 화면을 끌 때 어긋난다)
        public bool IsTerminalOpen => terminalPanel.activeSelf;

        // 화면을 열기 전 커서 상태. 닫을 때 되돌린다
        private CursorLockMode previousCursorLockMode;
        private bool previousCursorVisible;

        private void Start()
        {
            closeButton.onClick.AddListener(CloseTerminal);
        }

        // 터미널 앞에서 빈손으로 E: 꺼져 있으면 켜고, 켜져 있으면 끈다
        public void AccessTerminal()
        {
            if (IsTerminalOpen) CloseTerminal();
            else OpenTerminal();
        }

        private void OpenTerminal()
        {
            terminalPanel.SetActive(true);
            playerController.SetControlLock(true);
            playerController.SetLookLock(true);

            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 화면이 열린 동안은 E가 잠기므로 닫기 버튼이나 Esc(PauseUI가 처리)로 닫는다
        public void CloseTerminal()
        {
            if (IsTerminalOpen == false) return;

            terminalPanel.SetActive(false);
            playerController.SetControlLock(false);
            playerController.SetLookLock(false);

            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }

        // 재료를 주문에 넣는다. 맞는 주문이 없으면 false (재료는 손에 남는다)
        public bool SubmitItem(ItemObject item)
        {
            return OrderManager.Instance.SubmitItem(item.ItemID);
        }
    }


}
