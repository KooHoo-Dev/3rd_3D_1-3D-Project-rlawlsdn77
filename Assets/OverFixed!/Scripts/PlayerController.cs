using System;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace OverFixed
{
    // 3인칭 숄더뷰 플레이어 컨트롤러
    // 공통 입력 : 방향키(전후좌우), E(상호작용)
    // 선내 입력 : Shift(달리기)
    // 선외 입력 : Space(상승), Q(하강)
    
    // 해금 능력 
    // 선내 입력 : Q(레이더로 사건 추적)
    // 선외 입력 : Shift(제트 추진)
    
    // 상호작용 : 물건 집기 + 수리하기 + 터미널 조작
    
    public class PlayerController : MonoBehaviour
    {
        [Header("이동")] 
        [SerializeField] private float walkSpeed = 4f;

        [Header("선내 : 달리기 속도")] 
        [SerializeField] private float runSpeed = 6f;
        
        [Header("선외 : 제트팩 상승 / 하강")]
        [SerializeField] private float riseSpeed = 5f;
        [SerializeField] private float descentSpeed = 5f;

        [Header("선내(해금) : 레이더 추적")] 
        [SerializeField] private float RaderRadius = 10f;
        
        [Header("선외(해금) : 제트 추진")] 
        [SerializeField] private float jetSpeed = 5f;
        
        [Header("카메라")] 
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float mouseSensitivity = 2f; // 마우스 감도 입니다.
        [SerializeField] private float maxLookAngle = 80f;

        [Header("애니메이터")] 
        [SerializeField] private Animator animator;
        
        // 아래는 내부변수들
        private LocalPlayer localPlayer;
        private CharacterController controller;
        private Vector3 velocity;
        private float verticalLookAngle;
        private Quaternion initialCameraRotation;
        private float moveX, moveZ; // 키를 따라 -1 ~ 1 사이를 부드럽게 움직이는 로직에 사용되는 축 값
        #region Unity Methods

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            UpdateMovement();
            UpdateCameraRotation();
            UpdateInteraction();
        }

        #endregion

        private void UpdateInteraction()
        {
            // 상호 작용 (E)
            if (SimpleInput.GetKeyDown(Key.E))
            {
                switch (SceneManager.GetActiveScene().name)
                {
                    case "Space_In" : // 선내
                        break;
                        
                    case "Space_Out" : // 선외
                        break;
                }
            }
        }
        
        private void UpdateMovement()
        {
            // 첫번째로는 이동 속도를 결정해줍니다. LeftShift가 true이면 runSpeed 입니다.
            float speed = SimpleInput.GetKey(Key.LeftShift) ? runSpeed : walkSpeed;

            // 이동 연산에 사용할 방향의 축값을 가져옵니다. 가져온 데이터는 한축은
            // -1, 0, 1 의 3가지중 하나가 반환됩니다
            Vector2 inputRaw = SimpleInput.GetMoveAxisRaw();
            moveX = SmoothAxis(moveX, inputRaw.x);
            moveZ = SmoothAxis(moveZ, inputRaw.y);

            // 현재 플레이어의 방향에 맞춰서 moveVector를 계산합니다.
            Vector3 moveVector = (transform.right * moveX) + (transform.forward * moveZ);
            // moveVector의 길이만큼 현재 프레임의 이동량이 조정이 됩니다.
            controller.Move(moveVector * (speed * Time.deltaTime));
            
            controller.Move(velocity * Time.deltaTime);
            
            // 선외 입력
            // if (SceneManager.GetActiveScene().name == "Space_Out")
            if (SceneManager.GetActiveScene().name == "Space_In") // 테스트용 코드 (지울 예정)
            {
                if (SimpleInput.GetKey(Key.Space)) // 상승
                {
                    controller.Move(Vector3.up * riseSpeed * Time.deltaTime);
                }

                if (SimpleInput.GetKey(Key.Q)) // 하강
                {
                    controller.Move(Vector3.down * descentSpeed * Time.deltaTime);                   
                }
            }
        }
        
        private static float SmoothAxis(float current, float target)
        {
            // 아래값은 보간 속도(?)를 의미한다고 생각하시면 됩니다. 댐핑값
            const float AXIS_SPEED = 3f;
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
            return Mathf.MoveTowards(current, target, AXIS_SPEED * Time.deltaTime);
        }
        
        private void UpdateCameraRotation()
        {
            const float MOUSE_AXIS_SCALE = 0.1f;
            // 현재 프레임의 마우스 이동량을 가져옵니다.
            // MOUSE_AXIS_SCALE을 곱해서 픽셀로 돌려받은 값을 줄여줍니다.
            Vector2 delta = SimpleInput.GetMouseDelta() * MOUSE_AXIS_SCALE;
            float mouseX = delta.x * mouseSensitivity;
            float mouseY = delta.y * mouseSensitivity;
            
            Quaternion yawRotation = Quaternion.Euler(0, mouseX, 0);
            // 이것도 되겠죠?
            //Quaternion yawRotation = Quaternion.AngleAxis(mouseX, Vector3.up);
            transform.rotation *= yawRotation; // y축은 내가 돌아갑니다. 나는 월드 입니다 

            // Pitch값은 누적. verticalLookRotation => verticalLookAngle로 수정
            verticalLookAngle -= mouseY;
            verticalLookAngle = Mathf.Clamp(verticalLookAngle, -maxLookAngle, maxLookAngle);
            cameraTransform.localRotation = Quaternion.Euler(verticalLookAngle, 0f , 0f);
            
            // 아래와 동일합니다.
            // Quaternion pitchRotation = Quaternion.AngleAxis(verticalLookAngle, Vector3.right);
            // cameraTransform.localRotation = initialCameraRotation * pitchRotation;
        }
    }
}
