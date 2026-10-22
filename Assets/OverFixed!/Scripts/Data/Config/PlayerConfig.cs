using UnityEngine;

namespace OverFixed
{
    // 플레이어 (기획서 6.2) + PlayerController에 있던 조작 수치
    public class PlayerConfig : ScriptableObject
    {
        [Header("이동")]
        public float MoveSpeed = 3f;                // 선내 : 일반 이동속도
        public float RunSpeed = 6f;                 // 선내 : 달리기 이동속도 
        public float SpaceSpeed = 5f;               // 선외 : 일반 유영속도
        public float JetSpeed = 10f;                // 선외 : 제트팩 유영속도
        public float AxisSpeed = 3f;                // 이동 입력이 -1 ~ 1 사이를 따라가는 속도 (초당). 클수록 빨리 출발하고 멈춤


        [Header("운반, 상호작용")]
        public int CarryCount = 1;
        public float TractorRange = 15f;
        public float TractorSpeed = 12f;
        public float ExteriorPickRange = 4.5f;
        
        [Header("카메라")]
        public float MouseSensitivity = 2f;         // 조작 감도. 옵션 화면이 생기면 OptionSave.Sensitivity를 여기에 곱한다
        public float MouseAxisScale = 0.1f;         // 마우스 이동량(픽셀)을 회전 각도로 줄이는 기본 배율 (개발자 조절값)
        public float MaxLookAngle = 80f;

        [Header("플레이 조정")] 
        public float CameraTweenSpeed = 3f;
    }
}
