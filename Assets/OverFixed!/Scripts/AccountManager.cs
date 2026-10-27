using System;
using Jay;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OverFixed 
{
    public class AccountManager : SingletonBase<AccountManager>
    {
        [Header("Gold")]
        [SerializeField] private CreditConfig creditConfig;
        
        [SerializeField] private LocalPlayer localPlayer;
        
        public int CurrentGold { get; private set; }

        private void Start()
        {
            // 첫 골드 지급
            CurrentGold = creditConfig.StartCredit; 
        }
        
        private void GetAccount()
        {
            Debug.Log(CurrentGold);
        }
        
        public void SetAccount(GameObject orderedItem)
        {
            // AccountManager에게 아이템 위임
            orderedItem.transform.SetParent(transform);
            orderedItem.SetActive(false);
            
        }
    }
}
