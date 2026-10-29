using System.Collections.Generic;
using UnityEngine;

namespace OverFixed
{
    // 패널 사고. 선내 패널(OxygenRack, 태그 Panel)에 붙는다
    // 1. 항해 중에만 VoyageTable의 EventIntervalMin ~ EventIntervalMax 사이 무작위 간격으로 사고가 하나씩 난다 (한 번에 하나)
    // 2. 사고 종류는 EventTable의 SpawnWeight로 뽑고, 종류마다 미니게임(MiniGame)이 다르다
    // 3. 사고 중 E로 미니게임 화면을 연다. 성공 횟수는 화면을 닫았다 다시 열어도 남고, SuccessCount만큼 모이면 해결
    // 4. 제한시간(TimeLimit)이 지나면 코어 안정도를 StabilityPenalty만큼 한 번 깎고 사고가 끝난다. 해결 보상은 없다
    // 모듈 내구도가 없어서 기획서 5.2, 5.9와 달리 중앙 모듈에서 나고 안정도를 직접 깎는다
    public class Panel : MonoBehaviour
    {
        [SerializeField] private Core core;                 // 시간 초과 시 안정도를 깎는다
        [SerializeField] private MiniGameUI miniGameUI;     // 사고 중 E로 여는 미니게임 화면

        // 지금 사고의 EventTable 행. 사고가 없으면 null
        public EventRow CurrentEvent { get; private set; }
        public bool IsAccident => CurrentEvent != null;
        public float RemainingTime { get; private set; }    // 제한시간까지 남은 시간 (초)
        public int SuccessCount { get; private set; }       // 이번 사고에서 모은 성공 횟수

        private float nextAccidentTime;     // 다음 사고까지 남은 시간 (초)

        private void Start()
        {
            ScheduleNextAccident();
        }

        // 항해 중에만 시간이 흐른다 (정산, 게임오버 화면에서는 멈추고, 일시정지 중에는 timeScale이 0)
        private void Update()
        {
            if (GameManager.Instance.IsPlaying == false) return;

            if (IsAccident)
            {
                RemainingTime -= Time.deltaTime;
                if (RemainingTime <= 0f) FailAccident();
                return;
            }

            nextAccidentTime -= Time.deltaTime;
            if (nextAccidentTime <= 0f) StartAccident();
        }

        // LocalPlayer가 패널 앞에서 E를 누르면 부른다. 사고가 없으면 반응하지 않는다
        public void FixPanel()
        {
            if (IsAccident == false) return;
            miniGameUI.Open(this);
        }

        // 미니게임 성공 1회. MiniGameUI가 부른다
        public void AddSuccess()
        {
            if (IsAccident == false) return;

            SuccessCount++;
            if (SuccessCount >= CurrentEvent.SuccessCount) EndAccident();
        }

        private void StartAccident()
        {
            CurrentEvent = PickEvent();
            RemainingTime = CurrentEvent.TimeLimit;
            SuccessCount = 0;
        }

        // 시간 초과. 안정도를 한 번 깎고 사고를 끝낸다
        private void FailAccident()
        {
            core.MinusStability(CurrentEvent.StabilityPenalty);
            EndAccident();
        }

        private void EndAccident()
        {
            CurrentEvent = null;
            miniGameUI.Close();
            ScheduleNextAccident();
        }

        private void ScheduleNextAccident()
        {
            VoyageRow voyage = GameManager.Instance.Voyage;
            nextAccidentTime = Random.Range(voyage.EventIntervalMin, voyage.EventIntervalMax);
        }

        // SpawnWeight 비중으로 사고 종류 하나를 뽑는다
        private static EventRow PickEvent()
        {
            IReadOnlyList<EventRow> eventRows = DataManager.GetEventRows();
            float totalWeight = 0f;
            foreach (EventRow eventRow in eventRows) totalWeight += eventRow.SpawnWeight;

            float pick = Random.Range(0f, totalWeight);
            foreach (EventRow eventRow in eventRows)
            {
                pick -= eventRow.SpawnWeight;
                if (pick < 0f) return eventRow;
            }
            return eventRows[eventRows.Count - 1];
        }
    }
}
