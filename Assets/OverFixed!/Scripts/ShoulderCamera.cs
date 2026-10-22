using UnityEngine;

namespace OverFixed
{
    // 데드 스페이스식 근접 숄더뷰
    // 플레이어(좌우 회전) → 카메라 피벗(상하 회전, 오른쪽 어깨) → 카메라(피벗 뒤)
    // 벽이 가리면 카메라를 어깨 쪽으로 바로 당기고, 풀리면 천천히 돌아간다
    [RequireComponent(typeof(PlayerController))]
    public class ShoulderCamera : MonoBehaviour
    {
        [Header("설정값 (playerConfig로 밸런스 조정)")]
        [SerializeField] private PlayerConfig playerConfig;

        [Header("카메라 (피벗은 플레이어의 자식, 카메라는 피벗의 자식)")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;

        // 아래는 내부변수들
        private PlayerController playerController;
        private Renderer[] playerRenderers;                         // 숨길 수 있는 캐릭터 렌더러 (처음부터 켜져 있던 것만)
        private readonly RaycastHit[] hitBuffer = new RaycastHit[16];
        private float currentShoulderOffset;                        // 벽에 밀려 줄어든 지금의 어깨 오프셋
        private float currentDistance;                              // 벽에 밀려 줄어든 지금의 카메라 거리
        private bool isPlayerHidden;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerRenderers = System.Array.FindAll(GetComponentsInChildren<Renderer>(), playerRenderer => playerRenderer.enabled);
            currentShoulderOffset = playerConfig.ShoulderOffset;
            currentDistance = playerConfig.CameraDistance;
        }

        // 플레이어 이동과 회전(Update)이 끝난 뒤에 카메라를 맞춰야 떨리지 않는다
        private void LateUpdate()
        {
            playerCamera.fieldOfView = playerConfig.CameraFieldOfView;
            cameraPivot.localRotation = Quaternion.Euler(playerController.VerticalLookAngle, 0f, 0f);

            // 1. 머리 → 오른쪽 어깨 : 오른쪽에 벽이 있으면 어깨 오프셋을 줄인다
            Vector3 headPosition = transform.TransformPoint(0f, playerConfig.PivotHeight, 0f);
            float shoulderLimit = CastLimit(headPosition, transform.right, playerConfig.ShoulderOffset);
            currentShoulderOffset = FollowLimit(currentShoulderOffset, shoulderLimit);
            cameraPivot.localPosition = new Vector3(currentShoulderOffset, playerConfig.PivotHeight, 0f);

            // 2. 어깨 → 카메라 : 뒤가 막히면 카메라 거리를 줄인다
            float distanceLimit = CastLimit(cameraPivot.position, -cameraPivot.forward, playerConfig.CameraDistance);
            currentDistance = FollowLimit(currentDistance, distanceLimit);
            playerCamera.transform.localPosition = new Vector3(0f, 0f, -currentDistance);
            playerCamera.transform.localRotation = Quaternion.identity;

            // 3. 카메라가 머리에 너무 붙으면 캐릭터를 숨긴다
            bool isTooClose = Vector3.Distance(playerCamera.transform.position, headPosition) < playerConfig.HidePlayerDistance;
            if (isTooClose != isPlayerHidden) SetPlayerHidden(isTooClose);
        }

        // origin에서 direction으로 maxDistance까지 구를 쏴서, 벽에 막히기 전까지 갈 수 있는 거리
        // 플레이어 자신(손에 든 물건 포함)과 트리거는 벽으로 치지 않는다
        private float CastLimit(Vector3 origin, Vector3 direction, float maxDistance)
        {
            int hitCount = Physics.SphereCastNonAlloc(origin, playerConfig.CameraRadius, direction, hitBuffer, maxDistance,
                playerConfig.CameraCollisionMask, QueryTriggerInteraction.Ignore);

            float limit = maxDistance;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = hitBuffer[index];
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.distance < limit) limit = hit.distance;
            }
            return limit;
        }

        // 막히면 그 프레임에 바로 당기고, 풀리면 CameraTweenSpeed로 천천히 돌아간다
        private float FollowLimit(float current, float limit)
        {
            if (limit < current) return limit;
            return Mathf.Lerp(current, limit, playerConfig.CameraTweenSpeed * Time.deltaTime);
        }

        private void SetPlayerHidden(bool isHidden)
        {
            isPlayerHidden = isHidden;
            foreach (Renderer playerRenderer in playerRenderers)
            {
                if (playerRenderer != null) playerRenderer.enabled = isHidden == false;
            }
        }
    }
}
