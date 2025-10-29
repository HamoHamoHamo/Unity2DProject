using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private float effectDuration = 0.3f;
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private SpriteRenderer slashRenderer;
    private Color originalColor;
    private float timer;
    private bool isPlaying;

    private void Awake()
    {
        slashRenderer = GetComponent<SpriteRenderer>();
        originalColor = slashRenderer.color;
    }

    // Animation Event에서 호출됨
    public void PlaySlashEffect()
    {
        gameObject.SetActive(true);
        slashRenderer.color = originalColor;
        timer = 0f;
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        float progress = timer / effectDuration;

        if (progress >= 1f)
        {
            gameObject.SetActive(false);
            isPlaying = false;
            return;
        }

        // 페이드 아웃 효과
        Color color = originalColor;
        color.a = originalColor.a * fadeOutCurve.Evaluate(progress);
        slashRenderer.color = color;
    }
}