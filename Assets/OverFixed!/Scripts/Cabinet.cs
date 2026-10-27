using DG.Tweening;
using UnityEngine;

namespace OverFixed
{
    public class Cabinet : MonoBehaviour
    {
        [SerializeField] private Transform LeftDoor;
        [SerializeField] private Transform RightDoor;
        
        [SerializeField] private float openDistance = 1f;
        [SerializeField] private float openDuration = 3f;

        private Vector3 leftClosedPosition;
        private Vector3 rightClosedPosition;
        private bool isOpen;
        private bool isMoving;

        private void Awake()
        {
            leftClosedPosition = LeftDoor.localPosition;
            rightClosedPosition = RightDoor.localPosition;
        }

        // 닫힌 위치 기준 부모 로컬 축으로 이동 
        public void OpenCabinet()
        {
            if (isMoving) return;

            isOpen = isOpen == false;
            isMoving = true;

            Vector3 leftTarget = isOpen ? leftClosedPosition + Vector3.right * openDistance : leftClosedPosition;
            Vector3 rightTarget = isOpen ? rightClosedPosition + Vector3.left * openDistance : rightClosedPosition;

            LeftDoor.DOLocalMove(leftTarget, openDuration);
            RightDoor.DOLocalMove(rightTarget, openDuration).OnComplete(() => isMoving = false);
        }
    }
}