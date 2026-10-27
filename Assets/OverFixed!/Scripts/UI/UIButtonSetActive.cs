using System;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // 버튼을 누르면 대상 오브젝트를 켜거나 끈다 (닫기, 계속하기, 다른 화면 열기)
    // 다른 프리팹의 화면은 씬에 배치한 뒤 인스펙터에서 links에 추가해 연결한다
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
                Link current = link;
                current.Button.onClick.AddListener(() => current.Target.SetActive(current.IsActive));
            }
        }
        
        
    }
}
