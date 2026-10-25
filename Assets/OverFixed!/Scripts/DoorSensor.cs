using DG.Tweening;
using UnityEngine;

namespace OverFixed
{
    public class DoorSensor : MonoBehaviour
    {
        [SerializeField] private GameObject leftDoor;
        [SerializeField] private GameObject rightDoor;

        // 닫힌 위치를 기준으로 열고 닫는다 (현재 위치 기준이면 다 열리기 전에 드나들 때 문이 점점 밀린다)
        private Vector3 leftClosedPosition;
        private Vector3 rightClosedPosition;

        private void Awake()
        {
            leftClosedPosition = leftDoor.transform.position;
            rightClosedPosition = rightDoor.transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // 문 자신의 오른쪽 축(transform.right)으로 연다. 문을 돌려 놓으면 축도 같이 돌아서 항상 바깥으로 열린다
                MoveDoors(leftClosedPosition + leftDoor.transform.right, rightClosedPosition - rightDoor.transform.right);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                MoveDoors(leftClosedPosition, rightClosedPosition);
            }
        }

        // 움직이던 트윈을 멈추고 목표 위치로 새로 움직인다
        private void MoveDoors(Vector3 leftTarget, Vector3 rightTarget)
        {
            leftDoor.transform.DOKill();
            rightDoor.transform.DOKill();
            leftDoor.transform.DOMove(leftTarget, 1f);
            rightDoor.transform.DOMove(rightTarget, 1f);
        }
    }
}
