using System.Collections.Generic;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OverFixed
{
    // 순서 입력: 화면에 뜬 방향키를 순서대로 누른다 (방향키와 WASD). 한 줄을 다 맞추면 성공 1회, 틀리면 그 줄을 처음부터
    // 칸은 cellTemplate을 복제해 만들고, 칸 안의 화살표(위를 가리키는 그림)를 돌려 방향을 보인다
    public class SequenceGame : MiniGame
    {
        private enum Direction
        {
            Up,
            Left,
            Down,
            Right,
        }

        private const int DirectionCount = 4;
        private const float QuarterTurn = 90f;     // Direction 순서대로 화살표를 반시계 방향으로 90도씩 돌린다

        [SerializeField] private MiniGameConfig miniGameConfig;

        [Header("화면")]
        [SerializeField] private RectTransform row;         // 칸이 들어가는 줄 (가로 정렬)
        [SerializeField] private Image cellTemplate;        // 꺼 둔 칸 원본. 자식이 화살표
        [SerializeField] private Color pendingColor = Color.gray;   // 아직 누를 차례가 아닌 칸
        [SerializeField] private Color currentColor = Color.white;  // 지금 누를 칸
        [SerializeField] private Color doneColor = Color.cyan;      // 맞춘 칸

        public override AccidentType AccidentID => AccidentType.Sequence;

        private readonly List<Direction> sequence = new List<Direction>();
        private readonly List<Image> cells = new List<Image>();
        private int inputIndex;     // 지금 누를 칸의 순서

        public override void Begin(AccidentRow accidentRow, int successCount)
        {
            base.Begin(accidentRow, successCount);
            MakeRow();
        }

        public override void End()
        {
            ClearCells();
            base.End();
        }

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.UpArrow) || SimpleInput.GetKeyDown(Key.W)) Press(Direction.Up);
            else if (SimpleInput.GetKeyDown(Key.LeftArrow) || SimpleInput.GetKeyDown(Key.A)) Press(Direction.Left);
            else if (SimpleInput.GetKeyDown(Key.DownArrow) || SimpleInput.GetKeyDown(Key.S)) Press(Direction.Down);
            else if (SimpleInput.GetKeyDown(Key.RightArrow) || SimpleInput.GetKeyDown(Key.D)) Press(Direction.Right);
        }

        private void Press(Direction direction)
        {
            if (direction != sequence[inputIndex])
            {
                inputIndex = 0;
                Miss("틀렸습니다 · 처음부터");
                DrawCells();
                return;
            }

            inputIndex++;
            if (inputIndex < sequence.Count)
            {
                DrawCells();
                return;
            }

            // 새 줄을 만든 뒤 알린다 (마지막 성공이면 알림을 받은 화면이 닫히며 칸을 지운다)
            MakeRow();
            Succeed("성공");
        }

        // SequenceLength개의 방향을 뽑되 같은 방향은 SequenceMaxSameKey개까지만
        private void MakeRow()
        {
            sequence.Clear();
            var directionCounts = new int[DirectionCount];
            var candidates = new List<Direction>();
            for (int cellIndex = 0; cellIndex < miniGameConfig.SequenceLength; cellIndex++)
            {
                candidates.Clear();
                for (int directionIndex = 0; directionIndex < DirectionCount; directionIndex++)
                {
                    if (directionCounts[directionIndex] < miniGameConfig.SequenceMaxSameKey) candidates.Add((Direction)directionIndex);
                }
                Direction direction = candidates[Random.Range(0, candidates.Count)];
                directionCounts[(int)direction]++;
                sequence.Add(direction);
            }

            ClearCells();
            foreach (Direction direction in sequence)
            {
                Image cell = Instantiate(cellTemplate, row);
                cell.gameObject.SetActive(true);
                cell.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, (int)direction * QuarterTurn);
                cells.Add(cell);
            }
            inputIndex = 0;
            DrawCells();
        }

        private void DrawCells()
        {
            for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
            {
                if (cellIndex < inputIndex) cells[cellIndex].color = doneColor;
                else if (cellIndex == inputIndex) cells[cellIndex].color = currentColor;
                else cells[cellIndex].color = pendingColor;
            }
        }

        private void ClearCells()
        {
            foreach (Image cell in cells) Destroy(cell.gameObject);
            cells.Clear();
        }
    }
}
