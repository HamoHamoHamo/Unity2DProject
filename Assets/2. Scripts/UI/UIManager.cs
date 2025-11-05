using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Time Display")]
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Kill Display")]
    [SerializeField] private TextMeshProUGUI killText;

    [Header("Die Display")]
    [SerializeField] private CanvasGroup gameOverPanel;


    public void UpdateTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdateKills(int kills)
    {
        killText.text = kills.ToString();
    }

    public void ShowGameOverPanel()
    {
        StartCoroutine(FadeIn(gameOverPanel));
    }

    public void HideGameOverPanel()
    {
        gameOverPanel.alpha = 0;
    }

    // Fade 효과 (Coroutine 사용)
    private IEnumerator FadeIn(CanvasGroup group, float duration = 0.3f, System.Action onComplete = null)
    {
        group.alpha = 0f;
        group.interactable = false;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // timeScale 무시
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        onComplete?.Invoke();
    }

    private IEnumerator FadeOut(CanvasGroup group, float duration = 0.3f, System.Action onComplete = null)
    {
        group.interactable = false;

        float elapsed = 0f;
        float startAlpha = group.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        group.alpha = 0f;
        onComplete?.Invoke();
    }

}
