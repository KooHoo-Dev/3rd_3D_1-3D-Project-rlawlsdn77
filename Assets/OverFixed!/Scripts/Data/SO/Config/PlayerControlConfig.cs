using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/PlayerControlConfig")]
    public class PlayerControlConfig : ScriptableObject
    {
        [Header("선내 이동")]
        public float MoveSpeed = 3f;
        public float RunSpeed = 6f;                 // Shift
        public float Gravity = 9.81f;
        public float AxisSpeed = 3f;                // 이동 입력이 -1 ~ 1 사이를 따라가는 속도 (초당). 클수록 빨리 출발하고 멈춤

        [Header("선외 이동")]
        public float SpaceSpeed = 5f;               // 최고속도
        public float JetSpeed = 10f;                // 제트 추진(Shift) 최고속도
        public float SpaceAccel = 20f;              // 입력 방향 가속도 (초당)
        public float SpaceDrag = 1.8f;              // 속도가 exp(-SpaceDrag * 시간)으로 줄어든다
        public float BoundaryPush = 15f;            // 활동 구 밖에서 안쪽으로 미는 가속도 (초당)

        [Header("운반")]
        public int CarryCount = 1;

        [Header("카메라")]
        public float MouseSensitivity = 2f;         // 조작 감도
        public float MouseAxisScale = 0.1f;         // 마우스 이동량(픽셀)을 회전 각도로 줄이는 기본 배율
        public float InteriorMaxLookAngle = 60f;    // 위아래 시야 한계 (도)
        public float ExteriorMaxLookAngle = 80f;
        public float MaxLookDownAngle = 40f;        // 아래로 숙이는 한계 (도). 더 숙이면 1인칭에서 로봇 몸 안이 보인다

        [Header("애니메이션, IK (PlayerAnimator)")]
        public float AnimationDampTime = 0.1f;      // 이동 속도 값이 애니메이터에 따라가는 시간 (초)
        public float WalkStrideSpeed = 1.03f;       // Walk 클립에서 디딘 발이 몸 뒤로 가는 속도 (m/s, 로봇 크기 1.228 기준)
        public float RunStrideSpeed = 3.21f;        // Run 클립의 같은 값. 이보다 빠르면 Run을 그만큼 빨리 재생한다
        public float LookAtWeight = 1f;             // 조준점을 보는 IK 세기 (0 ~ 1)
        public float LookAtBodyWeight = 0.3f;
        public float LookAtHeadWeight = 1f;
        public float HandIKBlendSpeed = 4f;         // 들고 놓을 때 손 IK 세기가 0 ↔ 1을 오가는 속도 (초당)
        public float HandHoldGap = 0.05f;           // 손목을 재료 옆면에서 띄우는 거리 (m, 손바닥 두께)
        public Vector3 HandGripFingerDirection = new Vector3(0f, 0f, 1f);   // 재료를 쥔 왼손 검지 방향 (시선 기준, 오른손은 좌우 반전)
        public Vector3 HandGripThumbDirection = new Vector3(0f, 1f, 0f);    // 재료를 쥔 왼손 엄지 쪽 방향
    }
}
