using UnityEngine;

namespace OverFixed
{
    // 플레이어 로봇(Robobot T7)의 애니메이션과 IK. 로봇 모델의 Animator와 같은 오브젝트에 붙는다 (OnAnimatorIK는 그 오브젝트에서만 불린다)
    // 1. 선내는 이동 속도로 Idle → Walk → Run을 섞고(Speed 0 ~ 2), 선외는 Float
    //    발이 미끄러지지 않도록 클립이 디딘 발로 밀어내는 속도(WalkStrideSpeed, RunStrideSpeed)에 실제 속도를 맞춰 섞고,
    //    RunStrideSpeed보다 빠르면 MoveAnimSpeed로 그만큼 빨리 재생한다
    // 2. 재료를 주우면 PickUp, 터미널·패널·에어락을 쓰면 Use를 상체에만 한 번 재생 (Action 레이어, 상체 마스크)
    // 3. IK: 머리와 상체가 화면 중앙 조준점(ShoulderCamera.AimPoint)을 보고, 재료를 들면 두 손을 재료 양옆에 붙인다
    // 이동과 회전은 PlayerController가 하고, 여기서는 그 결과(위치 변화)를 읽어 보이기만 한다
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveAnimSpeedHash = Animator.StringToHash("MoveAnimSpeed");
        private static readonly int IsExteriorHash = Animator.StringToHash("IsExterior");
        private static readonly int PickUpHash = Animator.StringToHash("PickUp");
        private static readonly int UseHash = Animator.StringToHash("Use");

        // Move 블렌드 트리의 기준 값 (0 = Idle, 1 = Walk, 2 = Run)과 클립 이름 (한 주기 길이를 읽는다)
        private const float WalkBlend = 1f;
        private const float RunBlend = 2f;
        private const string WalkClipName = "Walk";
        private const string RunClipName = "Run";

        [Header("설정값 (WalkStrideSpeed, RunStrideSpeed로 발 맞추기, 애니메이션·IK 세기)")]
        [SerializeField] private PlayerControlConfig playerControlConfig;

        [Header("플레이어")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private ShoulderCamera shoulderCamera;

        private Animator animator;
        private Vector3 lastPosition;
        private MoveState lastMoveState;
        private float handWeight;               // 손 IK 세기 (0 ~ 1). 재료를 들면 1로, 놓으면 0으로 천천히 바뀐다
        private ItemObject handItem;            // 손 IK 대상 재료와 그 렌더러 (재료가 바뀔 때만 다시 찾는다)
        private Renderer handItemRenderer;
        private float walkCycleTime;            // Walk, Run 클립의 한 주기 길이 (초)
        private float runCycleTime;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == WalkClipName) walkCycleTime = clip.length;
                if (clip.name == RunClipName) runCycleTime = clip.length;
            }
        }

        private void OnEnable()
        {
            localPlayer.ItemPickedUp += PlayPickUp;
            localPlayer.DeviceUsed += PlayUse;
        }

        private void OnDisable()
        {
            localPlayer.ItemPickedUp -= PlayPickUp;
            localPlayer.DeviceUsed -= PlayUse;
        }

        private void Start()
        {
            lastPosition = playerController.transform.position;
            lastMoveState = playerController.MoveState;
        }

        private void Update()
        {
            float speed = ReadHorizontalSpeed();
            animator.SetFloat(SpeedHash, ToSpeedBlend(speed), playerControlConfig.AnimationDampTime, Time.deltaTime);
            animator.SetFloat(MoveAnimSpeedHash, Mathf.Max(1f, speed / playerControlConfig.RunStrideSpeed), playerControlConfig.AnimationDampTime, Time.deltaTime);
            animator.SetBool(IsExteriorHash, playerController.MoveState == MoveState.Exterior);

            float targetHandWeight = localPlayer.Item != null ? 1f : 0f;
            handWeight = Mathf.MoveTowards(handWeight, targetHandWeight, playerControlConfig.HandIKBlendSpeed * Time.deltaTime);
        }

        // 지난 프레임부터 움직인 수평 거리로 속도를 잰다 (선내 이동은 중력 Move가 따로 있어 CharacterController.velocity를 쓸 수 없다)
        // 에어락으로 순간이동한 프레임은 이동 상태가 바뀌므로 0으로 본다
        private float ReadHorizontalSpeed()
        {
            Vector3 position = playerController.transform.position;
            bool isTeleported = playerController.MoveState != lastMoveState;
            Vector3 moved = position - lastPosition;
            lastPosition = position;
            lastMoveState = playerController.MoveState;

            if (isTeleported || Time.deltaTime <= 0f) return 0f;
            moved.y = 0f;
            return moved.magnitude / Time.deltaTime;
        }

        // 섞인 클립의 발 속도가 실제 속도와 같아지는 블렌드 값. WalkStrideSpeed까지는 Idle ~ Walk, RunStrideSpeed까지는 Walk ~ Run
        // 블렌드 트리는 두 클립의 주기 길이도 섞어 맞추므로 발 속도 = 섞인 보폭 / 섞인 주기 길이 (가중치에 정비례하지 않는다)
        private float ToSpeedBlend(float speed)
        {
            float walkStrideSpeed = playerControlConfig.WalkStrideSpeed;
            float runStrideSpeed = playerControlConfig.RunStrideSpeed;
            if (speed <= walkStrideSpeed) return speed / walkStrideSpeed * WalkBlend;
            if (speed >= runStrideSpeed) return RunBlend;

            float walkStride = walkStrideSpeed * walkCycleTime;     // 한 주기 동안 나아가는 거리 (m)
            float runStride = runStrideSpeed * runCycleTime;
            float runWeight = (speed * walkCycleTime - walkStride) / (runStride - walkStride - speed * (runCycleTime - walkCycleTime));
            return WalkBlend + runWeight * (RunBlend - WalkBlend);
        }

        private void PlayPickUp()
        {
            animator.SetTrigger(PickUpHash);
        }

        private void PlayUse()
        {
            animator.SetTrigger(UseHash);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            animator.SetLookAtWeight(playerControlConfig.LookAtWeight, playerControlConfig.LookAtBodyWeight, playerControlConfig.LookAtHeadWeight);
            animator.SetLookAtPosition(shoulderCamera.AimPoint);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handWeight);
            if (handWeight <= 0f || localPlayer.Item == null) return;

            // 들고 있는 재료의 양옆 (플레이어 오른쪽 방향으로 재료 크기의 절반)
            Bounds itemBounds = GetHandItemRenderer().bounds;
            Vector3 right = playerController.transform.right;
            float halfWidth = Mathf.Abs(right.x) * itemBounds.extents.x + Mathf.Abs(right.y) * itemBounds.extents.y + Mathf.Abs(right.z) * itemBounds.extents.z;
            animator.SetIKPosition(AvatarIKGoal.LeftHand, itemBounds.center - right * halfWidth);
            animator.SetIKPosition(AvatarIKGoal.RightHand, itemBounds.center + right * halfWidth);
        }

        private Renderer GetHandItemRenderer()
        {
            if (handItem != localPlayer.Item)
            {
                handItem = localPlayer.Item;
                handItemRenderer = handItem.GetComponentInChildren<Renderer>();
            }
            return handItemRenderer;
        }
    }
}
