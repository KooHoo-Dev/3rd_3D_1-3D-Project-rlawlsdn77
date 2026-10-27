using UnityEngine;

namespace OverFixed
{
    // HUD 홀드 진행 막대. LocalPlayer가 알려 주는 진행도(0 ~ 1)만큼 채우고, 진행도가 0이면 숨긴다
    // 채움은 Fill의 anchorMax.x로 나타낸다 (Fill은 막대 왼쪽 끝에 붙어 anchorMax.x 비율만큼 늘어난다)
    public class HoldBarUI : MonoBehaviour
    {
        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private GameObject holdBar;     // 켜고 끄는 막대 전체
        [SerializeField] private RectTransform fill;     // 진행도만큼 늘어나는 채움

        private void OnEnable()
        {
            localPlayer.HoldProgressChanged += ShowProgress;
        }

        private void OnDisable()
        {
            localPlayer.HoldProgressChanged -= ShowProgress;
        }

        private void ShowProgress(float progress)
        {
            holdBar.SetActive(progress > 0f);
            fill.anchorMax = new Vector2(progress, fill.anchorMax.y);
        }
    }
}
