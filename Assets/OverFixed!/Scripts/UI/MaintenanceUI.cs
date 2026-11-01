using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OverFixed
{
    // PLANET 정비 화면. 다음 항해 번호, 크레딧, 안정도, 다음 항해 할당량을 보여 주고 안정화 물질 구매와 다음 항해 시작을 받는다
    public class MaintenanceUI : MonoBehaviour
    {
        [SerializeField] private CoreConfig coreConfig;

        [Header("상태 줄")]
        [SerializeField] private TMP_Text voyageIndexValue;     // "다음 항해 2번째"
        [SerializeField] private TMP_Text creditValue;
        [SerializeField] private TMP_Text stabilityValue;
        [SerializeField] private TMP_Text quotaValue;

        [Header("안정화 물질")]
        [SerializeField] private TMP_Text recoveryValue;
        [SerializeField] private TMP_Text priceValue;
        [SerializeField] private Button buyButton;
        [SerializeField] private TMP_Text buyLabel;

        [SerializeField] private Button startButton;            // 다음 항해 시작

        private void Start()
        {
            GameManager gameManager = GameManager.Instance;
            buyButton.onClick.AddListener(BuyStabilizer);
            startButton.onClick.AddListener(gameManager.StartNextVoyage);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            voyageIndexValue.text = "다음 항해 " + (gameManager.VoyageIndex + 1) + "번째";
            quotaValue.text = gameManager.NextVoyage.QuotaBase + " 크레딧";
            recoveryValue.text = "즉시 코어 안정도 +" + coreConfig.StabilizerRecovery + "%";
            priceValue.text = coreConfig.StabilizerPrice.ToString();
            Draw();
        }

        // 정비 중에는 값이 구매로만 바뀌므로 처음과 구매 뒤에만 그린다
        private void Draw()
        {
            GameManager gameManager = GameManager.Instance;
            creditValue.text = gameManager.Credit.ToString();
            stabilityValue.text = UIText.ToStability(gameManager.Stability);
            buyButton.interactable = gameManager.CanBuyStabilizer;
            buyLabel.text = UIText.ToStabilizerBuyLabel(gameManager);
        }

        private void BuyStabilizer()
        {
            GameManager.Instance.BuyStabilizer();
            Draw();
        }
    }
}
