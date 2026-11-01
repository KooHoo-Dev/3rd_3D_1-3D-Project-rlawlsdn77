using System.Collections.Generic;
using UnityEngine;

namespace OverFixed
{
    // 패널 사고. 항해 중 VoyageTable의 EventInterval 간격으로 사고가 하나씩 난다 (종류는 EventTable의 SpawnWeight로 뽑는다)
    // E로 연 미니게임에서 SuccessCount만큼 성공하면 해결, 제한시간이 지나면 안정도를 StabilityPenalty만큼 깎고 끝난다
    public class Panel : MonoBehaviour
    {
        [SerializeField] private MiniGameUI miniGameUI;

        public EventRow CurrentEvent { get; private set; }  // 사고가 없으면 null
        public bool IsAccident => CurrentEvent != null;
        public float RemainingTime { get; private set; }
        public int SuccessCount { get; private set; }       // 화면을 닫았다 다시 열어도 남는다

        private float nextAccidentTime;

        private void Start()
        {
            ScheduleNextAccident();
        }

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

        public void OpenMiniGame()
        {
            miniGameUI.Open(this);
        }

        // 미니게임 성공 1회 (MiniGameUI)
        public void AddSuccess()
        {
            if (IsAccident == false) return;

            SuccessCount++;
            if (SuccessCount >= CurrentEvent.SuccessCount) EndAccident();
        }

        private void StartAccident()
        {
            IReadOnlyList<EventRow> eventRows = DataManager.GetEventRows();
            float[] weights = new float[eventRows.Count];
            for (int i = 0; i < eventRows.Count; i++) weights[i] = eventRows[i].SpawnWeight;
            CurrentEvent = eventRows[WeightedRandom.PickIndex(weights)];
            RemainingTime = CurrentEvent.TimeLimit;
            SuccessCount = 0;
        }

        private void FailAccident()
        {
            GameManager.Instance.ChangeStability(-CurrentEvent.StabilityPenalty);
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
    }
}
