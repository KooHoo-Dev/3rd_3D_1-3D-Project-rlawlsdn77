using System;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace OverFixed
{
    public class Panel : MonoBehaviour
    {
        // Panel 사건은 평균적으로 40초에 1번씩 발생하지만,
        // 각 행성의 특징 or 이벤트에 영향을 받아 더 빠르게 / 늦게 발생할 수 있다. (이거는 가중치로 처리한다.)
        
        // ex.) Panel 사건 해결 요구시간이 30초라고 가정했을 때,
        // Panel 사건 발생 후 요구시간이 10초 남았을 때 부터 해당 사건이 발생한 모듈의 내구도가 점진적으로 감소한다.
        // 요구 시간을 초과했을 시 코어 안정도가 감소하고 내구도가 더 빨리 감소하기 시작한다.
        // 요구 시간 안에 패널 수리 성공 시 해당 모듈의 내구도가 상승한다.
        
        [SerializeField] private float accidentTick = 40f;
        [SerializeField] private float accidentTimer = 0f;

        [SerializeField] private float clearAccidentTimer = 0f;
        [SerializeField] private float clearAccidentTick = 30f;
        
        [Header("미니게임 패널 화면")]
        [SerializeField] private GameObject panel;

        [Header("미니게임 목록들")] 
        [SerializeField] private MiniGame[] MiniGames;

        public bool IsMiniGaming { get; private set; }
        
        private void Start()
        {
            panel.SetActive(false);
            IsMiniGaming = false;
        }
        
        private void Update()
        {
            accidentTimer += Time.deltaTime;

            if (accidentTimer > accidentTick)
            {
                accidentTimer = 0f;
                FixPanel();
            }
        }

        // LocalPlayer에서 호출하는 패널 고치기 로직
        public void FixPanel()
        {
            // 패널 활성화 및 미니게임 시작
            panel.SetActive(true);
            IsMiniGaming = true;

            Random.Range(0, MiniGames.Length);
        }

        public void MiniGame_0()
        {
            MiniGame MiniGame_0 = MiniGames[0];            
        }

        public void MiniGame_1()
        {
            MiniGame MiniGame_1 = MiniGames[1];
        }

        public void MiniGame_2()
        {
            MiniGame MiniGame_2 = MiniGames[2];
        }
    }

    public class MiniGame
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public float LimitTime { get; private set; }
        public int Reward { get; private set; }

        public MiniGame(int ID, string Name, string Description, float LimitTime, int Reward)
        {
            this.Id = ID;
            this.Name = Name;
            this.Description = Description;
            this.LimitTime = LimitTime;
            this.Reward = Reward;
        }
    }

    public class MiniGame_0
    {
        public int Id = 0;
        public string Name = "패널 수리";
        public string Description = "중앙의 영역 안에 좌우로 움직이는 토글이 들어올 때 SPACE 바를 누르면 통과, 3회 연속 성공시 완료.";
        public float LimitTime = 30f;
        public int Reward = 30;
    }
}