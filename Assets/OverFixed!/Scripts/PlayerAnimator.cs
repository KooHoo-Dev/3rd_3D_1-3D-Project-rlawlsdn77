using UnityEngine;

namespace OverFixed
{
    // 플레이어 로봇의 애니메이션과 IK. 로봇 모델의 Animator와 같은 오브젝트에 붙는다 (OnAnimatorIK가 그 오브젝트에서만 불린다)
    // 선내는 속도로 Idle·Walk·Run을 섞고 선외는 Float. 줍기·장치 사용은 상체에 PickUp·Use를 한 번, 재료를 든 동안은 상체를 Hold 자세로 고정
    // IK: 머리·상체는 조준점을 보고, 재료를 들면 두 손을 재료 양옆에 붙인 뒤 LateUpdate에서 쥔 방향으로 돌린다
    // 재료를 든 동안은 손 목표를 어깨가 늘 닿는 거리 안에 두어 걸어도 손이 미끄러지지 않게 하고, 걸음에 맞춰 손과 재료를 위아래로만 조금 움직인다
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
        [SerializeField] private float averageTime = 0.5f;      // 골반 높이·어깨 자리 평균을 내는 시간 (초). 걸음 한 번(약 0.35초)보다 길게
        [SerializeField] private float handBobRate = 0.25f;     // 골반 위아래 출렁임 중 손·재료에 남길 비율 (걷기 골반 12cm → 손 3cm)
        [SerializeField] private float handReachRate = 0.92f;   // 손 목표를 어깨 평균 자리에서 팔 길이의 이 비율 안으로 당긴다 (어깨가 출렁여도 손이 늘 닿게)

        private Animator animator;
        private Vector3 lastPosition;
        private MoveState lastMoveState;
        private float handWeight;               // 손 IK 세기 (0 ~ 1). 들고 놓을 때 천천히 바뀐다
        private ItemObject handItem;            // 손 IK 대상 재료와 렌더러 (재료가 바뀔 때만 다시 찾는다)
        private Renderer handItemRenderer;
        private Vector3 handItemHoldPosition;   // 재료를 든 자리 (부모 기준). 손 목표는 이 자리로 정하고, 위아래 움직임은 LateUpdate에서 더한다
        private Vector3 handBob;                // 이번 프레임 손 목표와 재료에 더하는 위아래 움직임
        private bool hasAverage;
        private float hipsHeightAverage;        // 골반 높이 평균 (플레이어 기준)
        private float hipsBob;                  // 골반이 평균보다 높은 양 (m). 걸음에 맞춰 손·재료를 위아래로 움직이는 데 쓴다
        private Vector3 leftShoulderAverage;    // 어깨(위팔 뿌리) 평균 자리 (플레이어 기준)
        private Vector3 rightShoulderAverage;
        private float armLength;                // 어깨에서 손목까지 (위팔 + 아래팔)
        private float walkCycleTime;            // Walk, Run 클립 한 주기 길이 (초)
        private float runCycleTime;
        private Transform leftHand;
        private Transform rightHand;
        private Transform leftUpperArm;
        private Transform rightUpperArm;
        private Transform hips;
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
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            armLength = Vector3.Distance(leftUpperArm.position, leftLowerArm.position) + Vector3.Distance(leftLowerArm.position, leftHand.position);
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
            // 지난 프레임에 위아래로 옮긴 재료를 든 자리로 되돌린다 (손 목표가 늘 든 자리 기준이 되게)
            if (isHolding)
            {
                RefreshHandItem();
                handItem.transform.localPosition = handItemHoldPosition;
            }
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
            // 크기는 재료 모델 기준 상자로 잰다 (월드 기준 Renderer.bounds는 재료가 비스듬하면 커져서 바라보는 방향에 따라 손이 벌어진다)
            RefreshHandItem();
            Transform itemTransform = handItemRenderer.transform;
            Bounds localBounds = handItemRenderer.localBounds;
            Vector3 center = itemTransform.TransformPoint(localBounds.center);
            Vector3 right = playerController.transform.right;
            float halfWidth = Mathf.Abs(Vector3.Dot(itemTransform.TransformVector(localBounds.extents.x, 0f, 0f), right))
                + Mathf.Abs(Vector3.Dot(itemTransform.TransformVector(0f, localBounds.extents.y, 0f), right))
                + Mathf.Abs(Vector3.Dot(itemTransform.TransformVector(0f, 0f, localBounds.extents.z), right))
                + playerControlConfig.HandHoldGap;

