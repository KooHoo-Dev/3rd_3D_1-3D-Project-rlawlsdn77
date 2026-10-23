using System;
using UnityEngine;

namespace OverFixed
{
    // InteractionCube 범위 안에 있는 물체의 Tag를 판단하는 건 InteractionCube이,
    // Tag에 따른 특정 행동을 수행하는 것은 LocalPlayer이 담당한다.
    // 변경!!! Cube 메시와 BoxCollider 없이 범위를 boxCenter, boxSize로 정하고 씬 뷰에 기즈모로 그린다
    public class InteractionCube : MonoBehaviour
    {
        [Header("상호작용 박스 세팅설정")]
        [SerializeField] private Vector3 boxCenter = Vector3.zero;
        [SerializeField] private Vector3 boxSize = Vector3.one;    
        [SerializeField] private Color gizmoColor = Color.green;    

        public Action<GameObject> InteractItem;
        public Action<GameObject> InteractTerminal;
        public Action<GameObject> InteractPanel;
        public Action<GameObject> InteractCabinet;
        public Action<GameObject> InteractionBox;
        public Action<GameObject> InteractionAirLock;

        private const string Item = "Item";
        private const string Terminal = "Terminal";
        private const string Panel = "Panel";
        private const string Cabinet = "Cabinet";
        private const string Box = "Box";
        private const string AirLock = "AirLock";
        private const string Door = "Door";
        
        // PlayerController에서 e를 누르면 호출됨
        public void Interact()
        {
            GameObject target = FindNearestTarget();
            if (target == null) return;

            if (target.CompareTag(Item)) InteractItem?.Invoke(target);
            else if (target.CompareTag(Terminal)) InteractTerminal?.Invoke(target);
            else if (target.CompareTag(Panel)) InteractPanel?.Invoke(target);
            else if (target.CompareTag(Cabinet)) InteractCabinet?.Invoke(target);
            else if (target.CompareTag(Box)) InteractionBox?.Invoke(target);
            else if (target.CompareTag(AirLock)) InteractionAirLock?.Invoke(target);
        }

        private static bool IsTarget(GameObject target)
        {
            return target.CompareTag(Item) ||
                   target.CompareTag(Terminal) ||
                   target.CompareTag(Panel) ||
                   target.CompareTag(Cabinet) ||
                   target.CompareTag(Box) ||
                   target.CompareTag(AirLock);
        }

        // 지금 박스 안에 있는 대상 중 가장 가까운 것
        private GameObject FindNearestTarget()
        {
            // 로컬 좌표 -> 월드 좌표
            Vector3 worldCenter = transform.TransformPoint(boxCenter);
            Vector3 worldHalfSize = Vector3.Scale(boxSize, transform.localScale) * 0.5f;
            Collider[] hitList = Physics.OverlapBox(worldCenter, worldHalfSize, transform.rotation);

            GameObject nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider hit in hitList)
            {
                GameObject target = hit.gameObject;
                if (IsTarget(target) == false) continue;

                Debug.Log(target.name);

                float distance = Vector3.Distance(worldCenter, target.transform.position); // 변경!!! 범위 중심 기준 거리
                if (distance < nearestDistance)
                {
                    nearest = target;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}
