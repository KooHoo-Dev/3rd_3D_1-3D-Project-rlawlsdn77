using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/SpawnConfig")]
    public class SpawnConfig : ScriptableObject
    {
        [Header("재료 생성과 흐름")]
        public float BaseSpawnRate = 1.5f;          // 초당 생성 개수
        public float FlowSpeed = 2.5f;              // 흐르는 속도 (m/s)

        [Header("풀링")]
        public int PoolCount = 40;                  // 종류마다 미리 만들어 두는 개수 (동시에 흐르는 개수보다 넉넉하게)
        public float FlowItemScale = 5f;            // 흐르는 재료의 크기 배율 (선외에서 잘 보이게)
    }
}
