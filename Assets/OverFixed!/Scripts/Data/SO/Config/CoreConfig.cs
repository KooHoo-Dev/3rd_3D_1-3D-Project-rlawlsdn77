using UnityEngine;

namespace OverFixed
{
    // 코어 안정도 (%). 시간 감소량은 항해 횟수마다 달라서 VoyageTable의 BaseDecay
    [CreateAssetMenu(menuName = "OverFixed/CoreConfig")]
    public class CoreConfig : ScriptableObject
    {
        [Header("코어 안정도")]
        public float StartStability = 100f;         // 새 게임 시작 값
        public float MaxStability = 100f;           // 상한. 회복해도 넘지 않는다
        public float WarningThreshold = 30f;        // 이 값 이하면 경고 색

        [Header("주문")]
        public float OrderSuccessRecovery = 2f;
        public float OrderFailPenalty = 5f;         // 제한시간 초과

        [Header("안정화 물질")]
        public int StabilizerPrice = 60;            // 크레딧
        public float StabilizerRecovery = 10f;
    }
}
