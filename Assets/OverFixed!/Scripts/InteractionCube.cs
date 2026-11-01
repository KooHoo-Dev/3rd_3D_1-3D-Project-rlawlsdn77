using UnityEngine;

namespace OverFixed
{
    // 상호작용 범위. 범위(boxCenter, boxSize) 안에서 상호작용 태그가 붙은 가장 가까운 대상을 찾는다
    // 대상별 행동은 LocalPlayer, 안내 문구는 InteractionPromptUI가 정한다
    public class InteractionCube : MonoBehaviour
    {
        private static readonly string[] InteractableTags = { "Item", "Terminal", "Panel", "Cabinet", "AirLock" };

        [SerializeField] private Vector3 boxCenter = Vector3.zero;
        [SerializeField] private Vector3 boxSize = Vector3.one;
        [SerializeField] private Color gizmoColor = Color.green;

        public GameObject FindNearestTarget()
        {
            Vector3 worldCenter = transform.TransformPoint(boxCenter);
            Vector3 worldHalfSize = Vector3.Scale(boxSize, transform.lossyScale) * 0.5f;

            GameObject nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Collider hit in Physics.OverlapBox(worldCenter, worldHalfSize, transform.rotation))
            {
                if (IsInteractable(hit.gameObject) == false) continue;

                float distance = Vector3.Distance(worldCenter, hit.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = hit.gameObject;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }

        private static bool IsInteractable(GameObject target)
        {
            foreach (string interactableTag in InteractableTags)
            {
                if (target.CompareTag(interactableTag)) return true;
            }
            return false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}
