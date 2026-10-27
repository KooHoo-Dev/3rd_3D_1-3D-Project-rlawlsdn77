using UnityEngine;

namespace OverFixed
{
    // 에어락 패널. 통과한 뒤 설 위치와 통과 시간을 알려 주고, 플레이어를 옮긴다
    // 통과 순서(잠금, 시간 대기, 이동 상태 전환)는 LocalPlayer가 진행한다
    public class AirLock : MonoBehaviour
    {
        [Header("설정값 (ModuleConfig의 AirlockTime으로 통과 시간 조절)")]
        [SerializeField] private ModuleConfig moduleConfig;

        [Header("통과한 뒤 플레이어가 설 위치")]
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

        // 위치를 바꾸는 동안만 컨트롤러를 끈다
        private static void Teleport(CharacterController player, Vector3 position)
        {
            player.enabled = false;
            player.transform.position = position;
            player.enabled = true;
        }
    }
}
