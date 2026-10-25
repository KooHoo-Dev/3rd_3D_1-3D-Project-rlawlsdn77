using UnityEngine;

namespace OverFixed
{
    // 재료 오브젝트. 자기가 어떤 재료인지만 가진다
    // 재료의 수치(운반 속도, 생성 비율 등)는 갖지 않고 ItemTable의 행을 받아 쓴다
    public class ItemObject : MonoBehaviour
    {
        [SerializeField] private ItemType itemId;

        public ItemType ItemID => itemId;

        public ItemRow Row => DataManager.GetItem(itemId);
    }
}
