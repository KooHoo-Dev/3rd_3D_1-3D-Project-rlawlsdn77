using Study.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OverFixed
{
    // 타이밍 바: 좌우로 움직이는 토글이 가운데 구간에 들어왔을 때 Space를 누르면 성공 1회 (실패해도 성공 횟수는 남는다)
    // 성공하면 토글이 왼쪽 끝에서 다시 출발하고, 빗나가면 TimingMissLockTime 동안 Space를 받지 않는다 (연타 방지)
    // 토글 속도와 구간 폭은 MiniGameConfig의 TimingCursorSpeed, TimingZoneWidth
    public class TimingBarGame : MiniGame
    {
        [SerializeField] private MiniGameConfig miniGameConfig;

        [Header("화면")]
        [SerializeField] private RectTransform zone;        // 가운데 성공 구간 (막대의 자식)
        [SerializeField] private RectTransform cursor;      // 움직이는 토글 (막대의 자식)
        [SerializeField] private TMP_Text resultValue;      // "성공" / "빗나감"
        [SerializeField] private Color successColor = Color.cyan;
        [SerializeField] private Color missColor = Color.red;

        public override EventType EventID => EventType.TimingBar;

        private float movedTime;        // 토글이 왼쪽 끝에서 출발한 뒤 지난 시간 (초)
        private float missLockTime;     // Space를 다시 받기까지 남은 시간 (초)

        // 토글 위치 (0 = 막대 왼쪽 끝, 1 = 오른쪽 끝). 끝에 닿으면 되돌아온다
        private float CursorValue => Mathf.PingPong(movedTime * miniGameConfig.TimingCursorSpeed, 1f);

        // 성공 구간은 막대 가운데에 TimingZoneWidth 폭
        private float ZoneMin => 0.5f - miniGameConfig.TimingZoneWidth * 0.5f;
        private float ZoneMax => 0.5f + miniGameConfig.TimingZoneWidth * 0.5f;

        public override void Begin(EventRow eventRow, int successCount)
        {
            base.Begin(eventRow, successCount);
            zone.anchorMin = new Vector2(ZoneMin, 0f);
            zone.anchorMax = new Vector2(ZoneMax, 1f);
            movedTime = 0f;
            missLockTime = 0f;
            resultValue.text = "";
            DrawCursor();
        }

        private void Update()
        {
            movedTime += Time.deltaTime;
            missLockTime -= Time.deltaTime;
            DrawCursor();

            if (SimpleInput.GetKeyDown(Key.Space)) Press();
        }

        private void Press()
        {
            if (missLockTime > 0f) return;

            float cursorValue = CursorValue;
            if (cursorValue >= ZoneMin && cursorValue <= ZoneMax)
            {
                movedTime = 0f;
                ShowResult("성공", successColor);
                Succeed();
                return;
            }

            missLockTime = miniGameConfig.TimingMissLockTime;
            ShowResult("빗나감", missColor);
        }

        private void DrawCursor()
        {
            float cursorValue = CursorValue;
            cursor.anchorMin = new Vector2(cursorValue, cursor.anchorMin.y);
            cursor.anchorMax = new Vector2(cursorValue, cursor.anchorMax.y);
        }

        private void ShowResult(string text, Color color)
        {
            resultValue.text = text;
            resultValue.color = color;
        }
    }
}
