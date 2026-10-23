using UnityEngine;

namespace OverFixed
{
    // 숄더뷰 카메라
    // 플레이어(좌우 회전, PlayerController) → 카메라 피벗(상하 회전) → 카메라(피벗의 자식)
    // 변경!!! 머리는 화면 중앙이 가리키는 지점을 바라본다 (3인칭 FPS식 조준). 머리 아래 상호작용 박스도 같이 화면 중앙을 향한다
    [RequireComponent(typeof(PlayerController))]
    public class ShoulderCamera : MonoBehaviour
    {
        [Header("카메라 (피벗은 플레이어의 자식, 카메라는 피벗의 자식. 위치는 씬에서 조절)")]
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform playerHead;
        [SerializeField] private Camera playerCamera; // 변경!!! 화면 중앙 레이를 쏘는 카메라

        [Header("조준")]
        [SerializeField] private float maxAimDistance = 30f; // 변경!!! 화면 중앙 레이가 아무것도 맞히지 못하면 이 거리의 점을 바라본다

        // 아래는 내부변수들
        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        // 플레이어 이동과 회전(Update)이 끝난 뒤에 카메라를 맞춰야 떨리지 않는다
        private void LateUpdate()
        {
            // 피벗을 중심으로 상하 회전. 카메라는 피벗의 자식이라 에디터에서 둔 자리 그대로 같이 돈다
            cameraPivot.localRotation = Quaternion.Euler(playerController.VerticalLookAngle, 0f, 0f);

            // 변경!!! 머리 = 피벗과 같은 각도 → 화면 중앙이 가리키는 지점을 바라보게
            Vector3 aimPoint = FindAimPoint();
            Vector3 lookDirection = aimPoint - playerHead.position;
            if (lookDirection.sqrMagnitude > 0.0001f) playerHead.rotation = Quaternion.LookRotation(lookDirection, transform.up);
        }

        // 화면 중앙에서 앞으로 레이를 쏴서 처음 맞는 지점. 없으면 maxAimDistance 거리의 점
        // 플레이어 자신(손에 든 물건 포함), 트리거, 카메라와 머리 사이에 있는 것은 건너뛴다
        private Vector3 FindAimPoint()
        {
            Ray centerRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            float headDistance = Vector3.Dot(playerHead.position - centerRay.origin, centerRay.direction);

            Vector3 aimPoint = centerRay.GetPoint(maxAimDistance);
            float nearestDistance = maxAimDistance;
            foreach (RaycastHit hit in Physics.RaycastAll(centerRay, maxAimDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.distance <= headDistance) continue;
                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    aimPoint = hit.point;
                }
            }
            return aimPoint;
        }
    }
}
