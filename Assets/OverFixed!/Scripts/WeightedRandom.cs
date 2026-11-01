using UnityEngine;

namespace OverFixed
{
    // 비중에 비례해 하나를 뽑는다 (재료 종류, 패널 사고 종류)
    public static class WeightedRandom
    {
        // 비중 배열에서 하나를 골라 그 번호를 돌려준다
        // 예: {6, 3, 1.2}이면 0번이 나올 확률은 6 / 10.2
        public static int PickIndex(float[] weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Length; i++) total += weights[i];

            float pick = Random.Range(0f, total);
            for (int i = 0; i < weights.Length; i++)
            {
                if (pick < weights[i]) return i;
                pick -= weights[i];
            }
            return weights.Length - 1;
        }
    }
}
