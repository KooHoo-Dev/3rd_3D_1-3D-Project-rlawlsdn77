using System;
using System.Collections;
using UnityEngine;

namespace OverFixed
{
    // LocalPlayer은 너무 많은 의존성을 가지지 않는다.
    // 실제 플레이어가 하는 행동만을 담당한다.
    public class LocalPlayer : MonoBehaviour
    {
        [Header("상호작용 세팅")]
        [SerializeField] private InteractionCube interactionCube;
        [SerializeField] private Vector3 holdOffset = new Vector3(0f, 0f, 2f);

        public ItemObject Item { get; private set; }

        // 홀드 진행도 (0 ~ 1). 0이면 진행 중인 홀드가 없다. HUD 홀드 막대가 구독한다
        public Action<float> HoldProgressChanged;

        // 재료를 주웠을 때, 장치(터미널, 패널, 에어락)를 썼을 때. 로봇 애니메이션(PlayerAnimator)이 구독한다
        public Action ItemPickedUp;
        public Action DeviceUsed;

        private PlayerController playerController;
        private CharacterController characterController; 

        private void Awake()
        {
            interactionCube = GetComponentInChildren<InteractionCube>();
            playerController = GetComponent<PlayerController>();
            characterController = GetComponent<CharacterController>(); 
        }

        private void OnEnable()
        {
            interactionCube.InteractItem += TakeItem;
            interactionCube.InteractTerminal += AccessTerminal;
            interactionCube.InteractPanel += FixPanel;
            interactionCube.InteractCabinet += OpenCabinet;
            interactionCube.InteractionBox += OpenBox;
            interactionCube.InteractionAirLock += OpenAirLock;
        }

        private void OnDisable()
        {
            interactionCube.InteractItem -= TakeItem;
            interactionCube.InteractTerminal -= AccessTerminal;
            interactionCube.InteractPanel -= FixPanel;
            interactionCube.InteractCabinet -= OpenCabinet;
            interactionCube.InteractionBox -= OpenBox;
            interactionCube.InteractionAirLock -= OpenAirLock;
        }

        private void OpenAirLock(GameObject airlock)
        {
            DeviceUsed?.Invoke();
            StartCoroutine(PassAirLock(airlock.GetComponent<AirLock>()));
        }

        // 감압(가압) 시간 동안 이동과 상호작용을 잠그고 홀드 막대를 채운 뒤,
        // 선내면 선외로, 선외면 선내로 옮기고 이동 상태를 바꾼다
        private IEnumerator PassAirLock(AirLock airLock)
        {
            playerController.SetControlLock(true);

            float elapsedTime = 0f;
            while (elapsedTime < airLock.PassTime)
            {
                elapsedTime += Time.deltaTime;
                HoldProgressChanged?.Invoke(Mathf.Clamp01(elapsedTime / airLock.PassTime));
                yield return null;
            }

            if (playerController.MoveState == MoveState.Interior)
            {
                airLock.ToExit(characterController);
                playerController.SetMoveState(MoveState.Exterior);
            }
            else
            {
                airLock.ToEntry(characterController);
                playerController.SetMoveState(MoveState.Interior);
            }

            HoldProgressChanged?.Invoke(0f);

            // 감압 중에 항해가 끝났으면(정산, 게임오버 화면) 그 화면이 건 잠금을 풀지 않는다
            if (GameManager.Instance.IsPlaying) playerController.SetControlLock(false);
        }

        private void OpenBox(GameObject box)
        {
        }
        
        private void TakeItem(GameObject item)
        {
            if (Item != null) return;
            if (item.TryGetComponent(out ItemObject _) == false) return; // 재료 종류가 없는 물건은 들지 않는다

            // 흐르는 풀 아이템은 풀로 돌려보내고 같은 종류의 새 인스턴스를 든다. 바닥에 놓인 아이템은 그대로 든다
            GameObject heldItem = SpawnManager.Instance.CloneToPooledItem(item, transform);

            heldItem.GetComponent<Collider>().enabled = false;
            if (heldItem.TryGetComponent(out Rigidbody itemBody)) itemBody.isKinematic = true;
            heldItem.transform.SetParent(transform);    // 로컬 플레이어의 자식으로 아이템이 들어간다
            heldItem.transform.localPosition = holdOffset; // 위치는 holdOffset만큼 떨어진 위치
            heldItem.transform.localRotation = Quaternion.identity; // 회전은 고정

            ItemObject itemObject = heldItem.GetComponent<ItemObject>();
            heldItem.transform.localScale = itemObject.OriginalScale;

            // 줍은 아이템을 플레이어의 아이템으로 등록
            Item = itemObject;
            ItemPickedUp?.Invoke();
        }
        
        public void LeaveItem()
        {
            if (Item == null) return;

            Item.transform.SetParent(null);
            Item.transform.position = transform.position + transform.forward * holdOffset.z;
            Item.GetComponent<Collider>().enabled = true;
            if (Item.TryGetComponent(out Rigidbody itemBody))
            {
                // 선외에서는 아이템을 잡고 놓으면 놓은 자리에 떠 있게 한다
                itemBody.isKinematic = playerController.MoveState == MoveState.Exterior;
            }

            // 플레이어의 아이템을 비움
            Item = null;
        }

        // 터미널과 상호작용. 재료를 들고 있으면 제출, 빈손이면 터미널 화면을 연다
        private void AccessTerminal(GameObject terminal)
        {
            Terminal targetTerminal = terminal.GetComponent<Terminal>();
            if (Item == null)
            {
                DeviceUsed?.Invoke();
                targetTerminal.AccessTerminal();
                return;
            }

            // 맞는 주문이 없으면 받지 않고 손에 남는다
            if (targetTerminal.SubmitItem(Item) == false) return;

            Destroy(Item.gameObject);
            Item = null;
            DeviceUsed?.Invoke();
        }

        // 패널 조작하기. 사고가 없으면 반응하지 않는다
        private void FixPanel(GameObject panel)
        {
            Panel targetPanel = panel.GetComponent<Panel>();
            if (targetPanel.IsAccident == false) return;

            DeviceUsed?.Invoke();
            targetPanel.FixPanel();
        }
        
        private void OpenCabinet(GameObject cabinet)
        {
            // 1. 플레이어가 아이템을 들고 있을 시 그 아이템을 보유 아이템 목록에 넣는다
            // 2. 플레이어가 아이템을 들고 있지 않을 시 보유 아이템 목록 UI를 띄운다. (아이템 클릭 시 그 아이템을 손에 들게 됨)
            // 3. 보유 아이템 목록의 아이템은 터미널의 제출 아이템과 연동되지 않는다 (터미널 제출 아이템은 직접 손으로 들고 가야함)
            // 4. 보유 아이템 목록은 다음 행성 이동시에도 아이템이 사라지지 않고 남아있다.
            // 5. 터미널에서의 OrderBoard에서 캐비넷 내부의 아이템을 제출 가능하다.
            // 6. 터미널에서의 
            
            // interactionCube 안에 상호작용 할 시 OpenCabinet 한다.
            cabinet.GetComponent<Cabinet>().OpenCabinet();    
        }
    }
}
