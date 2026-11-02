using UnityEngine;

namespace OverFixed
{
    // 미니게임 종류별 조절값. 종류마다 같은 열(제한시간, 성공 횟수, 감소량)은 AccidentTable
    [CreateAssetMenu(menuName = "OverFixed/MiniGameConfig")]
    public class MiniGameConfig : ScriptableObject
    {
        [Header("타이밍 바")]
        public float TimingCursorSpeed = 1f;    // 토글 속도 (막대 길이 / 초)
        public float TimingZoneWidth = 0.2f;    // 가운데 성공 구간 폭 (막대 길이에 대한 비율)
        public float TimingMissLockTime = 0.5f; // 빗나간 뒤 Space를 받지 않는 시간 (초)

        [Header("순서 입력")]
        public int SequenceLength = 6;          // 한 줄의 방향키 수
        public int SequenceMaxSameKey = 3;      // 한 줄에 같은 방향키 최대 수
    }
}
