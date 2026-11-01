using DG.Tweening;
using UnityEngine;

namespace OverFixed
{
    // 보관함. E로 양쪽 문을 열고 닫는다 (보관 기능은 범위 밖)
    public class Cabinet : MonoBehaviour
    {
        [SerializeField] private Transform leftDoor;
        [SerializeField] private Transform rightDoor;
        [SerializeField] private float openDistance = 1f;
        [SerializeField] private float openDuration = 3f;

        private Vector3 leftClosedPosition;
        private Vector3 rightClosedPosition;
        private bool isOpen;
        private bool isMoving;

        private void Awake()
        {
            leftClosedPosition = leftDoor.localPosition;
            rightClosedPosition = rightDoor.localPosition;
        }

        // 닫힌 위치에서 부모 로컬 좌우로 연다. 움직이는 동안은 받지 않는다
        public void ToggleDoors()
        {
            if (isMoving) return;

            isOpen = isOpen == false;
            isMoving = true;
            Vector3 openOffset = isOpen ? Vector3.right * openDistance : Vector3.zero;
            leftDoor.DOLocalMove(leftClosedPosition + openOffset, openDuration);
            rightDoor.DOLocalMove(rightClosedPosition - openOffset, openDuration).OnComplete(() => isMoving = false);
        }
    }
}
