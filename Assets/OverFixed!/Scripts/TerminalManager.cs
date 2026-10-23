using System;
using Jay;
using Study.Utilities;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    public class TerminalManager : MonoBehaviour
    {
        [SerializeField] private Canvas TerminalCanvas;
        
        private bool IsTerminal = false;

        public void AccessTerminal()
        {
            // 터미널이 꺼져있는 상태면 키고, 켜져있는 상태면 끈다.
            TerminalCanvas.gameObject.SetActive(IsTerminal == false);
            
            // 터미널 On/Off 상태 변경
            IsTerminal = !IsTerminal;
        }

        [SerializeField] private Button OrderButton;
        [SerializeField] private Button QuotaButton;
        [SerializeField] private Button StabilizerButton;
        [SerializeField] private Button ModulesButton;
        
        private void Start()
        {
            OrderButton.onClick.AddListener(AccessTerminal);
            QuotaButton.onClick.AddListener(AccessTerminal);
            StabilizerButton.onClick.AddListener(AccessTerminal);
            ModulesButton.onClick.AddListener(AccessTerminal);
        }

        private void Update()
        {
        }
            
    }
}