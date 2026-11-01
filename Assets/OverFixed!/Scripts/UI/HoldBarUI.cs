using UnityEngine;

namespace OverFixed
{
    // HUD 홀드 막대 (에어락 통과). 진행도만큼 Fill의 anchorMax.x를 늘리고, 0이면 숨긴다
    public class HoldBarUI : MonoBehaviour
    {
        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private GameObject holdBar;
        [SerializeField] private RectTransform fill;

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
