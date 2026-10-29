using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/PlayerControlConfig")]
    public class PlayerControlConfig : ScriptableObject
    {
        [Header("선내 이동")]
        public float MoveSpeed = 3f;                // 선내 : 일반 이동속도
        public float RunSpeed = 6f;                 // 선내 : 달리기 이동속도
        public float Gravity = 9.81f;               // 선내 : 바닥에 붙어 있게 하는 중력 가속도
        public float AxisSpeed = 3f;                // 이동 입력이 -1 ~ 1 사이를 따라가는 속도 (초당). 클수록 빨리 출발하고 멈춤

        [Header("선외 이동")]
        public float SpaceSpeed = 5f;               // 선외 : 일반 유영 최고속도
        public float JetSpeed = 10f;                // 선외 : 제트 추진 최고속도 (해금 후 Shift)
        public float SpaceAccel = 20f;              // 선외 : 입력 방향으로 붙는 가속도 (초당)
        public float SpaceDrag = 1.8f;              // 선외 : 감속 계수. 입력을 떼면 속도가 exp(-SpaceDrag * 시간)으로 줄어든다
        public float BoundaryPush = 15f;            // 선외 : 활동 구 경계를 넘었을 때 안쪽으로 밀어내는 가속도 (초당)

        [Header("운반, 상호작용")]
        public int CarryCount = 1;
        public float TractorRange = 15f;
        public float TractorSpeed = 12f;
        public float ExteriorPickRange = 4.5f;

        [Header("카메라")]
        public float MouseSensitivity = 2f;         // 조작 감도. 옵션 화면이 생기면 OptionSave.Sensitivity를 여기에 곱한다
        public float MouseAxisScale = 0.1f;         // 마우스 이동량(픽셀)을 회전 각도로 줄이는 기본 배율 (개발자 조절값)
        public float InteriorMaxLookAngle = 60f;    // 선내 : 마우스 상하 시야 한계 (도)
        public float ExteriorMaxLookAngle = 80f;    // 선외 : 마우스 상하 시야 한계 (도)
        public float MaxLookDownAngle = 40f;        // 선내·선외 : 아래로 숙이는 한계 (도). 1인칭에서 더 숙이면 로봇 목과 몸통 안이 보인다

        [Header("애니메이션, IK (PlayerAnimator)")]
        public float AnimationDampTime = 0.1f;      // 이동 속도 값이 애니메이터에 따라가는 시간 (초). 클수록 걷기·달리기가 부드럽게 바뀐다
        public float WalkStrideSpeed = 1.03f;       // Walk 클립에서 디딘 발(아래쪽 발)이 몸 뒤로 가는 속도 (m/s, 로봇 크기 1.228 기준으로 잰 값)
        public float RunStrideSpeed = 3.21f;        // Run 클립에서 디딘 발이 몸 뒤로 가는 속도 (m/s). 이보다 빠르면 Run을 그만큼 빨리 재생한다
        public float LookAtWeight = 1f;             // 조준점을 바라보는 IK 전체 세기 (0 ~ 1)
        public float LookAtBodyWeight = 0.3f;       // 상체가 함께 도는 정도 (0 ~ 1)
        public float LookAtHeadWeight = 1f;         // 머리가 도는 정도 (0 ~ 1)
        public float HandIKBlendSpeed = 4f;         // 재료를 들고 놓을 때 손 IK 세기가 0 ↔ 1을 오가는 속도 (초당)
    }
}
