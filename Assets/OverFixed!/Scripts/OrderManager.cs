using System.Collections.Generic;
using Jay;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OverFixed
{
    // 주문 진행 매니저. 규칙은 OrderBoard가 갖고, 여기서는 시간을 넘기고 완료한 주문의 보상을 크레딧으로 바꾼다
    // 주문 후보는 원재료 주문(RequireModule이 빈 행)만. 가공품 주문은 모듈이 생기면 추가한다
    public class OrderManager : SingletonBase<OrderManager>
    {
        [Header("설정값 (OrderConfig로 동시 주문 수, 빈자리 보충 대기 시간 조절)")]
        [SerializeField] private OrderConfig orderConfig;

        [Header("주문을 완료하면 안정도를 올릴 코어")]
        [SerializeField] private Core core;

        public OrderBoard Board { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            var candidateRows = new List<OrderRow>();
            foreach (OrderRow orderRow in DataManager.GetOrderRows())
            {
                if (orderRow.RequireModule.Count == 0) candidateRows.Add(orderRow);
            }

            Board = new OrderBoard(candidateRows, orderConfig.MaxOrderCount, orderConfig.RefillDelay, candidateCount => Random.Range(0, candidateCount));
            Board.OrderCompleted += GiveReward;
            Board.OrderFailed += RecordFail;
        }

        // 항해 중에만 주문 시간이 흐른다
        private void Update()
        {
            if (GameManager.Instance.IsPlaying == false) return;
            Board.Tick(Time.deltaTime);
        }

        // 재료 하나를 주문에 넣는다. 맞는 주문이 없으면 false
        public bool SubmitItem(ItemType itemId)
        {
            return Board.Submit(itemId);
        }

        private void GiveReward(ActiveOrder order)
        {
            AccountManager.Instance.AddCredit(order.Row.Reward);
            core.PlusStability();
            GameManager.Instance.RecordOrderSuccess(order.Row.Reward);
        }

        private void RecordFail(ActiveOrder order)
        {
            GameManager.Instance.RecordOrderFail();
            core.MinusStability();
        }
    }
}
