using UnityEngine;

namespace OverFixed
{
    // 에어락. 통과 시간과 통과한 뒤 설 위치를 갖고 플레이어를 옮긴다 (통과 순서는 LocalPlayer)
    public class AirLock : MonoBehaviour
    {
        [SerializeField] private ModuleConfig moduleConfig;
        [SerializeField] private Transform entryPoint;  // 선내 (AirlockInsidePoint)
        [SerializeField] private Transform exitPoint;   // 지붕 위 (AirlockRoofPoint)

        public float PassTime => moduleConfig.AirlockTime;

        public void ToExit(CharacterController player)
        {
            Teleport(player, exitPoint.position);
        }

        public void ToEntry(CharacterController player)
        {
            Teleport(player, entryPoint.position);
        }

        // CharacterController가 켜져 있으면 위치를 덮어써서 옮기는 동안만 끈다
        private static void Teleport(CharacterController player, Vector3 position)
        {
            player.enabled = false;
            player.transform.position = position;
            player.enabled = true;
        }
    }
}
