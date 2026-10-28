using System;
using Jay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OverFixed
{
    // 게임 처리를 담당하는 매니저. 씬이 바뀌어도 살아남는다
    // 1. 크레딧, 코어 안정도, 항해 번호처럼 씬을 넘어 이어지는 값을 갖는다. 값을 바꾸는 창구는 AccountManager, Core다
    // 2. 항해 씬이 열리면 항해를 시작하고, 항해 시간(VoyageTable)이 지나면 항해를 끝낸다
    // 3. 씬 오브젝트는 필드로 갖지 않는다 (씬이 바뀌면 끊긴다). 씬 쪽이 GameManager를 읽고 이벤트를 구독한다
    public class GameManager : SingletonBase<GameManager>
    {
        [Header("설정값 (새 게임의 시작 크레딧, 시작 안정도)")]
        [SerializeField] private CreditConfig creditConfig;
        [SerializeField] private CoreConfig coreConfig;

        [Header("이 씬이 열리면 항해를 시작한다")]
        [SerializeField] private string voyageSceneName = "SPACE";

        public int Credit { get; set; }                 // 보유 크레딧. AccountManager가 바꾼다
        public float Stability { get; set; }            // 코어 안정도 (%). Core가 바꾼다
        public int VoyageIndex { get; private set; }    // 지금 항해 번호 (1부터)
        public int TotalEarnedCredit { get; private set; }  // 게임 전체에서 번 크레딧 (게임오버 기록)

        // 게임오버 때 완료한 항해 수 (지금 항해는 끝내지 못했으므로 뺀다)
        public int CompletedVoyageCount => VoyageIndex - 1;

        // 이번 항해의 기록 (정산 화면)
        public int VoyageEarnedCredit { get; private set; }
        public int OrderSuccessCount { get; private set; }
        public int OrderFailCount { get; private set; }

        public bool IsPlaying { get; private set; } = false;
        public float RemainingTime { get; private set; }    // 도착까지 남은 시간 (초)

        // 지금 항해의 VoyageTable 행 (항해 시간, 안정도 감소량)
        public VoyageRow Voyage => DataManager.GetVoyage(VoyageIndex);

        // 항해 시간이 다 되어 도착했을 때. 정산 화면이 구독한다
        public Action VoyageEnded;

        // 코어 안정도가 0이 되었을 때. 게임오버 화면이 구독한다
        public Action GameOverHappened;

        // 다른 스크립트가 이 Awake보다 먼저 Instance를 부르면 SingletonBase가 DontDestroyOnLoad를 건너뛴다.
        // 그래도 씬을 넘어 살아남도록 여기서 한 번 더 건다 (두 번 불려도 문제없음)
        protected override void Awake()
        {
            base.Awake();
            if (instance == this) DontDestroyOnLoad(gameObject);
        }

        protected override void OnInitialize()
        {
            StartNewGame();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // 크레딧과 안정도를 처음 값으로, 항해 번호를 1로 되돌린다. MainMenu의 게임 시작이 생기면 거기서도 부른다
        public void StartNewGame()
        {
            Credit = creditConfig.StartCredit;
            Stability = coreConfig.StartStability;
            VoyageIndex = 1;
            TotalEarnedCredit = 0;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == voyageSceneName) StartVoyage();
        }

        private void StartVoyage()
        {
            VoyageEarnedCredit = 0;
            OrderSuccessCount = 0;
            OrderFailCount = 0;
            RemainingTime = Voyage.VoyageTime;
            IsPlaying = true;
        }

        private void Update()
        {
            if (IsPlaying == false) return;

            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                IsPlaying = false;
                VoyageEnded?.Invoke();
            }
        }

        // 주문 결과 기록. OrderManager가 부른다
        public void RecordOrderSuccess(int reward)
        {
            OrderSuccessCount++;
            VoyageEarnedCredit += reward;
            TotalEarnedCredit += reward;
        }

        public void RecordOrderFail()
        {
            OrderFailCount++;
        }

        // 정산 화면의 다음 버튼. PLANET이 생기기 전까지는 항해 씬을 다시 열어 다음 항해를 시작한다
        public void StartNextVoyage()
        {
            VoyageIndex++;
            SceneManager.LoadScene(voyageSceneName);
        }

        // 코어 모듈의 안정도가 0에 도달했을 때 Core가 부른다. 항해를 멈추고 게임오버 화면에 알린다
        public void GameOver()
        {
            IsPlaying = false;
            GameOverHappened?.Invoke();
        }

        // 게임오버 화면의 다시 시작. 새 게임 값으로 항해 씬을 다시 연다 (MainMenu가 생기면 타이틀로 가는 버튼도 쓴다)
        public void RestartGame()
        {
            StartNewGame();
            SceneManager.LoadScene(voyageSceneName);
        }
    }
}
