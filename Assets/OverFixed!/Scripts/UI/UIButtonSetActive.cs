using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 버튼을 누르면 대상 오브젝트를 켜거나 끈다 (닫기, 다른 화면 열기)
    public class UIButtonSetActive : MonoBehaviour
    {
        [Serializable]
        public class Link
        {
            public Button Button;
            public GameObject Target;
            public bool IsActive;   // true = 켜기, false = 끄기
        }

        [SerializeField] private Link[] links;

        private void Awake()
        {
            foreach (Link link in links)
            {
                link.Button.onClick.AddListener(() => link.Target.SetActive(link.IsActive));
            }
        }
    }
}
