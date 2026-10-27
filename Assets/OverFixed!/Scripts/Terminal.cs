using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    public class Terminal : MonoBehaviour
    {
        [SerializeField] private GameObject terminalPanel;  // 켜고 끄는 터미널 화면
        [SerializeField] private Button closeButton;
        
        [SerializeField] private List<GameObject> Items;
        
        [SerializeField] private OrderConfig orderConfig;
        
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

        public void SubmitItem(GameObject item)
        {
            Debug.Log($"{item.name} 을 제출하였습니다.");
            
            
        }

        public void CheckQuestItem(GameObject item)
        {
            
        }
    }
    
    
}
