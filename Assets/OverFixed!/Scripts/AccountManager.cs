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

        // 터미널의 안정화 물질 구매. 규칙은 GameManager.TryBuyStabilizer (PLANET 정비도 같은 규칙), 사면 크레딧 변화를 알린다
        public bool BuyStabilizer()
        {
            if (GameManager.Instance.TryBuyStabilizer() == false) return false;

            CreditChanged?.Invoke(CurrentCredit);
            return true;
        }
    }
}
