using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/SpawnConfig")]
    public class SpawnConfig : ScriptableObject
    {
        [Header("재료 생성과 흐름")]
        public float BaseSpawnRate = 1.5f;          // 초당 재료 생성 개수 (지대 효과의 배율을 여기에 곱한다)
        public float FlowSpeed = 2.5f;              // 재료가 흐르는 속도 (m/s)
        public float NearBias = 0f;                 // 고철, 연료 셀이 선체 가까이 몰리는 정도

        [Header("구조 캡슐 (조난선 지대)")]
        public float CapsuleSpawnRate = 0.15f;      // 조난선 지대에서 초당 캡슐 생성 개수
        public int CapsuleReward = 60;              // 에어락이나 하역구에 넣으면 받는 크레딧
    }
}
