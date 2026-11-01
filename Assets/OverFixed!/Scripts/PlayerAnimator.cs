using UnityEngine;

namespace OverFixed
{
    // 플레이어 로봇의 애니메이션과 IK. 로봇 모델의 Animator와 같은 오브젝트에 붙는다 (OnAnimatorIK가 그 오브젝트에서만 불린다)
    // 선내는 속도로 Idle·Walk·Run을 섞고 선외는 Float. 줍기·장치 사용은 상체에 PickUp·Use를 한 번, 재료를 든 동안은 상체를 Hold 자세로 고정
    // IK: 머리·상체는 조준점을 보고, 재료를 들면 두 손을 재료 양옆에 붙인 뒤 LateUpdate에서 쥔 방향으로 돌린다
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveAnimSpeedHash = Animator.StringToHash("MoveAnimSpeed");
        private static readonly int IsExteriorHash = Animator.StringToHash("IsExterior");
        private static readonly int PickUpHash = Animator.StringToHash("PickUp");
        private static readonly int UseHash = Animator.StringToHash("Use");
        private static readonly int IsHoldingHash = Animator.StringToHash("IsHolding");

        // Move 블렌드 트리 값 (0 = Idle, 1 = Walk, 2 = Run)과 한 주기 길이를 읽을 클립 이름
        private const float WalkBlend = 1f;
        private const float RunBlend = 2f;
        private const string WalkClipName = "Walk";
        private const string RunClipName = "Run";

        [SerializeField] private PlayerControlConfig playerControlConfig;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private PlayerCamera playerCamera;

        private Animator animator;
        private Vector3 lastPosition;
        private MoveState lastMoveState;
        private float handWeight;               // 손 IK 세기 (0 ~ 1). 들고 놓을 때 천천히 바뀐다
        private ItemObject handItem;            // 손 IK 대상 재료와 렌더러 (재료가 바뀔 때만 다시 찾는다)
        private Renderer handItemRenderer;
        private float walkCycleTime;            // Walk, Run 클립 한 주기 길이 (초)
        private float runCycleTime;
        private Transform leftHand;
        private Transform rightHand;
        private Quaternion leftHandAxes;        // 손 뼈 기준으로 검지 쪽을 앞, 엄지 쪽을 위로 보는 회전
        private Quaternion rightHandAxes;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == WalkClipName) walkCycleTime = clip.length;
                if (clip.name == RunClipName) runCycleTime = clip.length;
            }

            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftHandAxes = ReadHandAxes(leftHand, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftThumbProximal);
            rightHandAxes = ReadHandAxes(rightHand, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightThumbProximal);
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

            bool isHolding = localPlayer.Item != null;
            animator.SetBool(IsHoldingHash, isHolding);
            handWeight = Mathf.MoveTowards(handWeight, isHolding ? 1f : 0f, playerControlConfig.HandIKBlendSpeed * Time.deltaTime);
        }

        // 지난 프레임부터 움직인 수평 거리로 잰 속도 (선내는 중력 Move가 따로 있어 CharacterController.velocity를 쓸 수 없다)
        // 에어락으로 순간이동한 프레임은 0
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

        // 섞인 클립의 발 속도가 실제 속도와 같아지는 블렌드 값 (발이 미끄러지지 않게)
        // 블렌드 트리는 주기 길이도 섞으므로 발 속도 = 섞인 보폭 / 섞인 주기 길이 (가중치에 정비례하지 않는다)
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

        // 재료를 든 채면 Use를 재생하지 않는다 (상체가 Hold 자세를 벗어나 한 손이 재료에서 떨어진다)
        private void PlayUse()
        {
            if (localPlayer.Item != null) return;
            animator.SetTrigger(UseHash);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            animator.SetLookAtWeight(playerControlConfig.LookAtWeight, playerControlConfig.LookAtBodyWeight, playerControlConfig.LookAtHeadWeight);
            animator.SetLookAtPosition(playerCamera.AimPoint);

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handWeight);
            if (handWeight <= 0f || localPlayer.Item == null) return;

            // 재료 양옆: 플레이어 오른쪽 방향으로 재료 크기의 절반 + HandHoldGap
            Bounds itemBounds = GetHandItemRenderer().bounds;
            Vector3 right = playerController.transform.right;
            float halfWidth = Mathf.Abs(right.x) * itemBounds.extents.x + Mathf.Abs(right.y) * itemBounds.extents.y + Mathf.Abs(right.z) * itemBounds.extents.z
                + playerControlConfig.HandHoldGap;
            animator.SetIKPosition(AvatarIKGoal.LeftHand, itemBounds.center - right * halfWidth);
            animator.SetIKPosition(AvatarIKGoal.RightHand, itemBounds.center + right * halfWidth);
        }

        // 애니메이션과 IK가 끝난 뒤 손 뼈 회전을 재료를 쥔 방향(시선 기준)으로 덮어쓴다. 오른손은 왼손 값을 좌우로 뒤집는다
        private void LateUpdate()
        {
            if (handWeight <= 0f || localPlayer.Item == null) return;

            Quaternion view = playerController.transform.rotation * Quaternion.Euler(playerController.VerticalLookAngle, 0f, 0f);
            Vector3 finger = playerControlConfig.HandGripFingerDirection;
            Vector3 thumb = playerControlConfig.HandGripThumbDirection;
            RotateHand(leftHand, leftHandAxes, view * Quaternion.LookRotation(finger, thumb));
            RotateHand(rightHand, rightHandAxes, view * Quaternion.LookRotation(Mirror(finger), Mirror(thumb)));
        }

        // 손 뼈 공간에서 검지 뿌리 쪽을 앞, 엄지 뿌리 쪽을 위로 보는 회전 (뼈 사이 위치는 자세와 상관없다)
        private Quaternion ReadHandAxes(Transform hand, HumanBodyBones finger, HumanBodyBones thumb)
        {
            Vector3 fingerDirection = hand.InverseTransformPoint(animator.GetBoneTransform(finger).position);
            Vector3 thumbDirection = hand.InverseTransformPoint(animator.GetBoneTransform(thumb).position);
            return Quaternion.LookRotation(fingerDirection, thumbDirection);
        }

        private void RotateHand(Transform hand, Quaternion handAxes, Quaternion grip)
        {
            hand.rotation = Quaternion.Slerp(hand.rotation, grip * Quaternion.Inverse(handAxes), handWeight);
        }

        private static Vector3 Mirror(Vector3 direction)
        {
            return new Vector3(-direction.x, direction.y, direction.z);
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
