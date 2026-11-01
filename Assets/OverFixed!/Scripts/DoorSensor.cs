using DG.Tweening;
using UnityEngine;

namespace OverFixed
{
    // 자동문. 플레이어가 센서에 들어오면 양쪽 문을 열고, 나가면 닫는다
    public class DoorSensor : MonoBehaviour
    {
        [SerializeField] private GameObject leftDoor;
        [SerializeField] private GameObject rightDoor;
        [SerializeField] private float moveDuration = 1f;

        // 닫힌 위치 기준으로 움직인다 (현재 위치 기준이면 다 열리기 전에 드나들 때 문이 밀린다)
        private Vector3 leftClosedPosition;
        private Vector3 rightClosedPosition;

        private void Awake()
        {
            leftClosedPosition = leftDoor.transform.position;
            rightClosedPosition = rightDoor.transform.position;
        }

        // 문 자신의 오른쪽 축으로 연다 (문을 돌려 놓아도 바깥으로 열린다)
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) MoveDoors(leftClosedPosition + leftDoor.transform.right, rightClosedPosition - rightDoor.transform.right);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) MoveDoors(leftClosedPosition, rightClosedPosition);
        }

        private void MoveDoors(Vector3 leftTarget, Vector3 rightTarget)
        {
            leftDoor.transform.DOKill();
            rightDoor.transform.DOKill();
            leftDoor.transform.DOMove(leftTarget, moveDuration);
            rightDoor.transform.DOMove(rightTarget, moveDuration);
        }
    }
}
