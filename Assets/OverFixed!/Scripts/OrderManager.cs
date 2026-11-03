using System.Collections.Generic;
using Jay;
using UnityEngine;

namespace OverFixed
{
    // 항해 중 주문판(OrderBoard)의 시간을 넘기고, 끝난 주문을 GameManager에 알린다
    // 후보는 원재료 주문(RequireModule이 빈 행)만
    public class OrderManager : SingletonBase<OrderManager>
    {
        [SerializeField] private OrderConfig orderConfig;

        public OrderBoard Board { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            List<OrderRow> candidateRows = new List<OrderRow>();
            foreach (OrderRow orderRow in DataManager.GetOrderRows())
            {
                if (orderRow.RequireModule.Count == 0) candidateRows.Add(orderRow);
            }

            Board = new OrderBoard(candidateRows, orderConfig.MaxOrderCount, orderConfig.RefillDelay);
            Board.OrderCompleted += order => GameManager.Instance.CompleteOrder(order.Row.Reward);
            Board.OrderFailed += order => GameManager.Instance.FailOrder();
        }

        private void Update()
        {
            if (GameManager.Instance.IsPlaying) Board.Tick(Time.deltaTime);
        }
    }
}
