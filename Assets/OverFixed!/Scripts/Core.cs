using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OverFixed
{
    public class Core : MonoBehaviour
    {
        // 1. 코어는 시간이 지날수록 점진적으로 안정도가 감소한다
        // 2. 오더의 제한시간 안에 납부 여부에 따라 코어 안정도가 증감한다.
        // 3. 안정도는 0 ~ MaxStability 사이에 머물고, 항해 중에 0이 되면 게임오버

        [Header("설정값 (CoreConfig로 회복량, 감소량, 상한 조절. 시간 감소량은 VoyageTable의 BaseDecay)")]
        [SerializeField] private CoreConfig coreConfig;

        // 현재 안정도 (%). 씬이 바뀌어도 이어지도록 값은 GameManager가 갖는다 (시작 안정도도 GameManager가 넣는다)
        public float CurrentStability => GameManager.Instance.Stability;

        // 오더 성공 시 오더 매니저에서 호출당함
        public void PlusStability()
        {
            ChangeStability(coreConfig.OrderSuccessRecovery);
        }

        // 오더 실패(제한시간 초과) 시 오더 매니저에서 호출당함
        public void MinusStability()
        {
            ChangeStability(-coreConfig.OrderFailPenalty);
        }

        private void Update()
        {
            // 항해 중에만 안정도가 점진적으로 감소한다 (초당 BaseDecay 만큼)
            if (GameManager.Instance.IsPlaying) ChangeStability(-GameManager.Instance.Voyage.BaseDecay * Time.deltaTime);

#if UNITY_EDITOR
            Debug_AddStability();
#endif
        }

        // 안정도를 바꾸고 0 ~ MaxStability로 자른다. 항해 중에 0이 되면 게임오버
        private void ChangeStability(float amount)
        {
            GameManager gameManager = GameManager.Instance;
            gameManager.Stability = Mathf.Clamp(gameManager.Stability + amount, 0f, coreConfig.MaxStability);
            if (gameManager.IsPlaying && gameManager.Stability <= 0f) gameManager.GameOver();
        }

        #region 디버깅 코드 (에디터 전용)

#if UNITY_EDITOR
        private void Debug_AddStability()
        {
            // 0 누르면 주문 성공만큼 안정도를 올린다
            if (SimpleInput.GetKeyDown(Key.Digit0)) PlusStability();
        }
#endif

        #endregion
    }
}
