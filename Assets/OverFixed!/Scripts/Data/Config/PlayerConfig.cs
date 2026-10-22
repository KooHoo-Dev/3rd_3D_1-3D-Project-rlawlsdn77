using UnityEngine;

namespace OverFixed
{
    // 플레이어 (기획서 6.2) + PlayerController에 있던 조작 수치
    [CreateAssetMenu(menuName = "OverFixed/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
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

        [Header("숄더뷰 (데드 스페이스식)")]
        public float PivotHeight = 0.6f;            // 플레이어 중심에서 어깨(카메라 피벗)까지의 높이
        public float ShoulderOffset = 0.55f;        // 어깨 피벗을 오른쪽으로 미는 거리. 캐릭터가 화면 왼쪽에 서게 된다
        public float CameraDistance = 1.5f;         // 피벗에서 카메라까지의 거리
        public float CameraFieldOfView = 65f;
        public float CameraRadius = 0.2f;           // 벽 검사용 구의 반지름. 카메라가 벽에 파묻히지 않게 하는 여유
        public LayerMask CameraCollisionMask = ~0;  // 카메라를 당기는 대상 레이어 (플레이어 자신과 트리거는 항상 제외)
        public float HidePlayerDistance = 0.5f;     // 카메라가 머리에 이만큼 붙으면 캐릭터를 숨긴다

        [Header("플레이 조정")]
        public float CameraTweenSpeed = 3f;         // 벽에서 풀린 카메라가 제자리로 돌아가는 속도 (초당 비율)
    }
}
