using System;
using System.Collections.Generic;
using UnityEngine;

namespace OverFixed
{
    // InteractionCube Collider 안에 있는 물체의 Tag를 판단하는 건 InteractionCube이,
    // Tag에 따른 특정 행동을 수행하는 것은 LocalPlayer이 담당한다.
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public class InteractionCube : MonoBehaviour
    {
        public Action<GameObject> InteractItem;       
        public Action<GameObject> InteractTerminal;   
        public Action<GameObject> InteractPanel;      
        
        private readonly List<GameObject> targetList = new List<GameObject>();
        
        private void OnTriggerEnter(Collider other)
        {
            if (IsTarget(other.gameObject)) targetList.Add(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            targetList.Remove(other.gameObject);
        }
        
        // PlayerController에서 e를 누르면 호출됨
        public void Interact()
        {
            GameObject target = FindNearestTarget();
            if (target == null) return;

            if (target.CompareTag("Item")) InteractItem?.Invoke(target);
            else if (target.CompareTag("Terminal")) InteractTerminal?.Invoke(target);
            else if (target.CompareTag("Panel")) InteractPanel?.Invoke(target);
        }

        private static bool IsTarget(GameObject target)
        {
            return target.CompareTag("Item") || target.CompareTag("Terminal") || target.CompareTag("Panel");
        }

        // 사라졌거나 콜라이더가 꺼진 대상(손에 든 아이템)은 빼고 가장 가까운 것
        private GameObject FindNearestTarget()
        {
            targetList.RemoveAll(target => target == null || target.activeInHierarchy == false || target.GetComponent<Collider>().enabled == false);

            GameObject nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (GameObject target in targetList)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = target;
                    nearestDistance = distance;
                }
            }
            return nearest;
        }
    }
}
