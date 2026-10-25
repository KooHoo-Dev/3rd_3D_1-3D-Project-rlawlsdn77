using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 터미널 화면 열고 닫기
    // 화면 안의 탭 전환은 UITabGroup이 맡는다. 이 스크립트는 탭 버튼을 건드리지 않는다
    public class TerminalManager : MonoBehaviour
    {
        [SerializeField] private GameObject terminalPanel;  // 켜고 끄는 터미널 화면
        [SerializeField] private Button closeButton;

        // 열림 여부는 화면이 켜져 있는지로 판단한다 (따로 변수를 두면 다른 곳에서 화면을 끌 때 어긋난다)
        public bool IsTerminalOpen => terminalPanel.activeSelf;

        private void Start()
        {
            closeButton.onClick.AddListener(CloseTerminal);
        }

        // 터미널 앞에서 E: 꺼져 있으면 켜고, 켜져 있으면 끈다
        public void AccessTerminal()
        {
            terminalPanel.SetActive(IsTerminalOpen == false);
        }

        public void CloseTerminal()
        {
            terminalPanel.SetActive(false);
        }
    }
}
