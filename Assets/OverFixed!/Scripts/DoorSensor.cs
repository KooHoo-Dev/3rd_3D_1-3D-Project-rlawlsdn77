using System;
using DG.Tweening;
using UnityEngine;

namespace OverFixed
{
    public class DoorSensor : MonoBehaviour
    {
        [SerializeField] private GameObject LeftDoor;
        [SerializeField] private GameObject RightDoor;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                LeftDoor.transform.DOMove(LeftDoor.transform.position + Vector3.right, 1f);
                RightDoor.transform.DOMove(RightDoor.transform.position + Vector3.left, 1f);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                LeftDoor.transform.DOMove(LeftDoor.transform.position + Vector3.left, 1f);
                RightDoor.transform.DOMove(RightDoor.transform.position + Vector3.right, 1f);
            }
        }
    }
}