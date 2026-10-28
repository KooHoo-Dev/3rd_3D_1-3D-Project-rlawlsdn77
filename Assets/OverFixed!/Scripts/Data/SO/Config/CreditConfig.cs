using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/CreditConfig")]
    public class CreditConfig : ScriptableObject
    {
        [Header("크레딧")]
        [SerializeField] public int StartCredit = 100;
    }
}