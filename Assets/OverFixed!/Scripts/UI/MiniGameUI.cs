using System.Collections.Generic;
using Study.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OverFixed
{
    // 미니게임 화면. 패널 사고 중 E로 열고, 사고 종류에 맞는 미니게임(MiniGame)을 내용 영역에 보여 준다
    // 제목, 설명, 남은 시간, 성공 횟수를 그리고, 미니게임의 성공 1회를 Panel에 넘긴다
    // 열린 동안 이동, 시점, 상호작용을 잠그고 커서를 보인다 (항해 시간은 계속 흐른다)
    // Q, 닫기 버튼, Esc(PauseUI가 처리)로 닫는다. 닫아도 사고, 남은 시간, 성공 횟수는 Panel에 남아 다시 열면 이어서 한다
    public class MiniGameUI : MonoBehaviour
    {
        [SerializeField] private GameObject miniGamePanel;      // 켜고 끄는 미니게임 화면
        [SerializeField] private Button closeButton;
        [SerializeField] private PlayerController playerController;

        [Header("내용 영역의 미니게임 (사고 종류마다 하나)")]
        [SerializeField] private MiniGame[] miniGames;

        [Header("값 칸")]
        [SerializeField] private TMP_Text titleValue;          // 사고 이름
        [SerializeField] private TMP_Text descValue;           // 설명
        [SerializeField] private TMP_Text timeValue;           // "남은 시간 25s"
        [SerializeField] private TMP_Text successValue;        // "1 / 3"

        // 열림 여부는 화면이 켜져 있는지로 판단한다 (Terminal과 같은 방식)
        public bool IsOpen => miniGamePanel.activeSelf;

        private Panel panel;                // 지금 화면을 연 사고
        private MiniGame currentMiniGame;   // 지금 내용 영역에 보이는 미니게임

        // 화면을 열기 전 잠금과 커서 상태. 닫을 때 되돌린다
        private bool previousControlLocked;
        private bool previousLookLocked;
        private CursorLockMode previousCursorLockMode;
        private bool previousCursorVisible;

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

            previousControlLocked = playerController.IsControlLocked;
            previousLookLocked = playerController.IsLookLocked;
            previousCursorLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            playerController.SetControlLock(true);
            playerController.SetLookLock(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            miniGamePanel.SetActive(true);
            DrawProgress();

            currentMiniGame = FindMiniGame(eventRow.EventID);
            currentMiniGame.Succeeded += panel.AddSuccess;
            currentMiniGame.Begin(eventRow, panel.SuccessCount);
        }

        // 해결, 시간 초과, 정산·게임오버 화면, Q·Esc·닫기 버튼에서 부른다. 닫혀 있으면 아무것도 하지 않는다
        public void Close()
        {
            if (IsOpen == false) return;

            currentMiniGame.Succeeded -= panel.AddSuccess;
            currentMiniGame.End();
            currentMiniGame = null;
            miniGamePanel.SetActive(false);

            playerController.SetControlLock(previousControlLocked);
            playerController.SetLookLock(previousLookLocked);
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
        }

        private void DrawProgress()
        {
            timeValue.text = "남은 시간 " + Mathf.CeilToInt(panel.RemainingTime) + "s";
            successValue.text = panel.SuccessCount + " / " + panel.CurrentEvent.SuccessCount;
        }

        // 사고 종류를 맡는 미니게임. 없으면 예외가 나서 내용 영역의 누락을 바로 알 수 있다
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
