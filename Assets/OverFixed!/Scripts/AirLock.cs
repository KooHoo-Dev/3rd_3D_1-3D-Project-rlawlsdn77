using System;
using UnityEngine;

namespace OverFixed
{
    public class AirLock : MonoBehaviour
    {
        [Header("AirLock Transform")]
        [SerializeField] private Transform EntryTransform;
        [SerializeField] private Transform ExitTransform;
        
        public void ToExit(CharacterController player)
        {
            Teleport(player, ExitTransform.position);
        }

        public void ToEntry(CharacterController player)
        {
            Teleport(player, EntryTransform.position);
        }
        
        // 위치를 바꾸는 동안만 컨트롤러를 끈다
        private static void Teleport(CharacterController player, Vector3 position)
        {
            player.enabled = false;
            player.transform.position = position;
            player.enabled = true;
        }
    }
}