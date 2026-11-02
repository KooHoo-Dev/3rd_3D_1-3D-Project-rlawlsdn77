using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace OverFixed
{
    public class PlanetEvent
    {
        public int EventId;
        public string EventName;
    }
    
    public class EnvEvent
    {
        public int EventId;
        public string EventName;
    }

    public class DebugEvent
    {
        public PlanetEvent[] PlanetEvent;
        public EnvEvent[] EnvEvent;
        
        public void SetPlanetEvent()
        {
            for (int i = 0; i < PlanetEvent.Length; i++)
            {
                PlanetEvent[i].EventId = i;
            }
        }

        public void SetEnvEvent()
        {
            for (int i = 0; i < EnvEvent.Length; i++)
            {
                EnvEvent[i].EventId = i;
            }
        }
    }
    
    public class ReadyToSpaceUI : MonoBehaviour
    {
        // 이 UI는 게임 시작 버튼 누름 or 행성 완주 후 maintenanceUI가 나온 후 나온다.
        // 이 UI에는 행성이 3개 나온다. (행성은 각 행성마다 특수한 환경을 가져 특수 효과가 있다.)
        // 또한 하나의 행성에 3개의 특별 이벤트가 포함되어 있다.
        // 새로고침을 통해 이벤트 리셋 가능

        [Header("행성 이름 목록들 (행성은 3개 보여준다)")]
        public TMP_Text Planet1_name;
        public TMP_Text Planet2_name;
        public TMP_Text Planet3_name;

        [Header("행성 이름에 대응되는 행성 이벤트 목록 목록")] 
        public TMP_Text Planet1_PlanetEvent;
        public TMP_Text Planet2_PlanetEvent;
        public TMP_Text Planet3_PlanetEvent;

        [Header("각 행성에 랜덤하게 3개씩 붙은 환경 이벤트 목록")]
        public TMP_Text[] Planet1_EnvEvent;
        public TMP_Text[] Planet2_EnvEvent;
        public TMP_Text[] Planet3_EnvEvent;

        [Header("환경 이벤트 목록 개수 (디폴트는 3개)")]
        [SerializeField] private int EnvCount = 3;

        [Header("행성 새로고침 버튼")] 
        [SerializeField] public Button RefreshPlanetButton;
        
        [Header("환경 새로고침 버튼")]
        [SerializeField] public Button RefreshEnvButton;
        
        // 행성 개수
        private int PlanetCount = 3;
        
        // 새로고침 횟수
        public int RefreshPlanetCount = 1;
        public int RefreshEnvCount = 1;
        
        private List<PlanetTypeRow> planetTypeTable;
        private List<ZoneRow> zoneTable;
        
        private void Awake()
        {
            // 환경 이벤트 목록 개수 설정
            Planet1_EnvEvent = new TMP_Text[EnvCount];
            Planet2_EnvEvent = new TMP_Text[EnvCount];
            Planet3_EnvEvent = new TMP_Text[EnvCount];
            
            RefreshPlanetButton.onClick.AddListener(RefreshPlanet);
        }

        public void RefreshPlanet()
        {
            // 행성 새로고침은 새로고침 전에 있던 행성이 재등장 할 수 있다.
            // 행성 새로고침과 환경 새로고침의 제공 횟수는 동일하다.

            // 횟수가 남았다면 행동함
            if (RefreshPlanetCount > 0)
            {
                // 일단 먼저 카운트 감소
                RefreshPlanetCount--;
                IReadOnlyList<PlanetTypeRow> planetTypes = DataManager.GetPlanetTypeRows();
                
                // Enum PlanetType의 원소 개수 확인  
                int planetTypeCount = Enum.GetValues(typeof(PlanetType)).Length;

                HashSet<int> planetTypeSet = new HashSet<int>();

                // planetTypeSet 해시셋 크기가 행성 수보다 작다면 계속해서 반복문을 진행함
                while (planetTypeSet.Count < PlanetCount)
                {
                    planetTypeSet.Add(Random.Range(0, planetTypeCount));
                }
                
            }
        }
    }
}