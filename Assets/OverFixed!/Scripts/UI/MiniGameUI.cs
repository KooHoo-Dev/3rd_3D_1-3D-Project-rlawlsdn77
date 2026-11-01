using System.Collections.Generic;
using Study.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OverFixed
{
    // 미니게임 화면. 패널 사고 중 E로 열고, 사고 종류에 맞는 미니게임을 보여 주며 성공 1회를 Panel에 넘긴다
    // Q, 닫기 버튼, Esc(PauseUI)로 닫는다. 닫아도 사고, 남은 시간, 성공 횟수는 Panel에 남아 다시 열면 이어서 한다
    public class MiniGameUI : MonoBehaviour
    {
        [SerializeField] private GameObject miniGamePanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private PlayerController playerController;

        [Header("미니게임 (사고 종류마다 하나)")]
        [SerializeField] private MiniGame[] miniGames;

        [Header("값 칸")]
        [SerializeField] private TMP_Text titleValue;
        [SerializeField] private TMP_Text descValue;
        [SerializeField] private TMP_Text timeValue;
        [SerializeField] private TMP_Text successValue;     // "1 / 3"

        public bool IsOpen => miniGamePanel.activeSelf;

        private Panel panel;                // 지금 화면을 연 사고
        private MiniGame currentMiniGame;

        private void Start()
        {
            closeButton.onClick.AddListener(Close);
            foreach (MiniGame miniGame in miniGames) miniGame.gameObject.SetActive(false);
            miniGamePanel.SetActive(false);
        }

        private void Update()
        {
            if (IsOpen == false) return;

            if (SimpleInput.GetKeyDown(Key.Q))
            {
                Close();
                return;
            }
            DrawProgress();
        }

        public void Open(Panel targetPanel)
        {
            if (IsOpen) return;

            panel = targetPanel;
            EventRow eventRow = panel.CurrentEvent;
            titleValue.text = eventRow.Name;
            descValue.text = eventRow.Desc;
            miniGamePanel.SetActive(true);
            playerController.LockForScreen();
            DrawProgress();

            currentMiniGame = FindMiniGame(eventRow.EventID);
            currentMiniGame.Succeeded += panel.AddSuccess;
            currentMiniGame.Begin(eventRow, panel.SuccessCount);
        }

        // 해결, 시간 초과, 정산·게임오버 화면, Q·Esc·닫기 버튼
        public void Close()
        {
            if (IsOpen == false) return;

            currentMiniGame.Succeeded -= panel.AddSuccess;
            currentMiniGame.End();
            currentMiniGame = null;
            miniGamePanel.SetActive(false);
            playerController.UnlockForScreen();
        }

        private void DrawProgress()
        {
            timeValue.text = "남은 시간 " + Mathf.CeilToInt(panel.RemainingTime) + "s";
            successValue.text = panel.SuccessCount + " / " + panel.CurrentEvent.SuccessCount;
        }

        // 없으면 예외를 내서 미니게임 누락을 바로 알 수 있게 한다
        private MiniGame FindMiniGame(EventType eventId)
        {
            foreach (MiniGame miniGame in miniGames)
            {
                if (miniGame.EventID == eventId) return miniGame;
            }
            throw new KeyNotFoundException("미니게임 화면에 " + eventId + " 미니게임이 없습니다.");
        }
    }
}
