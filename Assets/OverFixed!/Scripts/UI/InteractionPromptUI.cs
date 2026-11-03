using TMPro;
using UnityEngine;

namespace OverFixed
{
    // HUD 상호작용 안내. 가장 가까운 대상 앞에서 E로 할 일("크리스탈 줍기")을, 지금 할 수 없으면 이유를 흐리게 보인다
    // 대상별 규칙은 LocalPlayer.Interact와 같다. 보관함처럼 시연에서 안내하지 않는 대상과 조작이 잠긴 동안에는 숨긴다
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private InteractionCube interactionCube;
        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private OrderManager orderManager;

        [Header("화면")]
        [SerializeField] private GameObject prompt;
        [SerializeField] private TMP_Text keyLabel;             // "E"
        [SerializeField] private TMP_Text labelValue;
        [SerializeField] private Color unavailableColor = Color.gray;

        // 할 수 있을 때의 색 (프리팹에 정해 둔 색)
        private Color keyLabelColor;
        private Color labelValueColor;

        private void Start()
        {
            keyLabelColor = keyLabel.color;
            labelValueColor = labelValue.color;
            prompt.SetActive(false);
        }

        // 플레이어가 이번 프레임에 움직인 뒤의 위치로 찾도록 LateUpdate에서 그린다
        private void LateUpdate()
        {
            if (playerController.IsControlLocked)
            {
                prompt.SetActive(false);
                return;
            }

            GameObject target = interactionCube.FindNearestTarget();
            string label;
            bool isAvailable;
            if (TryGetPrompt(target, out label, out isAvailable) == false)
            {
                prompt.SetActive(false);
                return;
            }

            labelValue.text = label;
            keyLabel.color = isAvailable ? keyLabelColor : unavailableColor;
            labelValue.color = isAvailable ? labelValueColor : unavailableColor;
            prompt.SetActive(true);
        }

        // 안내하지 않는 대상이면 false
        private bool TryGetPrompt(GameObject target, out string label, out bool isAvailable)
        {
            label = null;
            isAvailable = true;
            if (target == null) return false;

            ItemObject heldItem = localPlayer.Item;

            ItemObject item = target.GetComponent<ItemObject>();
            if (item != null)
            {
                isAvailable = heldItem == null;
                label = isAvailable ? item.Row.Name + " 줍기" : "손이 찼음";
                return true;
            }

            if (target.GetComponent<Terminal>() != null)
            {
                if (heldItem == null)
                {
                    label = "터미널 열기";
                    return true;
                }

                isAvailable = orderManager.Board.CanSubmit(heldItem.ItemID);
                label = isAvailable ? heldItem.Row.Name + " 제출" : "맞는 주문 없음";
                return true;
            }

            AccidentPanel accidentPanel = target.GetComponent<AccidentPanel>();
            if (accidentPanel != null)
            {
                isAvailable = accidentPanel.IsAccident;
                label = isAvailable ? "사고 처리" : "이상 없음";
                return true;
            }

            if (target.GetComponent<AirLock>() != null)
            {
                label = playerController.MoveState == MoveState.Interior ? "선외로 나가기" : "선내로 들어가기";
                return true;
            }

            return false;
        }
    }
}
