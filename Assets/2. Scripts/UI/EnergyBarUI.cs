using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 시간 감속 에너지 바 UI
/// </summary>
public class EnergyBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image energyFillImage;
    [SerializeField] private Text energyText;

    [Header("Colors")]
    [SerializeField] private Color fullColor = new Color(0.2f, 0.8f, 1f);      // 청록색
    [SerializeField] private Color lowColor = new Color(1f, 0.3f, 0.3f);       // 빨간색
    [SerializeField] private float lowEnergyThreshold = 0.3f;                   // 낮은 에너지 임계값

    [Header("Animation")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 3f;

    private TimeSlowManager timeSlowManager;
    private CanvasGroup canvasGroup;

    void Start()
    {
        timeSlowManager = Managers.TimeSlow;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (timeSlowManager == null)
        {
            timeSlowManager = Managers.TimeSlow;
            return;
        }

        UpdateEnergyBar();
        UpdateVisibility();
    }

    /// <summary>
    /// 에너지 바 업데이트
    /// </summary>
    private void UpdateEnergyBar()
    {
        float energyPercent = timeSlowManager.EnergyPercent;

        // Fill Amount 업데이트
        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = energyPercent;

            // 색상 변경 (에너지가 낮을 때 빨간색)
            Color targetColor = energyPercent > lowEnergyThreshold ? fullColor : lowColor;

            // 낮은 에너지일 때 펄스 효과
            if (enablePulse && energyPercent <= lowEnergyThreshold)
            {
                float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
                targetColor = Color.Lerp(lowColor * 0.5f, lowColor, pulse);
            }

            energyFillImage.color = targetColor;
        }

        // 텍스트 업데이트 (선택 사항)
        if (energyText != null)
        {
            energyText.text = $"{Mathf.CeilToInt(timeSlowManager.CurrentEnergy)}";
        }
    }

    /// <summary>
    /// UI 가시성 업데이트
    /// </summary>
    private void UpdateVisibility()
    {
        if (canvasGroup != null)
        {
            // 슬로우 모션이 활성화되었거나 에너지가 최대가 아닐 때 UI 표시
            bool shouldShow = timeSlowManager.IsSlowMotionActive ||
                             timeSlowManager.CurrentEnergy < timeSlowManager.MaxEnergy;

            float targetAlpha = shouldShow ? 1f : 0.3f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, 5f * Time.unscaledDeltaTime);
        }
    }
}
