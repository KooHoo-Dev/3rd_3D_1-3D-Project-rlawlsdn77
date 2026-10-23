using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OverFixed
{
    // 툴바 OverFixed > UI 생성
    // 웹 프로토타입 화면(07_ui.js, style.css)과 같은 모양의 UI를 화면별 프리팹(Prefabs/UI)으로 만든다.
    // 루트는 Canvas, 그 아래 {화면}Panel. HUD만 켜 두고 나머지 화면 패널은 꺼 둔다.
    // 화면 안의 버튼 전환(탭, 닫기, 계속하기)은 UITabGroup, UIButtonSetActive를 붙여 연결해 둔다 (실행 시 onClick.AddListener).
    // 값이 들어갈 텍스트는 ...Value, 버튼은 ...Button 이름으로 만든다.
    public static class GenerateUI
    {
        private const string PREFAB_FOLDER = "Assets/OverFixed!/Prefabs/UI";
        private const string FONT_PATH = "Assets/99. External Assets/Font/Noto_Sans_KR/static/NotoSansKR-Regular SDF.asset";

        // 웹 style.css 색
        private static readonly Color BackdropColor = new Color32(3, 6, 14, 173);       // .screen 배경
        private static readonly Color PanelColor = new Color32(14, 21, 40, 255);        // --panel-solid
        private static readonly Color CardColor = new Color32(11, 18, 34, 255);         // .planet-card
        private static readonly Color HudPanelColor = new Color32(8, 13, 26, 184);      // .hud-panel
        private static readonly Color StripColor = new Color32(255, 255, 255, 8);       // .info-strip, .shop-item
        private static readonly Color BarColor = new Color32(255, 255, 255, 20);        // .bar
        private static readonly Color FrameLineColor = new Color32(79, 216, 232, 89);   // .terminal-frame 테두리
        private static readonly Color LineColor = new Color32(120, 160, 220, 56);       // --line
        private static readonly Color RowLineColor = new Color32(120, 160, 220, 20);    // .data-table td 아래선
        private static readonly Color TextColor = new Color32(230, 236, 247, 255);      // --text
        private static readonly Color MutedColor = new Color32(138, 151, 179, 255);     // --muted
        private static readonly Color AccentColor = new Color32(255, 181, 71, 255);     // --accent
        private static readonly Color CyanColor = new Color32(79, 216, 232, 255);       // --cyan
        private static readonly Color GoodColor = new Color32(91, 227, 138, 255);       // --good
        private static readonly Color BadColor = new Color32(255, 90, 90, 255);         // --bad
        private static readonly Color ButtonColor = new Color32(26, 36, 64, 255);       // .btn
        private static readonly Color DangerColor = new Color32(74, 24, 32, 255);       // .btn.danger
        private static readonly Color PrimaryTextColor = new Color32(27, 18, 0, 255);   // .btn.primary 글자
        private static readonly Color KeyColor = new Color32(230, 236, 247, 255);       // .key 배경
        private static readonly Color KeyTextColor = new Color32(10, 15, 29, 255);      // .key 글자
        private static readonly Color CentralCellColor = new Color32(45, 90, 140, 255); // .grid-cell.central
        private static readonly Color ModuleCellColor = new Color32(52, 74, 120, 255);  // .grid-cell.module
        private static readonly Color ClearColor = new Color(0f, 0f, 0f, 0f);

        private enum ButtonStyle
        {
            Normal,     // .btn
            Small,      // .btn.small
            Primary,    // .btn.primary
            Danger,     // .btn.danger
            Big,        // .btn.primary.big
        }

        // 버튼 → 대상 켜기/끄기. 화면을 다 만든 뒤 루트의 UIButtonSetActive에 넣는다
        private class LinkSpec
        {
            public Button Button;
            public GameObject Target;
            public bool IsActive;
        }

        private static readonly List<LinkSpec> linkList = new List<LinkSpec>();
        private static TMP_FontAsset font;
        private static Sprite roundSprite;
        private static Sprite knobSprite;
        private static Sprite backgroundSprite;
        private static Sprite checkmarkSprite;

        // ---------------- 메뉴 ----------------

        [MenuItem("OverFixed/UI 생성/모든 화면", false, 0)]
        public static void GenerateAll()
        {
            GenerateHud();
            GenerateTerminal();
            GenerateTitle();
            GeneratePause();
            GenerateOptions();
            GenerateHelp();
            GeneratePrepare();
            GenerateStow();
            GenerateSettlement();
            GenerateMaintenance();
            GenerateGameOver();
        }

        [MenuItem("OverFixed/UI 생성/HUD", false, 20)] public static void GenerateHud() => Generate("HudUI", 10, BuildHud);
        [MenuItem("OverFixed/UI 생성/터미널", false, 21)] public static void GenerateTerminal() => Generate("TerminalUI", 20, BuildTerminal);
        [MenuItem("OverFixed/UI 생성/타이틀", false, 40)] public static void GenerateTitle() => Generate("TitleUI", 20, BuildTitle);
        [MenuItem("OverFixed/UI 생성/일시정지", false, 41)] public static void GeneratePause() => Generate("PauseUI", 30, BuildPause);
        [MenuItem("OverFixed/UI 생성/옵션", false, 42)] public static void GenerateOptions() => Generate("OptionsUI", 40, BuildOptions);
        [MenuItem("OverFixed/UI 생성/도움말", false, 43)] public static void GenerateHelp() => Generate("HelpUI", 40, BuildHelp);
        [MenuItem("OverFixed/UI 생성/출항 준비", false, 60)] public static void GeneratePrepare() => Generate("PrepareUI", 20, BuildPrepare);
        [MenuItem("OverFixed/UI 생성/격납", false, 61)] public static void GenerateStow() => Generate("StowUI", 20, BuildStow);
        [MenuItem("OverFixed/UI 생성/정산", false, 62)] public static void GenerateSettlement() => Generate("SettlementUI", 20, BuildSettlement);
        [MenuItem("OverFixed/UI 생성/정비", false, 63)] public static void GenerateMaintenance() => Generate("MaintenanceUI", 20, BuildMaintenance);
        [MenuItem("OverFixed/UI 생성/게임오버", false, 64)] public static void GenerateGameOver() => Generate("GameOverUI", 20, BuildGameOver);

        // 미리보기 씬에서 만들어 프리팹으로 저장한 뒤 버린다 (열린 씬은 건드리지 않음)
        private static void Generate(string prefabName, int sortingOrder, System.Action<GameObject> build)
        {
            string prefabPath = PREFAB_FOLDER + "/" + prefabName + ".prefab";
            bool isExisting = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;
            if (isExisting && EditorUtility.DisplayDialog("UI 생성", prefabName + ".prefab이 이미 있습니다. 덮어쓸까요?\n씬에서 이 프리팹 안의 오브젝트를 참조하고 있다면 연결이 끊어질 수 있습니다.", "덮어쓰기", "건너뛰기") == false) return;

            LoadResources();
            if (AssetDatabase.IsValidFolder(PREFAB_FOLDER) == false) AssetDatabase.CreateFolder("Assets/OverFixed!/Prefabs", "UI");

            Scene previewScene = EditorSceneManager.NewPreviewScene();
            GameObject root = CreateCanvas(prefabName, sortingOrder, previewScene);
            linkList.Clear();
            build(root);
            AttachLinks(root);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            EditorSceneManager.ClosePreviewScene(previewScene);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        private static void LoadResources()
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
            roundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            backgroundSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            checkmarkSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        }

        // ---------------- HUD (항해 중) ----------------

        private static void BuildHud(GameObject root)
        {
            RectTransform panel = CreateRect("HudPanel", root.transform);
            Stretch(panel);

            // 왼쪽 위: 코어 안정도, 크레딧, 할당량
            RectTransform status = CreateHudPanel("Status", panel, new Vector2(0f, 1f), new Vector2(16f, -16f), 250f);
            RectTransform stabilityRow = CreateRow("StabilityRow", status, 0f, TextAnchor.MiddleLeft);
            CreateText("StabilityLabel", stabilityRow, "코어 안정도", 11f, MutedColor, false);
            AddFlexibleSpace(stabilityRow);
            CreateText("StabilityValue", stabilityRow, "100%", 20f, TextColor, true);
            CreateBar("StabilityBar", status, 10f, CyanColor, 1f);
            RectTransform statusGrid = CreateRow("StatusGrid", status, 6f, TextAnchor.UpperLeft);
            statusGrid.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;
            CreateStatusCell(statusGrid, "Credit", "크레딧", "0", AccentColor);
            CreateStatusCell(statusGrid, "Quota", "할당량", "0 / 200", TextColor);
            CreateBar("QuotaBar", status, 5f, AccentColor, 0f);

            // 왼쪽: 주문 카드 3개
            RectTransform orders = CreateRect("Orders", panel);
            Place(orders, new Vector2(0f, 1f), new Vector2(16f, -158f), new Vector2(250f, 0f));
            AddVertical(orders, 6f, new RectOffset(0, 0, 0, 0), true);
            orders.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            CreateText("Title", orders, "주문", 11f, MutedColor, false);
            for (int index = 1; index <= 3; index++)
            {
                RectTransform card = CreateImage("OrderCard_" + index, orders, HudPanelColor, true);
                card.gameObject.AddComponent<Outline>().effectColor = LineColor;
                AddVertical(card, 4f, new RectOffset(10, 10, 8, 8), true);
                RectTransform head = CreateRow("Head", card, 0f, TextAnchor.MiddleLeft);
                CreateText("RewardValue", head, "30 CR", 13f, AccentColor, true);
                AddFlexibleSpace(head);
                CreateText("TimeValue", head, "60s", 13f, TextColor, false);
                CreateText("ItemsValue", card, "고철 0/1", 12f, TextColor, false);
                CreateBar("TimeBar", card, 5f, CyanColor, 1f);
            }

            // 위 가운데: 경로 막대
            RectTransform route = CreateHudPanel("Route", panel, new Vector2(0.5f, 1f), new Vector2(0f, -16f), 620f);
            RectTransform routeHead = CreateRow("Head", route, 0f, TextAnchor.MiddleLeft);
            CreateText("ZoneValue", routeHead, "일반 구간", 13f, TextColor, false);
            AddFlexibleSpace(routeHead);
            CreateText("TimeValue", routeHead, "도착까지 0:00", 13f, AccentColor, false);
            CreateBar("RouteBar", route, 22f, ClearColor, 0f);

            // 오른쪽 위: 함선 지도, 오른쪽 아래: 레이더 (웹은 canvas 그림. 자리만 둔다)
            RectTransform shipMap = CreateHudPanel("ShipMap", panel, new Vector2(1f, 1f), new Vector2(-16f, -16f), 182f);
            CreateText("Label", shipMap, "함선 지도", 11f, MutedColor, false);
            CreateRect("MapArea", shipMap).gameObject.AddComponent<LayoutElement>().preferredHeight = 150f;
            RectTransform radar = CreateHudPanel("Radar", panel, new Vector2(1f, 0f), new Vector2(-16f, 16f), 192f);
            CreateRect("RadarArea", radar).gameObject.AddComponent<LayoutElement>().preferredHeight = 170f;
            CreateText("NoteValue", radar, "레이더", 11f, MutedColor, false).alignment = TextAlignmentOptions.Center;

            // 왼쪽 아래: 손
            RectTransform hands = CreateHudPanel("Hands", panel, new Vector2(0f, 0f), new Vector2(16f, 16f), 180f);
            CreateText("TitleValue", hands, "손 0/1", 11f, MutedColor, false);
            CreateText("HandsValue", hands, "빈손", 13f, MutedColor, false);

            // 왼쪽 아래 위: 튜토리얼 (처음에는 꺼짐)
            RectTransform tutorial = CreateHudPanel("Tutorial", panel, new Vector2(0f, 0f), new Vector2(16f, 96f), 290f);
            CreateText("Title", tutorial, "튜토리얼", 11f, MutedColor, false);
            CreateText("StepValue", tutorial, "선외로 나가 재료를 주워 오세요", 13f, TextColor, false, true);
            tutorial.gameObject.SetActive(false);

            // 가운데: 조준점
            RectTransform crosshair = CreateImage("Crosshair", panel, new Color(1f, 1f, 1f, 0.7f), false);
            crosshair.GetComponent<Image>().sprite = knobSprite;
            Place(crosshair, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f));

            // 아래 가운데: E 안내와 홀드 막대 (대상이 있을 때만 켬)
            RectTransform prompt = CreateImage("Prompt", panel, new Color32(8, 13, 26, 224), true);
            prompt.gameObject.AddComponent<Outline>().effectColor = new Color32(255, 181, 71, 153);
            Place(prompt, new Vector2(0.5f, 0f), new Vector2(0f, 120f), Vector2.zero);
            AddHorizontal(prompt, 8f, new RectOffset(16, 16, 9, 9), TextAnchor.MiddleCenter);
            prompt.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            prompt.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            CreateKey("Key", prompt, "E");
            CreateText("LabelValue", prompt, "줍기", 15f, TextColor, false);
            prompt.gameObject.SetActive(false);

            RectTransform hold = CreateBar("HoldBar", panel, 8f, AccentColor, 0f);
            Place(hold, new Vector2(0.5f, 0f), new Vector2(0f, 104f), new Vector2(220f, 8f));
            hold.gameObject.SetActive(false);

            // HUD는 클릭을 막지 않는다 (웹 pointer-events: none)
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
        }

        // ---------------- 터미널 ----------------

        private static void BuildTerminal(GameObject root)
        {
            RectTransform frame = CreateModal(root, "TerminalPanel", 1120f, true, out GameObject panel);
            Button closeButton = CreateHeader(frame, "터미널", "TimeValue", "항해는 계속 진행 중 · 도착까지 0:00", "닫기 (Esc)");
            AddLink(closeButton, panel, false);

            RectTransform status = CreateRow("StatusStrip", frame, 22f, TextAnchor.MiddleLeft);
            CreateLabelValue(status, "Credit", "크레딧", "0", AccentColor);
            CreateLabelValue(status, "Quota", "할당량", "0 / 0", TextColor);
            CreateLabelValue(status, "Stability", "코어 안정도", "100.0%", TextColor);

            RectTransform[] bodies = CreateTabs(root, frame, new[] { "Orders", "Quota", "Stabilizer", "ModuleFrame" }, new[] { "주문 보드", "할당량 납부", "안정화 물질", "모듈 상태" });

            float[] orderWeights = { 3f, 1f, 1f };
            CreateTableRow("HeaderRow", bodies[0], new[] { "필요한 재료", "남은 시간", "보상" }, orderWeights, MutedColor, LineColor);
            for (int index = 1; index <= 3; index++)
            {
                CreateTableRow("OrderRow_" + index, bodies[0], new[] { "고철 0/1", "60초", "30 크레딧" }, orderWeights, TextColor, RowLineColor);
            }
            CreateText("Note", bodies[0], "재료를 들고 터미널이나 납품소에서 E를 누르면 마감이 가장 급한 주문에 들어갑니다.", 12f, MutedColor, false);

            RectTransform quotaInfo = CreateRow("Info", bodies[1], 6f, TextAnchor.MiddleLeft);
            CreateText("RemainingLabel", quotaInfo, "남은 할당량", 14f, TextColor, false);
            CreateText("RemainingValue", quotaInfo, "200", 14f, TextColor, true);
            CreateText("CreditLabel", quotaInfo, "· 보유 크레딧", 14f, TextColor, false);
            CreateText("CreditValue", quotaInfo, "0", 14f, AccentColor, true);
            RectTransform payRow = CreateRow("PayRow", bodies[1], 6f, TextAnchor.MiddleLeft);
            CreateButton("Minus50Button", payRow, "-50", ButtonStyle.Small);
            CreateButton("Minus10Button", payRow, "-10", ButtonStyle.Small);
            TextMeshProUGUI payAmount = CreateText("PayAmountValue", payRow, "0", 26f, AccentColor, true);
            payAmount.alignment = TextAlignmentOptions.Center;
            payAmount.gameObject.AddComponent<LayoutElement>().minWidth = 90f;
            CreateButton("Plus10Button", payRow, "+10", ButtonStyle.Small);
            CreateButton("Plus50Button", payRow, "+50", ButtonStyle.Small);
            CreateButton("MaxButton", payRow, "최대", ButtonStyle.Small);
            CreateButton("PayButton", bodies[1], "0 크레딧 납부", ButtonStyle.Primary);
            CreateText("Note", bodies[1], "납부한 크레딧은 돌려받지 못합니다. 같은 크레딧이 안정화 물질과 정비 자금이 됩니다.", 12f, MutedColor, false);

            RectTransform recovery = CreateRow("Info", bodies[2], 6f, TextAnchor.MiddleLeft);
            CreateText("RecoveryLabel", recovery, "안정화 물질 · 즉시 코어 안정도", 14f, TextColor, false);
            CreateText("RecoveryValue", recovery, "+10.0%", 14f, TextColor, true);
            RectTransform price = CreateRow("Price", bodies[2], 6f, TextAnchor.MiddleLeft);
            CreateText("PriceLabel", price, "가격", 14f, TextColor, false);
            CreateText("PriceValue", price, "60", 14f, TextColor, true);
            CreateText("PriceUnit", price, "크레딧", 14f, TextColor, false);
            CreateButton("BuyButton", bodies[2], "구매", ButtonStyle.Primary);

            float[] moduleWeights = { 2f, 1f, 1f, 2f };
            CreateTableRow("HeaderRow", bodies[3], new[] { "모듈", "위치", "내구도", "상태" }, moduleWeights, MutedColor, LineColor);
            CreateTableRow("ModuleRow_1", bodies[3], new[] { "보조 에어락", "앞", "100%", "정상" }, moduleWeights, TextColor, RowLineColor);
            CreateTableRow("ModuleRow_2", bodies[3], new[] { "납품소", "우현", "100%", "정상" }, moduleWeights, TextColor, RowLineColor);

            panel.SetActive(false);
        }

        // ---------------- 메뉴 화면 ----------------

        private static void BuildTitle(GameObject root)
        {
            RectTransform panel = CreateRect("TitlePanel", root.transform);
            Stretch(panel);
            Stretch(CreateImage("Backdrop", panel, BackdropColor, false));

            RectTransform column = CreateRect("Column", panel);
            Place(column, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 0f));
            AddVertical(column, 10f, new RectOffset(0, 0, 0, 0), false);
            column.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            column.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateText("Logo", column, "Overfixed<color=#FFB547>!</color>", 96f, Color.white, true);
            CreateText("Subtitle", column, "행성 사이를 가로지르는 우주 물류 아케이드", 16f, MutedColor, false);

            RectTransform record = CreateImage("RecordBox", column, new Color32(10, 16, 32, 179), true);
            record.gameObject.AddComponent<Outline>().effectColor = LineColor;
            AddVertical(record, 2f, new RectOffset(22, 22, 12, 12), false);
            record.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            CreateText("RecordLabel", record, "최고 기록", 13f, MutedColor, false);
            CreateText("RecordValue", record, "항해 0회", 22f, TextColor, true);
            CreateText("TotalCreditValue", record, "누적 크레딧 0", 13f, MutedColor, false);

            CreateMenuButton("StartButton", column, "출항 시작", ButtonStyle.Big);
            CreateMenuButton("HelpButton", column, "조작법 · 규칙", ButtonStyle.Normal);
            CreateMenuButton("OptionsButton", column, "옵션", ButtonStyle.Normal);

            panel.gameObject.SetActive(false);
        }

        private static void BuildPause(GameObject root)
        {
            RectTransform frame = CreateModal(root, "PausePanel", 460f, false, out GameObject panel);
            CreateHeader(frame, "일시정지", null, null, null);
            RectTransform menu = CreateRect("Menu", frame);
            AddVertical(menu, 10f, new RectOffset(0, 0, 0, 0), false);
            menu.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            Button resumeButton = CreateMenuButton("ResumeButton", menu, "계속하기", ButtonStyle.Primary);
            CreateMenuButton("HelpButton", menu, "조작법 · 규칙", ButtonStyle.Normal);
            CreateMenuButton("OptionsButton", menu, "옵션", ButtonStyle.Normal);
            CreateMenuButton("QuitButton", menu, "항해 포기 · 타이틀로", ButtonStyle.Danger);
            AddLink(resumeButton, panel, false);
            panel.SetActive(false);
        }

        private static void BuildOptions(GameObject root)
        {
            RectTransform frame = CreateModal(root, "OptionsPanel", 460f, false, out GameObject panel);
            CreateHeader(frame, "옵션", null, null, null);
            CreateSliderRow(frame, "Volume", "음량", 0f, 1f, 0.6f, "60%");
            CreateSliderRow(frame, "Sensitivity", "마우스 감도", 0.3f, 2.5f, 1f, "1.0");
            CreateToggleRow(frame, "InvertY", "상하 반전", false);
            CreateToggleRow(frame, "Tutorial", "첫 항해 튜토리얼", true);
            RectTransform foot = CreateFoot(frame);
            AddLink(CreateButton("CloseButton", foot, "닫기", ButtonStyle.Primary), panel, false);
            panel.SetActive(false);
        }

        private static void BuildHelp(GameObject root)
        {
            RectTransform frame = CreateModal(root, "HelpPanel", 720f, false, out GameObject panel);
            CreateHeader(frame, "조작법 · 규칙", null, null, null);

            string[,] keyRows =
            {
                { "WASD", "선내 이동 / 선외 제트팩 수평 이동 (관성)" },
                { "Space", "선외 상승 · 미니게임 입력" },
                { "Ctrl", "선외 하강" },
                { "마우스", "카메라 회전" },
                { "E", "줍기 · 투입 · 납품 · 터미널 · 에어락" },
                { "E 홀드", "수리 · 밸브 잠그기 · 소화기 진압" },
                { "Q", "내려놓기 (선외에서는 다시 흘러감)" },
                { "우클릭 홀드", "견인빔 (정비에서 해금)" },
                { "Esc", "터미널 닫기 · 일시정지" },
            };
            RectTransform keyTable = CreateRect("KeyTable", frame);
            AddVertical(keyTable, 4f, new RectOffset(0, 0, 0, 0), true);
            for (int index = 0; index < keyRows.GetLength(0); index++)
            {
                RectTransform row = CreateRow("KeyRow_" + (index + 1), keyTable, 8f, TextAnchor.MiddleLeft);
                RectTransform keyCell = CreateRect("KeyCell", row);
                keyCell.gameObject.AddComponent<LayoutElement>().preferredWidth = 150f;
                AddHorizontal(keyCell, 0f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft);
                CreateKey("Key", keyCell, keyRows[index, 0]);
                CreateText("Desc", row, keyRows[index, 1], 14f, TextColor, false);
            }

            string[] ruleList =
            {
                "할당량을 모두 납부한 채 목표 행성에 도착해야 합니다. 미납이면 게임오버.",
                "코어 안정도가 0%가 되면 즉시 게임오버. 주문 성공 +2%, 안정화 물질 +10%, 주문 실패 -5%, 모듈 파괴 -10%.",
                "재료는 선외에서 흘러갑니다. 들고 들어와 터미널에서 E로 납품하거나, 납품소에서 E 2초 홀드로 포장 납품(보상 +30%)하세요.",
                "보조 에어락 지붕의 하역구에 재료를 넣으면 선내 하역함으로 들어갑니다.",
                "정제실(고철 -> 연료 셀 / 합금판)과 조립실(합금판 + 크리스탈 -> 정밀 부품)이 있으면 보상이 큰 가공품 주문이 나옵니다.",
                "사고: 화재(소화기 들고 E 3초), 산소 누출(밸브 E 4초), 기기 오류(타이밍 3회).",
                "부서진 모듈은 재건 지점에 고철 3개 + 연료 셀 1개를 넣으면 내구도 50%로 다시 섭니다.",
            };
            RectTransform rules = CreateRect("RuleList", frame);
            AddVertical(rules, 4f, new RectOffset(18, 0, 6, 0), true);
            for (int index = 0; index < ruleList.Length; index++)
            {
                CreateText("Rule_" + (index + 1), rules, "· " + ruleList[index], 13f, MutedColor, false, true);
            }

            RectTransform foot = CreateFoot(frame);
            AddLink(CreateButton("CloseButton", foot, "닫기", ButtonStyle.Primary), panel, false);
            panel.SetActive(false);
        }

        // ---------------- 진행 화면 ----------------

        private static void BuildPrepare(GameObject root)
        {
            RectTransform frame = CreateModal(root, "PreparePanel", 1120f, true, out GameObject panel);
            CreateHeader(frame, "터미널 · 출항 준비", "VoyageIndexValue", "항해 1번째", null);
            CreateInfoStrip(frame, new[,]
            {
                { "할당량", "Quota", "200" }, { "항해 시간", "VoyageTime", "4:00" }, { "도착 보상", "ArrivalReward", "50" },
                { "코어 안정도", "Stability", "100.0%" }, { "크레딧", "Credit", "0" }, { "붙인 모듈", "Attached", "2" }, { "새로고침", "Refresh", "1회" },
            });

            RectTransform cards = CreateRow("PlanetCards", frame, 14f, TextAnchor.UpperLeft);
            cards.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;
            string[,] planetList =
            {
                { "광산 행성", "고철 주문이 자주 나옴", "모듈 복구·재건 30% 할인", "6" },
                { "결정 행성", "크리스탈 주문이 잦고 보상 +30%", "없음", "4" },
                { "공업 행성", "재료가 고르게 섞임", "모듈 구매 30% 할인", "8" },
            };
            for (int index = 0; index < planetList.GetLength(0); index++)
            {
                RectTransform card = CreateImage("PlanetCard_" + (index + 1), cards, CardColor, true);
                card.gameObject.AddComponent<Outline>().effectColor = LineColor;
                card.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
                AddVertical(card, 6f, new RectOffset(16, 16, 14, 14), true);
                CreateText("NameValue", card, planetList[index, 0], 20f, TextColor, true);
                CreateCardLine(card, "Trend", "주문 경향", planetList[index, 1]);
                CreateCardLine(card, "Benefit", "정비 혜택", planetList[index, 2]);
                CreateCardLine(card, "ModuleCap", "모듈 상한", planetList[index, 3]);
                CreateBar("RouteBar", card, 16f, ClearColor, 0f);
                for (int zoneIndex = 1; zoneIndex <= 3; zoneIndex++)
                {
                    CreateText("ZoneValue_" + zoneIndex, card, "자원 밀집 지대 · 유리", 12f, TextColor, false, true);
                }
                RectTransform buttons = CreateRow("Buttons", card, 8f, TextAnchor.MiddleLeft);
                CreateButton("RefreshButton", buttons, "경로 새로고침", ButtonStyle.Small);
                CreateButton("SelectButton", buttons, "이 행성으로 출항", ButtonStyle.Primary);
            }
            CreateText("Note", frame, "할당량 · 항해 시간 · 도착 보상은 세 후보가 같습니다. 경로는 출항 후 바꿀 수 없습니다.", 12f, MutedColor, false);
            panel.SetActive(false);
        }

        private static void BuildStow(GameObject root)
        {
            RectTransform frame = CreateModal(root, "StowPanel", 720f, false, out GameObject panel);
            CreateHeader(frame, "모듈 격납", "PlanetValue", "결정 행성 · 모듈 상한 4", null);
            RectTransform strip = CreateInfoStrip(frame, new[,] { { "붙인 모듈", "Attached", "2 / 4" } });
            CreateText("StatusValue", strip, "출항 가능", 13f, GoodColor, true);

            // 격자 5×5. 이름은 Cell_{gridX}_{gridZ} (앞 = -Z가 위쪽 줄)
            RectTransform grid = CreateRect("StowGrid", frame);
            GridLayoutGroup gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(72f, 72f);
            gridLayout.spacing = new Vector2(6f, 6f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            grid.gameObject.AddComponent<LayoutElement>().preferredHeight = 384f;
            for (int gridZ = -2; gridZ <= 2; gridZ++)
            {
                for (int gridX = -2; gridX <= 2; gridX++)
                {
                    string cellName = "Cell_" + gridX + "_" + gridZ;
                    if (gridX == 0 && gridZ == 0) CreateGridCell(cellName, grid, "중앙", CentralCellColor, false);
                    else if (gridX == 0 && gridZ == -1) CreateGridCell(cellName, grid, "에어락", ModuleCellColor, true);
                    else if (gridX == 1 && gridZ == 0) CreateGridCell(cellName, grid, "납품", ModuleCellColor, true);
                    else CreateGridCell(cellName, grid, "", RowLineColor, false);
                }
            }
            CreateText("Note", frame, "위쪽이 앞쪽입니다. 빼면 다른 모듈의 연결이 끊기는 모듈은 고를 수 없습니다.", 12f, MutedColor, false, true);

            RectTransform foot = CreateFoot(frame);
            CreateButton("BackButton", foot, "행성 다시 고르기", ButtonStyle.Normal);
            CreateButton("ResetButton", foot, "초기화", ButtonStyle.Normal);
            CreateButton("ConfirmButton", foot, "격납 후 출항", ButtonStyle.Primary);
            panel.SetActive(false);
        }

        private static void BuildSettlement(GameObject root)
        {
            RectTransform frame = CreateModal(root, "SettlementPanel", 460f, false, out GameObject panel);
            CreateHeader(frame, "정산", "PlanetValue", "결정 행성 도착", null);
            string[,] rowList =
            {
                { "Quota", "할당량 납부", "200 / 200 (완납)" }, { "ArrivalReward", "도착 보상", "+50 크레딧" },
                { "Order", "주문 성공 / 실패", "0 / 0" }, { "EventsResolved", "해결한 사고", "0" },
                { "ModuleFrame", "파괴된 모듈 / 재건한 모듈", "0 / 0" }, { "Capsules", "구조 캡슐", "0" },
                { "Stability", "남은 안정도", "100.0%" }, { "VoyageCount", "완료한 항해", "1" },
            };
            CreateSettleTable(frame, rowList);
            RectTransform foot = CreateFoot(frame);
            CreateButton("NextButton", foot, "보상 받고 정비로", ButtonStyle.Primary);
            panel.SetActive(false);
        }

        private static void BuildMaintenance(GameObject root)
        {
            RectTransform frame = CreateModal(root, "MaintenancePanel", 1120f, false, out GameObject panel);
            CreateHeader(frame, "정비", "VoyageIndexValue", "다음 항해 2번째", null);
            RectTransform strip = CreateInfoStrip(frame, new[,]
            {
                { "크레딧", "Credit", "0" }, { "코어 안정도", "Stability", "100.0%" }, { "붙인 모듈", "Attached", "2" }, { "격납고", "Hangar", "0" },
            });
            CreateText("BenefitValue", strip, "광산 행성 혜택 · 모듈 복구·재건 30% 할인", 13f, TextColor, false);

            RectTransform[] bodies = CreateTabs(root, frame, new[] { "Shop", "Repair", "Assembly" }, new[] { "상점", "복구", "조립" });

            RectTransform columns = CreateRow("Columns", bodies[0], 16f, TextAnchor.UpperLeft);
            columns.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;
            RectTransform character = CreateShopColumn(columns, "Character", "캐릭터", 1f);
            CreateShopItem(character, "MoveSpeed1", "이동속도 I", "이동 · 제트팩 속도 +15%", "120");
            CreateShopItem(character, "Carry1", "운반량 I", "재료를 2개까지 운반", "250");
            CreateShopItem(character, "RepairSpeed1", "수리속도 I", "수리 속도 1.5배", "100");
            CreateShopItem(character, "Tractor", "견인빔", "우클릭 홀드로 15m 안의 재료 끌어오기", "300");
            RectTransform ship = CreateShopColumn(columns, "Ship", "함선", 1f);
            CreateShopItem(ship, "Stabilizer", "안정화 물질", "즉시 코어 안정도 +10.0%", "60");
            CreateShopItem(ship, "Refresh1", "새로고침 I", "경로 새로고침 +1회", "150");
            RectTransform module = CreateShopColumn(columns, "Module", "모듈", 1.2f);
            CreateShopItem(module, "Airlock", "보조 에어락", "선외 출입구 추가 · 하역구", "150");
            CreateShopItem(module, "Delivery", "납품소", "포장 납품대 · 보상 +30%", "180");
            CreateShopItem(module, "Refinery", "정제실", "고철 -> 연료 셀 · 합금판", "200");
            CreateShopItem(module, "RepairBay", "수리 베이", "고철 1개로 붙은 모듈 내구도 +25", "220");
            CreateShopItem(module, "DroneBay", "드론 베이", "12초마다 재료 1개 회수", "220");

            float[] repairWeights = { 2f, 1f, 1f, 1f, 1.5f };
            CreateTableRow("HeaderRow", bodies[1], new[] { "모듈", "위치", "상태", "비용", "" }, repairWeights, MutedColor, LineColor);
            RectTransform repairRow = CreateRow("RepairRow_1", bodies[1], 0f, TextAnchor.MiddleLeft);
            repairRow.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(8, 8, 6, 6);
            string[] repairCells = { "납품소", "우현", "80%", "30 크레딧" };
            for (int index = 0; index < repairCells.Length; index++)
            {
                SetFlexibleWidth(CreateText("Cell_" + index, repairRow, repairCells[index], 13f, TextColor, false).gameObject, repairWeights[index]);
            }
            RectTransform repairButtonCell = CreateRect("Cell_4", repairRow);
            SetFlexibleWidth(repairButtonCell.gameObject, repairWeights[4]);
            AddHorizontal(repairButtonCell, 0f, new RectOffset(0, 0, 0, 0), TextAnchor.MiddleLeft);
            CreateButton("RepairButton", repairButtonCell, "수리 (100%)", ButtonStyle.Small);
            CreateText("Note", bodies[1], "손상 수리: 내구도 10%당 15 크레딧 · 파괴 재건: 구매가의 50%", 12f, MutedColor, false);

            CreateText("Intro", bodies[2], "격자 위에 모듈을 배치하고 옮기거나 격납, 철거합니다. 모든 모듈은 앞뒤좌우로 이어져 중앙 모듈까지 연결되어야 합니다.", 14f, TextColor, false, true);
            CreateText("HangarValue", bodies[2], "격납고 0개 · 철거 환불 50%", 12f, MutedColor, false);
            CreateButton("OpenAssemblyButton", bodies[2], "조립 화면 열기", ButtonStyle.Primary);

            RectTransform foot = CreateFoot(frame);
            CreateButton("PrepareButton", foot, "출항 준비로", ButtonStyle.Primary);
            panel.SetActive(false);
        }

        private static void BuildGameOver(GameObject root)
        {
            RectTransform frame = CreateModal(root, "GameOverPanel", 460f, false, out GameObject panel);
            frame.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
            CreateText("Title", frame, "GAME OVER", 44f, BadColor, true).alignment = TextAlignmentOptions.Center;
            CreateText("ReasonValue", frame, "코어 안정도가 0%가 되었습니다", 14f, MutedColor, false).alignment = TextAlignmentOptions.Center;
            RectTransform newBest = CreateImage("NewBest", frame, new Color32(91, 227, 138, 46), true);
            AddHorizontal(newBest, 0f, new RectOffset(10, 10, 3, 3), TextAnchor.MiddleCenter);
            CreateText("Label", newBest, "최고 기록 갱신", 12f, GoodColor, true);
            newBest.gameObject.SetActive(false);
            CreateSettleTable(frame, new[,]
            {
                { "VoyageCount", "완료한 항해", "0회" }, { "TotalCredit", "누적 크레딧", "0" }, { "Record", "최고 기록", "항해 0회 · 0 크레딧" },
            });
            RectTransform foot = CreateFoot(frame);
            CreateButton("TitleButton", foot, "타이틀로", ButtonStyle.Normal);
            CreateButton("RestartButton", foot, "다시 시작", ButtonStyle.Primary);
            panel.SetActive(false);
        }

        // ---------------- 화면 틀 ----------------

        private static GameObject CreateCanvas(string name, int sortingOrder, Scene scene)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(root, scene);

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return root;
        }

        // 모달 화면: {panelName}(전체) → Backdrop(어둡게) + Frame(가운데, 폭 고정, 높이는 내용에 맞춤)
        private static RectTransform CreateModal(GameObject root, string panelName, float width, bool isTerminalFrame, out GameObject panel)
        {
            RectTransform panelRect = CreateRect(panelName, root.transform);
            Stretch(panelRect);
            Stretch(CreateImage("Backdrop", panelRect, BackdropColor, false));

            RectTransform frame = CreateImage("Frame", panelRect, PanelColor, true);
            frame.sizeDelta = new Vector2(width, 0f);
            frame.gameObject.AddComponent<Outline>().effectColor = isTerminalFrame ? FrameLineColor : LineColor;
            AddVertical(frame, 12f, new RectOffset(22, 22, 20, 20), true);
            frame.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            panel = panelRect.gameObject;
            return frame;
        }

        // 제목 (보조 문구) ............ (닫기 버튼). 닫기 버튼이 없으면 null
        private static Button CreateHeader(Transform frame, string title, string subName, string subText, string closeText)
        {
            RectTransform header = CreateRow("Header", frame, 12f, TextAnchor.MiddleLeft);
            CreateText("Title", header, title, 20f, TextColor, true);
            if (subName != null) CreateText(subName, header, subText, 13f, MutedColor, false);
            if (closeText == null) return null;

            AddFlexibleSpace(header);
            return CreateButton("CloseButton", header, closeText, ButtonStyle.Small);
        }

        // .info-strip: {라벨, 이름, 값} 목록. 값 텍스트 이름은 {이름}Value
        private static RectTransform CreateInfoStrip(Transform frame, string[,] itemList)
        {
            RectTransform strip = CreateImage("InfoStrip", frame, StripColor, true);
            AddHorizontal(strip, 18f, new RectOffset(12, 12, 10, 10), TextAnchor.MiddleLeft);
            for (int index = 0; index < itemList.GetLength(0); index++)
            {
                RectTransform item = CreateRow(itemList[index, 1], strip, 4f, TextAnchor.MiddleLeft);
                CreateText(itemList[index, 1] + "Label", item, itemList[index, 0], 13f, MutedColor, false);
                Color valueColor = itemList[index, 1] == "Credit" ? AccentColor : TextColor;
                CreateText(itemList[index, 1] + "Value", item, itemList[index, 2], 13f, valueColor, true);
            }
            return strip;
        }

        // 탭 버튼 + 밑줄 + 본문. UITabGroup을 루트에 붙여 연결한다. 본문 목록을 돌려준다
        private static RectTransform[] CreateTabs(GameObject root, Transform frame, string[] tabNames, string[] tabLabels)
        {
            RectTransform tabs = CreateRow("Tabs", frame, 6f, TextAnchor.MiddleLeft);
            CreateImage("TabLine", frame, LineColor, false).gameObject.AddComponent<LayoutElement>().preferredHeight = 1f;
            RectTransform body = CreateRect("TabBody", frame);
            LayoutElement bodyLayout = body.gameObject.AddComponent<LayoutElement>();
            bodyLayout.minHeight = 180f;
            AddVertical(body, 0f, new RectOffset(0, 0, 0, 0), true);

            var buttonList = new Object[tabNames.Length];
            var bodyList = new Object[tabNames.Length];
            var underlineList = new Object[tabNames.Length];
            var labelList = new Object[tabNames.Length];
            var bodyRects = new RectTransform[tabNames.Length];

            for (int index = 0; index < tabNames.Length; index++)
            {
                bool isSelected = index == 0;
                RectTransform tab = CreateImage("Tab_" + tabNames[index], tabs, ClearColor, false);
                buttonList[index] = tab.gameObject.AddComponent<Button>();
                AddHorizontal(tab, 0f, new RectOffset(14, 14, 9, 9), TextAnchor.MiddleCenter);
                labelList[index] = CreateText("Label", tab, tabLabels[index], 14f, isSelected ? TextColor : MutedColor, isSelected);

                RectTransform underline = CreateImage("Underline", tab, AccentColor, false);
                underline.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                underline.anchorMin = new Vector2(0f, 0f);
                underline.anchorMax = new Vector2(1f, 0f);
                underline.pivot = new Vector2(0.5f, 0f);
                underline.anchoredPosition = Vector2.zero;
                underline.sizeDelta = new Vector2(0f, 2f);
                underline.gameObject.SetActive(isSelected);
                underlineList[index] = underline.gameObject;

                RectTransform tabBody = CreateRect("Body_" + tabNames[index], body);
                AddVertical(tabBody, 12f, new RectOffset(0, 0, 8, 0), true);
                tabBody.gameObject.SetActive(isSelected);
                bodyList[index] = tabBody.gameObject;
                bodyRects[index] = tabBody;
            }

            var tabGroup = new SerializedObject(root.AddComponent<UITabGroup>());
            SetArray(tabGroup, "tabButtons", buttonList);
            SetArray(tabGroup, "tabBodies", bodyList);
            SetArray(tabGroup, "tabUnderlines", underlineList);
            SetArray(tabGroup, "tabLabels", labelList);
            tabGroup.ApplyModifiedPropertiesWithoutUndo();
            return bodyRects;
        }

        // .panel-foot: 오른쪽 정렬 버튼 줄
        private static RectTransform CreateFoot(Transform frame)
        {
            RectTransform foot = CreateRow("Foot", frame, 10f, TextAnchor.MiddleRight);
            foot.GetComponent<LayoutElement>().minHeight = 40f;
            return foot;
        }

        // ---------------- 부품 ----------------

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject target = new GameObject(name, typeof(RectTransform));
            target.transform.SetParent(parent, false);
            return target.GetComponent<RectTransform>();
        }

        private static RectTransform CreateImage(string name, Transform parent, Color color, bool isRounded)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            if (isRounded)
            {
                image.sprite = roundSprite;
                image.type = Image.Type.Sliced;
            }
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float size, Color color, bool isBold, bool isWrapped = false)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal;
            label.textWrappingMode = isWrapped ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, string text, ButtonStyle style)
        {
            bool isPrimary = style == ButtonStyle.Primary || style == ButtonStyle.Big;
            Color background = isPrimary ? AccentColor : (style == ButtonStyle.Danger ? DangerColor : ButtonColor);
            RectTransform rect = CreateImage(name, parent, background, true);
            Button button = rect.gameObject.AddComponent<Button>();
            if (isPrimary == false) rect.gameObject.AddComponent<Outline>().effectColor = LineColor;

            RectOffset padding = new RectOffset(16, 16, 9, 9);
            float fontSize = 14f;
            if (style == ButtonStyle.Small)
            {
                padding = new RectOffset(11, 11, 6, 6);
                fontSize = 13f;
            }
            if (style == ButtonStyle.Big)
            {
                padding = new RectOffset(34, 34, 14, 14);
                fontSize = 18f;
            }
            AddHorizontal(rect, 0f, padding, TextAnchor.MiddleCenter);
            CreateText("Label", rect, text, fontSize, isPrimary ? PrimaryTextColor : TextColor, isPrimary);
            return button;
        }

        // .menu .btn: 최소 폭 240
        private static Button CreateMenuButton(string name, Transform parent, string text, ButtonStyle style)
        {
            Button button = CreateButton(name, parent, text, style);
            button.gameObject.AddComponent<LayoutElement>().minWidth = 240f;
            return button;
        }

        private static RectTransform CreateRow(string name, Transform parent, float spacing, TextAnchor alignment)
        {
            RectTransform row = CreateRect(name, parent);
            AddHorizontal(row, spacing, new RectOffset(0, 0, 0, 0), alignment);
            row.gameObject.AddComponent<LayoutElement>();
            return row;
        }

        private static void CreateLabelValue(Transform parent, string name, string label, string value, Color valueColor)
        {
            RectTransform group = CreateRow(name, parent, 6f, TextAnchor.MiddleLeft);
            CreateText(name + "Label", group, label, 14f, MutedColor, false);
            CreateText(name + "Value", group, value, 14f, valueColor, true);
        }

        private static void CreateKey(string name, Transform parent, string text)
        {
            RectTransform key = CreateImage(name, parent, KeyColor, true);
            AddHorizontal(key, 0f, new RectOffset(7, 7, 1, 1), TextAnchor.MiddleCenter);
            CreateText("Label", key, text, 12f, KeyTextColor, true);
        }

        // .bar: 배경 + Fill. 비율은 Fill의 anchorMax.x로 표시한다 (0 ~ 1)
        private static RectTransform CreateBar(string name, Transform parent, float height, Color fillColor, float rate)
        {
            RectTransform bar = CreateImage(name, parent, BarColor, true);
            bar.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            bar.sizeDelta = new Vector2(bar.sizeDelta.x, height);
            RectTransform fill = CreateImage("Fill", bar, fillColor, true);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(rate, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            return bar;
        }

        private static RectTransform CreateHudPanel(string name, Transform parent, Vector2 anchor, Vector2 position, float width)
        {
            RectTransform panel = CreateImage(name, parent, HudPanelColor, true);
            Place(panel, anchor, position, new Vector2(width, 0f));
            AddVertical(panel, 4f, new RectOffset(12, 12, 10, 10), true);
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return panel;
        }

        private static void CreateStatusCell(Transform parent, string name, string label, string value, Color valueColor)
        {
            RectTransform cell = CreateRect(name, parent);
            AddVertical(cell, 0f, new RectOffset(0, 0, 0, 0), true);
            CreateText(name + "Label", cell, label, 11f, MutedColor, false);
            CreateText(name + "Value", cell, value, 16f, valueColor, true);
        }

        private static void CreateCardLine(Transform parent, string name, string label, string value)
        {
            RectTransform line = CreateRow(name, parent, 8f, TextAnchor.UpperLeft);
            CreateText(name + "Label", line, label, 13f, MutedColor, false).gameObject.AddComponent<LayoutElement>().minWidth = 58f;
            SetFlexibleWidth(CreateText(name + "Value", line, value, 13f, TextColor, false, true).gameObject, 1f);
        }

        private static RectTransform CreateShopColumn(Transform parent, string name, string title, float weight)
        {
            RectTransform column = CreateRect(name, parent);
            SetFlexibleWidth(column.gameObject, weight);
            AddVertical(column, 6f, new RectOffset(0, 0, 0, 0), true);
            CreateText("Title", column, title, 13f, AccentColor, true);
            return column;
        }

        // .shop-item: 왼쪽 이름과 설명, 오른쪽 가격과 구매 버튼
        private static void CreateShopItem(Transform parent, string name, string title, string desc, string price)
        {
            RectTransform item = CreateImage(name, parent, StripColor, true);
            AddHorizontal(item, 10f, new RectOffset(10, 10, 9, 9), TextAnchor.MiddleLeft);

            RectTransform left = CreateRect("Info", item);
            SetFlexibleWidth(left.gameObject, 1f);
            AddVertical(left, 2f, new RectOffset(0, 0, 0, 0), true);
            CreateText("NameValue", left, title, 14f, TextColor, true);
            CreateText("DescValue", left, desc, 12f, MutedColor, false, true);

            RectTransform right = CreateRow("Buy", item, 8f, TextAnchor.MiddleRight);
            CreateText("PriceValue", right, price, 12f, TextColor, false);
            CreateButton("BuyButton", right, "구매", ButtonStyle.Small);
        }

        private static void CreateGridCell(string name, Transform parent, string text, Color color, bool isButton)
        {
            RectTransform cell = CreateImage(name, parent, color, true);
            if (isButton) cell.gameObject.AddComponent<Button>();

            TextMeshProUGUI label = CreateText("Label", cell, text, 13f, TextColor, color == CentralCellColor);
            label.alignment = TextAlignmentOptions.Center;
            Stretch(label.rectTransform);
        }

        // .data-table 한 줄. 칸 너비는 weights 비율, 아래에 구분선
        private static void CreateTableRow(string name, Transform parent, string[] cells, float[] weights, Color textColor, Color lineColor)
        {
            RectTransform row = CreateRow(name, parent, 0f, TextAnchor.MiddleLeft);
            row.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(8, 8, 8, 8);
            for (int index = 0; index < cells.Length; index++)
            {
                SetFlexibleWidth(CreateText("Cell_" + index, row, cells[index], 13f, textColor, false).gameObject, weights[index]);
            }
            CreateRowLine(row, lineColor);
        }

        // .data-table.settle: 왼쪽 항목, 오른쪽 값(굵게). 값 이름은 {이름}Value
        private static void CreateSettleTable(Transform frame, string[,] rowList)
        {
            RectTransform table = CreateRect("Table", frame);
            AddVertical(table, 0f, new RectOffset(0, 0, 0, 0), true);
            for (int index = 0; index < rowList.GetLength(0); index++)
            {
                RectTransform row = CreateRow(rowList[index, 0], table, 0f, TextAnchor.MiddleLeft);
                row.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(8, 8, 8, 8);
                SetFlexibleWidth(CreateText(rowList[index, 0] + "Label", row, rowList[index, 1], 13f, TextColor, false).gameObject, 1f);
                CreateText(rowList[index, 0] + "Value", row, rowList[index, 2], 13f, TextColor, true).alignment = TextAlignmentOptions.Right;
                CreateRowLine(row, RowLineColor);
            }
        }

        private static void CreateRowLine(RectTransform row, Color lineColor)
        {
            RectTransform line = CreateImage("Line", row, lineColor, false);
            line.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            line.anchorMin = new Vector2(0f, 0f);
            line.anchorMax = new Vector2(1f, 0f);
            line.pivot = new Vector2(0.5f, 0f);
            line.anchoredPosition = Vector2.zero;
            line.sizeDelta = new Vector2(0f, 1f);
        }

        // .option-row + range: 라벨 / 슬라이더 / 값
        private static void CreateSliderRow(Transform frame, string name, string label, float minValue, float maxValue, float value, string valueText)
        {
            RectTransform row = CreateRow(name, frame, 12f, TextAnchor.MiddleLeft);
            row.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 10, 10);
            CreateText(name + "Label", row, label, 14f, TextColor, false).gameObject.AddComponent<LayoutElement>().minWidth = 110f;

            GameObject sliderObject = DefaultControls.CreateSlider(CreateDefaultResources());
            sliderObject.name = name + "Slider";
            sliderObject.transform.SetParent(row, false);
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = value;
            sliderObject.transform.Find("Background").GetComponent<Image>().color = BarColor;
            sliderObject.transform.Find("Fill Area/Fill").GetComponent<Image>().color = AccentColor;
            LayoutElement sliderLayout = sliderObject.AddComponent<LayoutElement>();
            sliderLayout.flexibleWidth = 1f;
            sliderLayout.preferredHeight = 20f;

            CreateText(name + "Value", row, valueText, 14f, TextColor, false).gameObject.AddComponent<LayoutElement>().minWidth = 48f;
            CreateRowLine(row, RowLineColor);
        }

        // .option-row + checkbox: 라벨 ........ 체크
        private static void CreateToggleRow(Transform frame, string name, string label, bool isOn)
        {
            RectTransform row = CreateRow(name, frame, 12f, TextAnchor.MiddleLeft);
            row.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 10, 10);
            CreateText(name + "Label", row, label, 14f, TextColor, false);
            AddFlexibleSpace(row);

            GameObject toggleObject = DefaultControls.CreateToggle(CreateDefaultResources());
            toggleObject.name = name + "Toggle";
            toggleObject.transform.SetParent(row, false);
            Object.DestroyImmediate(toggleObject.transform.Find("Label").gameObject);
            toggleObject.GetComponent<Toggle>().isOn = isOn;
            toggleObject.transform.Find("Background/Checkmark").GetComponent<Image>().color = AccentColor;
            LayoutElement toggleLayout = toggleObject.AddComponent<LayoutElement>();
            toggleLayout.preferredWidth = 20f;
            toggleLayout.preferredHeight = 20f;

            CreateRowLine(row, RowLineColor);
        }

        private static DefaultControls.Resources CreateDefaultResources()
        {
            return new DefaultControls.Resources
            {
                standard = roundSprite,
                background = backgroundSprite,
                knob = knobSprite,
                checkmark = checkmarkSprite,
            };
        }

        // ---------------- 버튼 연결 ----------------

        private static void AddLink(Button button, GameObject target, bool isActive)
        {
            linkList.Add(new LinkSpec { Button = button, Target = target, IsActive = isActive });
        }

        // 모아 둔 연결을 루트의 UIButtonSetActive.links에 넣는다
        private static void AttachLinks(GameObject root)
        {
            if (linkList.Count == 0) return;

            var serialized = new SerializedObject(root.AddComponent<UIButtonSetActive>());
            SerializedProperty links = serialized.FindProperty("links");
            links.arraySize = linkList.Count;
            for (int index = 0; index < linkList.Count; index++)
            {
                SerializedProperty link = links.GetArrayElementAtIndex(index);
                link.FindPropertyRelative("Button").objectReferenceValue = linkList[index].Button;
                link.FindPropertyRelative("Target").objectReferenceValue = linkList[index].Target;
                link.FindPropertyRelative("IsActive").boolValue = linkList[index].IsActive;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(SerializedObject serialized, string propertyName, Object[] values)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
        }

        // ---------------- 레이아웃 ----------------

        private static void AddFlexibleSpace(Transform parent)
        {
            SetFlexibleWidth(CreateRect("Space", parent).gameObject, 1f);
        }

        private static void SetFlexibleWidth(GameObject target, float weight)
        {
            LayoutElement layout = target.GetComponent<LayoutElement>();
            if (layout == null) layout = target.AddComponent<LayoutElement>();
            layout.minWidth = 0f;
            layout.preferredWidth = 0f;
            layout.flexibleWidth = weight;
        }

        private static void AddVertical(RectTransform target, float spacing, RectOffset padding, bool isFullWidth)
        {
            VerticalLayoutGroup layout = target.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = isFullWidth;
            layout.childForceExpandHeight = false;
        }

        private static void AddHorizontal(RectTransform target, float spacing, RectOffset padding, TextAnchor alignment)
        {
            HorizontalLayoutGroup layout = target.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = alignment;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        // 화면 모서리 기준 배치. anchor는 붙일 모서리(0 ~ 1), position은 그 모서리에서 떨어진 거리
        private static void Place(RectTransform target, Vector2 anchor, Vector2 position, Vector2 size)
        {
            target.anchorMin = anchor;
            target.anchorMax = anchor;
            target.pivot = anchor;
            target.anchoredPosition = position;
            target.sizeDelta = size;
        }

        private static void Stretch(RectTransform target)
        {
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }
    }
}
