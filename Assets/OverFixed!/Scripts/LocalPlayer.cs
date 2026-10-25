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

        private void Awake() 
        {
            interactionCube = GetComponentInChildren<InteractionCube>();
            playerController = GetComponent<PlayerController>();
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
            // 선내면 선외로, 선외면 선내로 전환
            if (playerController.MoveState == MoveState.Interior) 
                playerController.MoveState = MoveState.Exterior;
            else playerController.MoveState = MoveState.Interior;
        }

        private void OpenBox(GameObject box)
        {
        }
        
        private void TakeItem(GameObject item)
        {
            if (Item != null) return;

            item.GetComponent<Collider>().enabled = false;
            if (item.TryGetComponent(out Rigidbody itemBody)) itemBody.isKinematic = true;
            item.transform.SetParent(transform);    // 로컬 플레이어의 자식으로 아이템이 들어간다
            item.transform.localPosition = holdOffset; // 위치는 holdOffset만큼 떨어진 위치
            item.transform.localRotation = Quaternion.identity; // 회전은 고정

            // 줍은 아이템을 플레이어의 아이템으로 등록
            Item = item;
        }
        
        public void LeaveItem()
        {
            if (Item == null) return;

            Item.transform.SetParent(null);
            Item.transform.position = transform.position + transform.forward * holdOffset.z;
            Item.GetComponent<Collider>().enabled = true;
            if (Item.TryGetComponent(out Rigidbody itemBody)) itemBody.isKinematic = false;

            // 플레이어의 아이템을 비움
            Item = null;
        }

        [SerializeField] private TerminalManager terminalManager;

        // 터미널과 상호작용
        private void AccessTerminal(GameObject terminal)
        {
            terminalManager.AccessTerminal();
        }

        // 패널 조작하기 
        private void FixPanel(GameObject panel) 
        {
            
        }
        
        private void OpenCabinet(GameObject cabinet)
        {
        }
    }
}
