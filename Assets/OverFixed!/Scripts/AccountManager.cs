using System;
using Jay;
using UnityEngine;

namespace OverFixed
{
    public class AccountManager : SingletonBase<AccountManager>
    {
        // 보유 크레딧. 씬이 바뀌어도 이어지도록 값은 GameManager가 갖는다 (시작 크레딧도 GameManager가 넣는다)
        public int CurrentCredit => GameManager.Instance.Credit;

        // 보유 크레딧이 바뀌면 바뀐 값을 알린다. HUD가 구독한다
        public Action<int> CreditChanged;

        public void AddCredit(int credit)
        {
            GameManager.Instance.Credit += credit;
            CreditChanged?.Invoke(CurrentCredit);
        }
    }
}
