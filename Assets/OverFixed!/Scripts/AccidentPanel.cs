using System.Collections.Generic;
using UnityEngine;

namespace OverFixed
{
    // 사고가 나는 패널. 항해 중 VoyageTable의 AccidentInterval 간격으로 사고가 하나씩 난다 (종류는 AccidentTable의 SpawnWeight로 뽑는다)
    // E로 연 미니게임에서 SuccessCount만큼 성공하면 해결, 제한시간이 지나면 안정도를 StabilityPenalty만큼 깎고 끝난다
    public class AccidentPanel : MonoBehaviour
    {
        [SerializeField] private MiniGameUI miniGameUI;

        public AccidentRow CurrentAccident { get; private set; }  // 사고가 없으면 null
        public bool IsAccident => CurrentAccident != null;
        public float RemainingTime { get; private set; }
        public int SuccessCount { get; private set; }             // 화면을 닫았다 다시 열어도 남는다

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
            if (SuccessCount >= CurrentAccident.SuccessCount) EndAccident();
        }

        private void StartAccident()
        {
            IReadOnlyList<AccidentRow> accidentRows = DataManager.GetAccidentRows();
            float[] weights = new float[accidentRows.Count];
            for (int index = 0; index < accidentRows.Count; index++) weights[index] = accidentRows[index].SpawnWeight;
            CurrentAccident = accidentRows[WeightedRandom.PickIndex(weights)];
            RemainingTime = CurrentAccident.TimeLimit;
            SuccessCount = 0;
        }

        private void FailAccident()
        {
            GameManager.Instance.ChangeStability(-CurrentAccident.StabilityPenalty);
            EndAccident();
        }

        private void EndAccident()
        {
            CurrentAccident = null;
            miniGameUI.Close();
            ScheduleNextAccident();
        }

        private void ScheduleNextAccident()
        {
            VoyageRow voyage = GameManager.Instance.Voyage;
            nextAccidentTime = Random.Range(voyage.AccidentIntervalMin, voyage.AccidentIntervalMax);
        }
    }
}