            // 어깨가 늘 닿는 거리로 당긴 뒤 걸음에 맞춘 위아래 움직임을 더한다 (재료도 LateUpdate에서 같은 만큼 옮긴다)
            handBob = playerController.transform.up * (hipsBob * handBobRate);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, ToReachable(center - right * halfWidth, leftShoulderAverage) + handBob);
            animator.SetIKPosition(AvatarIKGoal.RightHand, ToReachable(center + right * halfWidth, rightShoulderAverage) + handBob);
        }

        // 어깨 평균 자리에서 팔 길이 × handReachRate보다 먼 목표는 그 거리로 당긴다
        // 팔을 다 뻗은 채면 손이 걸음마다 출렁이는 어깨를 따라 앞뒤로 미끄러지므로, 늘 닿는 거리에 두어 손이 목표에 머물게 한다
        private Vector3 ToReachable(Vector3 target, Vector3 shoulderAverage)
        {
            Vector3 shoulder = playerController.transform.TransformPoint(shoulderAverage);
            Vector3 toTarget = target - shoulder;
            float maxDistance = armLength * handReachRate;
            if (toTarget.magnitude <= maxDistance) return target;
            return shoulder + toTarget.normalized * maxDistance;
        }

        // 애니메이션과 IK가 끝난 뒤 손 뼈 회전을 재료를 쥔 방향(시선 기준)으로 덮어쓴다. 오른손은 왼손 값을 좌우로 뒤집는다
        // 재료도 손 목표와 같은 만큼 위아래로 옮긴다 (다음 프레임 Update에서 든 자리로 되돌린다)
        private void LateUpdate()
        {
            UpdateAverages();
            if (handWeight <= 0f || localPlayer.Item == null) return;

            Quaternion view = playerController.transform.rotation * Quaternion.Euler(playerController.VerticalLookAngle, 0f, 0f);
            Vector3 finger = playerControlConfig.HandGripFingerDirection;
            Vector3 thumb = playerControlConfig.HandGripThumbDirection;
            RotateHand(leftHand, leftHandAxes, view * Quaternion.LookRotation(finger, thumb));
            RotateHand(rightHand, rightHandAxes, view * Quaternion.LookRotation(Mirror(finger), Mirror(thumb)));

            RefreshHandItem();
            handItem.transform.position += handBob * handWeight;
        }

        // 골반 높이와 어깨 자리의 평균 (플레이어 기준). 애니메이션·IK가 끝난 뒤 읽어 다음 프레임 손 목표에 쓴다 (IK는 어깨를 옮기지 않는다)
        private void UpdateAverages()
        {
            Transform player = playerController.transform;
            float hipsHeight = player.InverseTransformPoint(hips.position).y;
            Vector3 leftShoulder = player.InverseTransformPoint(leftUpperArm.position);
            Vector3 rightShoulder = player.InverseTransformPoint(rightUpperArm.position);
            if (hasAverage == false)
            {
                hipsHeightAverage = hipsHeight;
                leftShoulderAverage = leftShoulder;
                rightShoulderAverage = rightShoulder;
                hasAverage = true;
            }

            float rate = 1f - Mathf.Exp(-Time.deltaTime / averageTime);
            hipsHeightAverage = Mathf.Lerp(hipsHeightAverage, hipsHeight, rate);
            leftShoulderAverage = Vector3.Lerp(leftShoulderAverage, leftShoulder, rate);
            rightShoulderAverage = Vector3.Lerp(rightShoulderAverage, rightShoulder, rate);
            hipsBob = hipsHeight - hipsHeightAverage;
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

        // 든 재료가 바뀌면 렌더러와 든 자리를 다시 읽는다 (든 자리는 LocalPlayer가 재료를 손에 붙일 때 정한 값)
        private void RefreshHandItem()
        {
            if (handItem == localPlayer.Item) return;

            handItem = localPlayer.Item;
            handItemRenderer = handItem.GetComponentInChildren<Renderer>();
            handItemHoldPosition = handItem.transform.localPosition;
        }
    }
}
