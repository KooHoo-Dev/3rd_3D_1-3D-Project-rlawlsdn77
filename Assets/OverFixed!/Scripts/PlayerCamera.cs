using UnityEngine;

namespace OverFixed
{
    // 1인칭 카메라. 플레이어(좌우 회전) → 카메라 피벗(상하 회전) → 카메라
    // 머리(상호작용 범위가 붙어 있음)는 화면 중앙이 가리키는 지점을 본다
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Transform playerHead;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float maxAimDistance = 30f;

        public Vector3 AimPoint { get; private set; }   // 화면 중앙이 가리키는 지점. 로봇 머리 IK(PlayerAnimator)도 여기를 본다

        private PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        // 플레이어 이동·회전(Update)이 끝난 뒤에 맞춰야 떨리지 않는다
        private void LateUpdate()
        {
            cameraPivot.localRotation = Quaternion.Euler(playerController.VerticalLookAngle, 0f, 0f);

            AimPoint = FindAimPoint();
            Vector3 lookDirection = AimPoint - playerHead.position;
            if (lookDirection.sqrMagnitude > 0.0001f) playerHead.rotation = Quaternion.LookRotation(lookDirection, transform.up);
        }

        // 화면 중앙에서 쏜 레이가 처음 맞는 지점. 플레이어 자신, 트리거, 머리보다 가까운 것은 건너뛰고 없으면 maxAimDistance 거리
        private Vector3 FindAimPoint()
        {
            Ray centerRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            float headDistance = Vector3.Dot(playerHead.position - centerRay.origin, centerRay.direction);

            Vector3 aimPoint = centerRay.GetPoint(maxAimDistance);
            float nearestDistance = maxAimDistance;
            foreach (RaycastHit hit in Physics.RaycastAll(centerRay, maxAimDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.IsChildOf(transform) || hit.distance <= headDistance) continue;
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
