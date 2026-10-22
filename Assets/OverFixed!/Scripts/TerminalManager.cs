using System;
using Jay;
using Study.Utilities;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace OverFixed
{
    public class TerminalManager : SingletonBase<TerminalManager>
    {
        [SerializeField] private Canvas TerminalCanvas;

        [SerializeField] private Camera TerminalCamera;
        
        private bool IsTerminal = false;

        public void AccessTerminal()
        {
            // 터미널이 꺼져있는 상태면 키고, 켜져있는 상태면 끈다.
            TerminalCanvas.gameObject.SetActive(IsTerminal == false);
            
            // 터미널 On/Off 상태 변경
            IsTerminal = IsTerminal == false;
        }
    }
}