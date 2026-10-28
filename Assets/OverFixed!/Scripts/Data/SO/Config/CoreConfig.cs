using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/CoreConfig")]
    public class CoreConfig : ScriptableObject
    {
        [Header("코어 안정도 (%)")]
        public float StartStability = 100f;         // 새 게임을 시작할 때 안정도 (시간 감소량은 VoyageTable의 BaseDecay)
        public float MaxStability = 100f;           // 안정도 상한. 회복해도 이 값을 넘지 않는다
        public float WarningThreshold = 30f;        // 이 값 이하가 되면 경고 색으로 표시

        [Header("주문, 모듈")]
        public float OrderSuccessRecovery = 2f;     // 주문 성공 시 회복
        public float OrderFailPenalty = 5f;         // 주문 실패(제한시간 초과) 시 감소
        public float ModuleDestroyPenalty = 10f;    // 모듈 파괴 시 감소

        [Header("안정화")]
        public int StabilizerPrice = 60;            // 안정화 물질 가격 (크레딧)
        public float StabilizerRecovery = 10f;      // 안정화 물질 회복
        public float StabCellRecovery = 12f;        // 안정화 셀을 코어에 넣었을 때 회복
    }
}
