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

        [Header("풀링")]
        public int PoolCount = 40;                  // 재료 종류마다 미리 만들어 두는 개수. 동시에 흐르는 개수(초당 생성 개수 × 이동 시간 × 종류 비율)보다 넉넉하게
        public float FlowItemScale = 5f;            // 흐르는 재료의 크기 배율 (선외에서 잘 보이게). 손에 들면 원래 크기로 돌아간다

        [Header("구조 캡슐 (조난선 지대)")]
        public float CapsuleSpawnRate = 0.15f;      // 조난선 지대에서 초당 캡슐 생성 개수
        public int CapsuleReward = 60;              // 에어락이나 하역구에 넣으면 받는 크레딧
    }
}
