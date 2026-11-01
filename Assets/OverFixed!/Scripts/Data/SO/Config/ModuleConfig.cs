using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/ModuleConfig")]
    public class ModuleConfig : ScriptableObject
    {
        [Header("에어락")]
        public float AirlockTime = 0.5f;            // 감압·가압 시간 (초). 이 동안 이동과 상호작용이 잠긴다
    }
}
