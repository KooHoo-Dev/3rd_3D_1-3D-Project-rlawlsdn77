using System;
using UnityEngine;

namespace OverFixed
{
    // 모듈 : 함선에서 특별한 기능을 수행하는 정육면체 형태의 구역
    // 1. 모듈은 5 * 5 크기의 배열로 이루어져 있다.
    // 2. 중앙 모듈은 정 중앙(12번 인덱스)에 위치해 있고,
    // 중앙 모듈을 제외한 추가 모듈을 배치하여 모듈끼리 연결 가능하다.
    // 3. 게임 시작 시 "보조 에어락"(7번 인덱스)과 "납품소"(13번 인덱스)가 기본적으로 지급된다 (ShipConfig.StartModules) // 변경!!! 기획서 시작 배치에 맞춤
    // 4. 중앙 모듈을 제외한 각 모듈은 내구도를 가지고 있으며
    // 함선 사고, 운석 충돌과 같은 이벤트로 내구도가 감소한다.
    public class ModuleManager : MonoBehaviour
    {
        
    }
}
