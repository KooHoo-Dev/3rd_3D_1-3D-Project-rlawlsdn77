using System;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OverFixed
{
    // 3인칭 숄더뷰 플레이어 컨트롤러
    // 공통 입력 : 방향키(전후좌우), E(상호작용), Q(물건 내려놓기)
    // 선내 입력 : Shift(달리기). 전후좌우 평면 이동만 한다
    // 선외 입력 : Space(상승), Ctrl(하강). 제트팩 이동이라 관성이 있다 (입력을 떼도 조금 더 감)

    // 해금 능력
    // 선내 입력 : 레이더로 사건 추적
    // 선외 입력 : Shift(제트 추진)

    // 상호작용 : 물건 집기 + 수리하기 + 터미널 조작

    public enum MoveState
    {
        Interior,   // 선내
        Exterior,   // 선외
    }

    public class PlayerController : MonoBehaviour
    {

        [Header("설정값 (playerConfig로 밸런스 조정)")]
        [SerializeField] private PlayerConfig playerConfig;

        [Header("애니메이터")]
        [SerializeField] private Animator animator;

        [Header("시작 이동 상태")]
        [SerializeField] private MoveState startMoveState = MoveState.Interior;

        public MoveState MoveState { get; private set; }

        // 제트 추진 해금 여부. 정비 상점이 생기면 그쪽에서 켠다
        public bool IsJetUnlocked { get; set; }

        // 마우스 상하 시야 각도. ShoulderCamera가 읽어서 카메라 피벗에 적용한다
        public float VerticalLookAngle => verticalLookAngle;

        // 아래는 내부변수들
        private LocalPlayer localPlayer;
        private InteractionCube interactionCube; // 상호작용 큐브
        private CharacterController controller;
        private float fallSpeed;        // 선내 : 중력으로 쌓이는 낙하 속도
        private Vector3 spaceVelocity;  // 선외 : 관성으로 유지되는 제트팩 속도
        private float verticalLookAngle;
        private float moveX, moveZ; // 키를 따라 -1 ~ 1 사이를 부드럽게 움직이는 로직에 사용되는 축 값

        #region Unity Methods

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            localPlayer = GetComponent<LocalPlayer>();
            interactionCube = GetComponentInChildren<InteractionCube>();
            SetMoveState(startMoveState);
        }

        private void Update()
        {
            if (MoveState == MoveState.Interior) UpdateInteriorMovement();
            else UpdateExteriorMovement();
            UpdateCameraRotation();
            UpdateInteraction();
        }
        #endregion

        // 에어락을 통과할 때 호출. 이전 상태의 속도는 이어가지 않는다
        public void SetMoveState(MoveState moveState)
        {
            MoveState = moveState;
            fallSpeed = 0f;
            spaceVelocity = Vector3.zero;
            moveX = 0f;
            moveZ = 0f;
        }

        private void UpdateInteraction()
        {
            // 상호 작용 (E)
            if (SimpleInput.GetKeyDown(Key.E))  interactionCube.Interact();

            // 집은 아이템 떨구기 (Q)
            if (SimpleInput.GetKeyDown(Key.Q))  localPlayer.LeaveItem();
        }

        private void UpdateInteriorMovement()
        {
            // 이동속도 결정
            float speed = SimpleInput.GetKey(Key.LeftShift) ? playerConfig.RunSpeed : playerConfig.MoveSpeed;

            // 이동 연산에 사용할 방향의 축값을 가져옵니다. 가져온 데이터는 한축은
            // -1, 0, 1 의 3가지중 하나가 반환됩니다
            Vector2 inputRaw = SimpleInput.GetMoveAxisRaw();
            moveX = SmoothAxis(moveX, inputRaw.x, playerConfig.AxisSpeed);
            moveZ = SmoothAxis(moveZ, inputRaw.y, playerConfig.AxisSpeed);

            // 현재 플레이어의 방향에 맞춰서 moveVector를 계산합니다.
            Vector3 moveVector = (transform.right * moveX) + (transform.forward * moveZ);
            // moveVector의 길이만큼 현재 프레임의 이동량이 조정이 됩니다.
            controller.Move(moveVector * (speed * Time.deltaTime));

            // 선내는 상하 입력이 없고, 중력으로 바닥에 붙어 있는다
            if (controller.isGrounded && fallSpeed < 0f) fallSpeed = 0f;
            fallSpeed -= playerConfig.Gravity * Time.deltaTime;
            controller.Move(Vector3.up * (fallSpeed * Time.deltaTime));
        }

        private void UpdateExteriorMovement()
        {
            // 전후좌우 + 상승(Space) / 하강(LeftCtrl) 입력을 합친 방향
            Vector2 inputRaw = SimpleInput.GetMoveAxisRaw();
            float inputY = 0f;
            if (SimpleInput.GetKey(Key.Space)) inputY += 1f;
            if (SimpleInput.GetKey(Key.LeftCtrl)) inputY -= 1f;

            Vector3 thrustVector = (transform.right * inputRaw.x) + (transform.forward * inputRaw.y) + (Vector3.up * inputY);
            if (thrustVector.sqrMagnitude > 1f) thrustVector.Normalize();

            // 입력은 속도에 더해지고(가속), 속도는 매 프레임 조금씩 줄어든다(감속).
            // 그래서 입력을 떼도 바로 멈추지 않고 미끄러지다 멈춘다
            spaceVelocity += thrustVector * (playerConfig.SpaceAccel * Time.deltaTime);
            spaceVelocity *= Mathf.Exp(-playerConfig.SpaceDrag * Time.deltaTime);

            // 최고속도 : 제트 추진을 해금했고 Shift를 누르는 동안만 JetSpeed
            bool isJetting = IsJetUnlocked && SimpleInput.GetKey(Key.LeftShift);
            float maxSpeed = isJetting ? playerConfig.JetSpeed : playerConfig.SpaceSpeed;
            spaceVelocity = Vector3.ClampMagnitude(spaceVelocity, maxSpeed);

            controller.Move(spaceVelocity * Time.deltaTime);
        }

        private static float SmoothAxis(float current, float target, float axisSpeed)
        {
            // axisSpeed는 보간 속도(?)를 의미한다고 생각하시면 됩니다. 댐핑값
            // target이 0이면 정지를 의미하고
            // current * target 음수일 경우에는 반대 방향으로 이동하는 케이스
            // 둘다 current를 0으로 두고 계산합니다.
            // (왼쪽으로 이동하고 있는데 오른쪽으로 이동하라고 명령이 들어오면 0일 기준으로
            // 이동처리를 하라고 처리합니다.)
            if (target != 0f && current * target < 0f) current = 0f;
            // Toward : ~ 향해 이런뜻
            // MoveTowards(current, target, maxDelta)
            // : current에서 target 목표 지점으로 maxDelta를 초과하지 않게 움직인 양을 반환합니다.
            // ex) current = -0.2f, target : -1, maxDelta = 0.1f
            // -0.3이 반환 됩니다.
            return Mathf.MoveTowards(current, target, axisSpeed * Time.deltaTime);
        }

        private void UpdateCameraRotation()
        {
            // 현재 프레임의 마우스 이동량을 가져옵니다.
            // MouseAxisScale을 곱해서 픽셀로 돌려받은 값을 줄여줍니다.
            Vector2 delta = SimpleInput.GetMouseDelta() * playerConfig.MouseAxisScale;
            float mouseX = delta.x * playerConfig.MouseSensitivity;
            float mouseY = delta.y * playerConfig.MouseSensitivity;

            Quaternion yawRotation = Quaternion.Euler(0, mouseX, 0);
            // 이것도 되겠죠?
            //Quaternion yawRotation = Quaternion.AngleAxis(mouseX, Vector3.up);
            transform.rotation *= yawRotation; // y축은 내가 돌아갑니다. 나는 월드 입니다
            
            // 선외는 위아래로 날아다니므로 선내보다 넓게 본다
            float maxLookAngle = MoveState == MoveState.Interior ? playerConfig.InteriorMaxLookAngle : playerConfig.ExteriorMaxLookAngle;
            verticalLookAngle -= mouseY;
            verticalLookAngle = Mathf.Clamp(verticalLookAngle, -maxLookAngle, maxLookAngle);
        }
    }
}
