using TMPro;
using UnityEngine;

namespace OverFixed
{
    // HUD 위치 표시 하나. 월드의 대상 위에 "이름 거리m" 라벨과 아래 화살표를 띄우고, 화면 밖(뒤 포함)이면 가장자리에 붙여 화살표로 방향을 보인다
    // 대상은 Show로 매 프레임 정하고, 안 부르면 Hide로 끈다. 카메라가 LateUpdate에서 움직인 뒤에 그려야 떨리지 않아 실행 순서를 뒤로 둔다
    [DefaultExecutionOrder(100)]
    public class WorldMarkerUI : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private TMP_Text labelValue;
        [SerializeField] private RectTransform pill;
        [SerializeField] private RectTransform arrow;
        [SerializeField] private float heightOffset = 1.5f;                     // 대상 기준점(바닥) 위로 띄우는 높이 (m)
        [SerializeField] private Vector2 edgeMargin = new Vector2(110f, 80f);   // 화면 가장자리에서 띄우는 거리 (캔버스 단위)
        [SerializeField] private float edgeArrowGap = 14f;                      // 가장자리일 때 라벨과 화살표 사이

        private RectTransform markerRect;
        private RectTransform areaRect;     // 표시가 움직이는 영역 (부모, 화면 전체)
        private Vector3 targetPosition;
        private string targetLabel;

        // 화면 안일 때의 화살표 자리 (씬에 정해 둔 값)
        private Vector2 arrowPosition;
        private Quaternion arrowRotation;

        private void Awake()
        {
            markerRect = GetComponent<RectTransform>();
            areaRect = transform.parent.GetComponent<RectTransform>();
            arrowPosition = arrow.anchoredPosition;
            arrowRotation = arrow.localRotation;
        }

        public void Show(Vector3 position, string label)
        {
            targetPosition = position + Vector3.up * heightOffset;
            targetLabel = label;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            int distance = Mathf.RoundToInt(Vector3.Distance(playerCamera.transform.position, targetPosition));
            labelValue.text = targetLabel + "  " + distance + "m";

            // 카메라 뒤에 있으면 화면 좌표가 뒤집혀 나오므로 다시 뒤집어 방향을 맞춘다
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(targetPosition);
            bool isBehind = screenPoint.z < 0f;
            if (isBehind) screenPoint = new Vector3(Screen.width - screenPoint.x, Screen.height - screenPoint.y, 0f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(areaRect, screenPoint, null, out Vector2 localPoint);

            Rect area = areaRect.rect;
            Vector2 center = area.center;
            Vector2 halfSize = new Vector2(area.width * 0.5f - edgeMargin.x, area.height * 0.5f - edgeMargin.y);
            Vector2 offset = localPoint - center;
            bool isInside = isBehind == false && Mathf.Abs(offset.x) <= halfSize.x && Mathf.Abs(offset.y) <= halfSize.y;

            if (isInside)
            {
                markerRect.localPosition = localPoint;
                arrow.anchoredPosition = arrowPosition;
                arrow.localRotation = arrowRotation;
                return;
            }

            // 가운데에서 대상 쪽으로 그은 선이 가장자리 사각형에 닿는 곳에 붙인다
            if (isBehind && offset.sqrMagnitude < 1f) offset = Vector2.down;
            float scaleX = Mathf.Abs(offset.x) > 0.001f ? halfSize.x / Mathf.Abs(offset.x) : float.MaxValue;
            float scaleY = Mathf.Abs(offset.y) > 0.001f ? halfSize.y / Mathf.Abs(offset.y) : float.MaxValue;
            markerRect.localPosition = center + offset * Mathf.Min(scaleX, scaleY);

            // 화살표는 라벨 바깥쪽에서 대상 방향을 가리킨다 (화살표 그림은 위가 앞)
            Vector2 direction = offset.normalized;
            Vector2 pillCenter = (Vector2)pill.localPosition + pill.rect.center;
            Vector2 pillHalfSize = pill.rect.size * 0.5f;
            float arrowDistance = Mathf.Abs(direction.x) * pillHalfSize.x + Mathf.Abs(direction.y) * pillHalfSize.y + edgeArrowGap;
            arrow.anchoredPosition = pillCenter + direction * arrowDistance;
            arrow.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        }
    }
}
