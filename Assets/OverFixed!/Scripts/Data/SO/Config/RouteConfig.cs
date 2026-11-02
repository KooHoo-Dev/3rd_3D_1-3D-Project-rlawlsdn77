using UnityEngine;

namespace OverFixed
{
    // 출항 준비(후보 행성, 경로 지대)와 경로 구간 조절값. 행성 종류별·지대별 값은 PlanetTypeTable, ZoneTable
    [CreateAssetMenu(menuName = "OverFixed/RouteConfig")]
    public class RouteConfig : ScriptableObject
    {
        [Header("후보 행성")]
        public int CandidateCount = 3;          // 후보 행성 수 (종류는 겹치지 않음)
        public int BaseRefreshCount = 1;        // 출항 준비마다 쓸 수 있는 경로 새로고침 횟수

        [Header("경로")]
        public int ZoneCount = 3;               // 한 경로의 지대 수 (겹치지 않음)
        public float ZoneSegmentRate = 0.2f;    // 지대 구간 길이 (항해 시간에 대한 비율)
        public float NormalSegmentRate = 0.1f;  // 일반 구간 길이. 경로 = 일반, 지대 1, 일반, 지대 2, 일반, 지대 3, 일반
        public float ZoneWarningTime = 10f;     // 지대에 들어가기 전 예고 (초)
    }
}
