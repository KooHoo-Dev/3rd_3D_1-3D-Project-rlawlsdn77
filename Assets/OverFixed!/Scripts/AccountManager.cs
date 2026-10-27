using System;
using Jay;
using UnityEngine;

namespace OverFixed
{
    public class AccountManager : SingletonBase<AccountManager>
    {
        [Header("크레딧")]
        [SerializeField] private CreditConfig creditConfig;

        [SerializeField] private LocalPlayer localPlayer;

        public int CurrentCredit { get; private set; }

        // 보유 크레딧이 바뀌면 바뀐 값을 알린다. HUD가 구독한다
        public Action<int> CreditChanged;

        private void Start()
        {
            // 첫 크레딧 지급
            CurrentCredit = creditConfig.StartCredit;
            CreditChanged?.Invoke(CurrentCredit);
        }

        private void GetAccount()
        {
            Debug.Log(CurrentCredit);
        }

        public void AddCredit(int credit)
        {
            CurrentCredit += credit;
            CreditChanged?.Invoke(CurrentCredit);
        }
    }
}
