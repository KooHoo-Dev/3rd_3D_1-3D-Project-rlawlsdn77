using UnityEngine;

namespace OverFixed
{
    [CreateAssetMenu(menuName = "OverFixed/CreditConfig")]
    public class CreditConfig : ScriptableObject
    {
        [Header("Gold")]
        
        [SerializeField] public int StartCredit = 100;
        [SerializeField] public int MaxCredit = 1000;
    }
}