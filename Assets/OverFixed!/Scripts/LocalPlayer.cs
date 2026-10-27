using System;
using UnityEngine;

namespace OverFixed
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField]
        private InteractionCube interactionCube;
        
        [SerializeField] // 집은 오브젝트의 오프셋을 표시하는 프로퍼티
        private Vector3 holdOffset = new Vector3(0f, 0f, 2f); 
        
        public GameObject Item { get; private set; }
        
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
            // 선내면 선외로, 선외면 선내로 이동
            if (playerController.MoveState == MoveState.Interior)
            {
                playerController.SetMoveState(MoveState.Exterior);
                airlock.GetComponent<AirLock>().ToExit(characterController);
            }
            else
            {
                playerController.SetMoveState(MoveState.Interior);
                airlock.GetComponent<AirLock>().ToEntry(characterController);
            }
        }

        private void OpenBox(GameObject box)
        {
        }
        
        private void TakeItem(GameObject item)
        {
            if (Item != null) return;
            
            SpawnManager.Instance.StopItemMove(item);

            item.GetComponent<Collider>().enabled = false;
            if (item.TryGetComponent(out Rigidbody itemBody)) itemBody.isKinematic = true;
            item.transform.SetParent(transform);    // 로컬 플레이어의 자식으로 아이템이 들어간다
            item.transform.localPosition = holdOffset; // 위치는 holdOffset만큼 떨어진 위치
            item.transform.localRotation = Quaternion.identity; // 회전은 고정
            
            if (item.TryGetComponent(out ItemObject itemObject)) item.transform.localScale = itemObject.OriginalScale;

            // 줍은 아이템을 플레이어의 아이템으로 등록
            Item = item;
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

        // 터미널과 상호작용
        private void AccessTerminal(GameObject terminal)
        { 
            terminal.GetComponent<Terminal>().AccessTerminal();
        }

        // 패널 조작하기 
        private void FixPanel(GameObject panel) 
        {
            
        }
        
        private void OpenCabinet(GameObject cabinet)
        {
            // 1. 플레이어가 아이템을 들고 있을 시 그 아이템을 보유 아이템 목록에 넣는다
            // 2. 플레이어가 아이템을 들고 있지 않을 시 보유 아이템 목록 UI를 띄운다. (아이템 클릭 시 그 아이템을 손에 들게 됨)
            // 3. 보유 아이템 목록의 아이템은 터미널의 제출 아이템과 연동되지 않는다 (터미널 제출 아이템은 직접 손으로 들고 가야함)
            // 4. 보유 아이템 목록은 다음 행성 이동시에도 아이템이 사라지지 않고 남아있다.
            cabinet.GetComponent<Cabinet>().OpenCabinet();    
        }
    }
}
