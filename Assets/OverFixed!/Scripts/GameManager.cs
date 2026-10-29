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
    // 4. 게임오버와 항해 포기 때 최고 기록(RecordSave)을 갱신한다
    public class GameManager : SingletonBase<GameManager>
    {
        [Header("설정값 (새 게임의 시작 크레딧, 시작 안정도)")]
        [SerializeField] private CreditConfig creditConfig;
        [SerializeField] private CoreConfig coreConfig;

        [Header("씬 이름 (항해 씬이 열리면 항해를 시작한다)")]
        [SerializeField] private string voyageSceneName = "SPACE";
        [SerializeField] private string titleSceneName = "MainMenu";

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

        public RecordSave Record { get; private set; }      // 최고 기록
        public bool IsNewRecord { get; private set; }       // 이번 게임오버(항해 포기)에서 최고 기록을 갱신했는지

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
            Record = RecordSave.Load();
            StartNewGame();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // 크레딧과 안정도를 처음 값으로, 항해 번호를 1로 되돌린다
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

            // 항해 중에는 커서를 잠그고 숨긴다. 터미널, 정산, 게임오버, 일시정지 화면이 열리면 보인다
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

        // 안정도가 가득 찼는지. 화면 표기(올림 정수)가 상한이면 가득 찬 것으로 본다
        // (시간 감소로 99.98%처럼 100%로 보일 때 사서 크레딧만 잃는 것을 막는다)
        public bool IsStabilityFull => Mathf.CeilToInt(Stability) >= coreConfig.MaxStability;

        // 안정화 물질을 살 수 있는지. 크레딧이 가격 이상이고 안정도가 가득 차지 않아야 한다
        public bool CanBuyStabilizer => Credit >= coreConfig.StabilizerPrice && IsStabilityFull == false;

        // 안정화 물질 구매 (항해 중 터미널, PLANET 정비). 크레딧을 내고 안정도를 올린다 (상한을 넘지 않음). 못 사면 false
        public bool TryBuyStabilizer()
        {
            if (CanBuyStabilizer == false) return false;

            Credit -= coreConfig.StabilizerPrice;
            Stability = Mathf.Min(Stability + coreConfig.StabilizerRecovery, coreConfig.MaxStability);
            return true;
        }

        // 정산 화면의 다음 버튼. 도착 보상을 받고 다음 항해로 간다 (PLANET이 생기면 정비로 간다)
        // 도착 보상은 누적 크레딧에 넣고, 이번 항해에 번 크레딧(주문 보상)에는 넣지 않는다
        public void FinishSettlement()
        {
            int arrivalReward = Voyage.ArrivalReward;
            Credit += arrivalReward;
            TotalEarnedCredit += arrivalReward;
            StartNextVoyage();
        }

        // 항해 번호를 올리고 항해 씬을 다시 열어 다음 항해를 시작한다
        public void StartNextVoyage()
        {
            VoyageIndex++;
            SceneManager.LoadScene(voyageSceneName);
        }

        // 코어 모듈의 안정도가 0에 도달했을 때 Core가 부른다. 항해를 멈추고 최고 기록을 갱신한 뒤 게임오버 화면에 알린다
        public void GameOver()
        {
            IsPlaying = false;
            IsNewRecord = Record.TryUpdate(CompletedVoyageCount, TotalEarnedCredit);
            GameOverHappened?.Invoke();
        }

        // 타이틀의 출항 시작, 게임오버 화면의 다시 시작. 새 게임 값으로 항해 씬을 연다
        public void StartGame()
        {
            StartNewGame();
            SceneManager.LoadScene(voyageSceneName);
        }

        // 일시정지 화면의 항해 포기. 게임오버처럼 최고 기록을 갱신하고 타이틀로 간다
        public void AbandonVoyage()
        {
            IsNewRecord = Record.TryUpdate(CompletedVoyageCount, TotalEarnedCredit);
            GoToTitle();
        }

        // 게임오버 화면의 타이틀로
        public void GoToTitle()
        {
            IsPlaying = false;
            SceneManager.LoadScene(titleSceneName);
        }
    }
}
