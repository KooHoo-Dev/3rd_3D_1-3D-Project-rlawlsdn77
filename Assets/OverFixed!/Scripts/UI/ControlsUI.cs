using TMPro;
using UnityEngine;

namespace OverFixed
{
    // HUD 조작법 안내. 선내와 선외는 쓰는 키가 달라 이동 상태에 맞춰 그린다
    public class ControlsUI : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private TMP_Text keysValue;

        private void LateUpdate()
        {
            if (playerController.MoveState == MoveState.Interior)
            {
                keysValue.text = "WASD 이동 · Shift 달리기\nE 상호작용 · Q 놓기\nEsc 일시정지";
                return;
            }

            string moveKeys = playerController.IsJetUnlocked ? "WASD 이동 · Shift 빠르게\n" : "WASD 이동\n";
            keysValue.text = moveKeys + "Space 위 · Ctrl 아래\nE 상호작용 · Q 놓기\nEsc 일시정지";
        }
    }
}
