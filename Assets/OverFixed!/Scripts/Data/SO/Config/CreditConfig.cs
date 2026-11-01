using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/CreditConfig")]
    public class CreditConfig : ScriptableObject
    {
        [Header("크레딧")]
        public int StartCredit = 100;               // 새 게임 시작 값
    }
}
