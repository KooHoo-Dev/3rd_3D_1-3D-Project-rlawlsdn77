using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 탭 전환. 탭 버튼을 누르면 같은 순서의 본문만 켜고, 그 탭의 밑줄과 글자색을 선택 표시로 바꾼다
    // tabButtons, tabBodies, tabUnderlines, tabLabels는 같은 순서로 넣는다
    public class UITabGroup : MonoBehaviour
    {
        [SerializeField] private Button[] tabButtons;
        [SerializeField] private GameObject[] tabBodies;
        [SerializeField] private GameObject[] tabUnderlines;
        [SerializeField] private TMP_Text[] tabLabels;

        [SerializeField] private Color selectedColor = new Color32(230, 236, 247, 255);
        [SerializeField] private Color normalColor = new Color32(138, 151, 179, 255);

        private void Awake()
        {
            for (int index = 0; index < tabButtons.Length; index++)
            {
                int tabIndex = index;
                tabButtons[index].onClick.AddListener(() => SelectTab(tabIndex));
            }
            SelectTab(0);
        }

        public void SelectTab(int tabIndex)
        {
            for (int index = 0; index < tabButtons.Length; index++)
            {
                bool isSelected = index == tabIndex;
                tabBodies[index].SetActive(isSelected);
                tabUnderlines[index].SetActive(isSelected);
                tabLabels[index].color = isSelected ? selectedColor : normalColor;
                tabLabels[index].fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }
}
