using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/OrderConfig")]
    public class OrderConfig : ScriptableObject
    {
        [Header("주문")]
        public int MaxOrderCount = 3;               // 동시에 뜨는 주문 수
        public float RefillDelay = 5f;              // 끝난 칸을 새 주문으로 채우기까지 (초)
    }
}
