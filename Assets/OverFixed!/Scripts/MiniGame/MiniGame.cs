using System;
using UnityEngine;

namespace OverFixed
{
    // 패널 사고 미니게임 한 종류의 내용 (전략 패턴). 미니게임 화면(MiniGameUI)의 내용 영역에 종류마다 하나씩 둔다
    // 화면이 이 종류의 사고로 열리면 Begin, 닫히면 End가 불린다. 성공 1회마다 Succeed()를 부르면 Panel이 횟수를 센다
    // 해결 판정(누적 성공 횟수), 제한시간, 시간 초과는 Panel이 맡으므로 여기서는 성공 1회만 판정한다
    public abstract class MiniGame : MonoBehaviour
    {
        // 이 미니게임이 맡는 사고 종류 (EventTable의 EventID)
        public abstract EventType EventID { get; }

        // 성공 1회. MiniGameUI가 구독해서 Panel에 넘긴다
        public Action Succeeded;

        // 화면이 열릴 때. successCount는 이번 사고에서 이미 모은 성공 횟수 (닫았다 다시 열면 이어서 한다)
        public virtual void Begin(EventRow eventRow, int successCount)
        {
            gameObject.SetActive(true);
        }

        // 화면이 닫힐 때 (해결, 시간 초과, Esc·Q·닫기 버튼, 정산·게임오버 화면)
        public virtual void End()
        {
            gameObject.SetActive(false);
        }

        protected void Succeed()
        {
            Succeeded?.Invoke();
        }
    }
}
