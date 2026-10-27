using System;
using System.Collections.Generic;

namespace OverFixed
{
    // 진행 중인 주문 하나. 주문 행(OrderRow)과 남은 시간, 재료별로 채운 개수를 가진다
    public class ActiveOrder
    {
        public OrderRow Row { get; }
        public float RemainingTime { get; private set; }

        private readonly Dictionary<ItemType, int> filledCounts = new Dictionary<ItemType, int>();

        public ActiveOrder(OrderRow row)
        {
            Row = row;
            RemainingTime = row.TimeLimit;
        }

        // 필요한 재료를 모두 채웠는지
        public bool IsComplete
        {
            get
            {
                foreach (ItemCount requireItem in Row.RequireItems)
                {
                    if (GetFilledCount(requireItem.ItemID) < requireItem.Count) return false;
                }
                return true;
            }
        }

        public int GetFilledCount(ItemType itemId)
        {
            return filledCounts.TryGetValue(itemId, out int filledCount) ? filledCount : 0;
        }

        // 이 재료가 아직 더 필요한지
        public bool NeedsItem(ItemType itemId)
        {
            foreach (ItemCount requireItem in Row.RequireItems)
            {
                if (requireItem.ItemID == itemId && GetFilledCount(itemId) < requireItem.Count) return true;
            }
            return false;
        }

        public void Fill(ItemType itemId)
        {
            filledCounts[itemId] = GetFilledCount(itemId) + 1;
        }

        public void Tick(float deltaTime)
        {
            RemainingTime -= deltaTime;
        }
    }

    // 주문 칸 규칙 (MonoBehaviour 없이 규칙만 가진다. 시간은 OrderManager가 넘겨 준다)
    // 1. 처음에 칸을 모두 채운다
    // 2. 제출한 재료는 그 재료가 필요한 주문 중 남은 시간이 가장 짧은 주문에 들어간다. 맞는 주문이 없으면 받지 않는다
    // 3. 주문이 끝나면(완료, 실패) 그 칸은 refillDelay 뒤 새 주문으로 채운다
    public class OrderBoard
    {
        public Action OrdersChanged;                // 주문이 생기거나, 채워지거나, 끝났을 때
        public Action<ActiveOrder> OrderCompleted;
        public Action<ActiveOrder> OrderFailed;

        private readonly IReadOnlyList<OrderRow> candidateRows;
        private readonly float refillDelay;
        private readonly Func<int, int> pickIndex;  // 후보 개수를 받아 0 ~ (개수 - 1) 중 하나를 고른다

        private readonly ActiveOrder[] orders;      // 빈 칸은 null
        private readonly float[] refillTimes;       // 빈 칸이 새 주문으로 채워지기까지 남은 시간

        // 주문 칸 목록. 빈 칸은 null
        public IReadOnlyList<ActiveOrder> Orders => orders;

        public OrderBoard(IReadOnlyList<OrderRow> candidateRows, int maxOrderCount, float refillDelay, Func<int, int> pickIndex)
        {
            this.candidateRows = candidateRows;
            this.refillDelay = refillDelay;
            this.pickIndex = pickIndex;

            orders = new ActiveOrder[maxOrderCount];
            refillTimes = new float[maxOrderCount];
            for (int slotIndex = 0; slotIndex < maxOrderCount; slotIndex++)
            {
                orders[slotIndex] = CreateOrder();
            }
        }

        // 빈 칸이 새 주문으로 채워지기까지 남은 시간 (HUD의 "새 주문 n초")
        public float GetRefillTime(int slotIndex)
        {
            return refillTimes[slotIndex];
        }

        public void Tick(float deltaTime)
        {
            bool isChanged = false;
            for (int slotIndex = 0; slotIndex < orders.Length; slotIndex++)
            {
                ActiveOrder order = orders[slotIndex];
                if (order == null)
                {
                    refillTimes[slotIndex] -= deltaTime;
                    if (refillTimes[slotIndex] <= 0f)
                    {
                        orders[slotIndex] = CreateOrder();
                        isChanged = true;
                    }
                    continue;
                }

                order.Tick(deltaTime);
                if (order.RemainingTime <= 0f)
                {
                    EmptySlot(slotIndex);
                    OrderFailed?.Invoke(order);
                    isChanged = true;
                }
            }
            if (isChanged) OrdersChanged?.Invoke();
        }

        // 재료 하나를 제출한다. 받았으면 true, 맞는 주문이 없으면 false
        public bool Submit(ItemType itemId)
        {
            int targetIndex = -1;
            for (int slotIndex = 0; slotIndex < orders.Length; slotIndex++)
            {
                ActiveOrder order = orders[slotIndex];
                if (order == null || order.NeedsItem(itemId) == false) continue;
                if (targetIndex == -1 || order.RemainingTime < orders[targetIndex].RemainingTime) targetIndex = slotIndex;
            }
            if (targetIndex == -1) return false;

            ActiveOrder targetOrder = orders[targetIndex];
            targetOrder.Fill(itemId);
            if (targetOrder.IsComplete)
            {
                EmptySlot(targetIndex);
                OrderCompleted?.Invoke(targetOrder);
            }
            OrdersChanged?.Invoke();
            return true;
        }

        private void EmptySlot(int slotIndex)
        {
            orders[slotIndex] = null;
            refillTimes[slotIndex] = refillDelay;
        }

        private ActiveOrder CreateOrder()
        {
            return new ActiveOrder(candidateRows[pickIndex(candidateRows.Count)]);
        }
    }
}
