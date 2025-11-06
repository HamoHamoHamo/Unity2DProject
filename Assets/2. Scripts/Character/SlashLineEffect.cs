using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlashLineEffect : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private float lineWidth = 0.15f;
    [SerializeField] private float lineLength = 3f;
    [SerializeField] private Gradient lineColor;
    [SerializeField] private Gradient endColor;

    [Header("Animation Settings")]
    [SerializeField] private float extendDuration = 0.08f; // 빠르게 뻗기
    [SerializeField] private float fadeDuration = 0.12f; // 천천히 사라지기
    [SerializeField] private AnimationCurve extendCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Visual Effects")]
    [SerializeField] private bool useWidthAnimation = true; // 두께 애니메이션
    [SerializeField] private float maxWidth = 0.3f; // 최대 두께
    [SerializeField] private float minWidth = 0.05f; // 최소 두께

    private LineRenderer lineRenderer;
    private bool isActive;
    private float timer;
    private Vector3 startPos;
    private Vector3 endPos;
    private Gradient originalGradient;

    private GradientColorKey[] startColorKeys;
    private GradientColorKey[] targetColorKeys;
    private GradientAlphaKey[] originalAlphaKeys;
    private Gradient tempGradient; // 매번 new 하지 않기 위한 임시 그라데이션

    private enum AnimationPhase
    {
        Extending,
        Fading
    }
    private AnimationPhase currentPhase;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        if (lineColor == null || lineColor.colorKeys.Length == 0)
        {
            SetupDefaultGradient();
        }

        // originalGradient = new Gradient();
        // originalGradient.SetKeys(lineColor.colorKeys, lineColor.alphaKeys);

        startColorKeys = lineColor.colorKeys;
        originalAlphaKeys = lineColor.alphaKeys;

        // endColor가 지정되었는지 확인
        if (endColor != null && endColor.colorKeys.Length > 0)
        {
            targetColorKeys = endColor.colorKeys;
        }
        else
        {
            // endColor가 없으면, 그냥 시작 색상으로 고정
            targetColorKeys = startColorKeys;
        }

        tempGradient = new Gradient(); // 임시 그라데이션 초기화

        lineRenderer.colorGradient = lineColor;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (!isActive) return;

        timer += Time.unscaledDeltaTime;

        float totalDuration = extendDuration + fadeDuration;
        float totalProgress = 0f; // 0.0 (시작) ~ 1.0 (끝)

        switch (currentPhase)
        {
            case AnimationPhase.Extending:
                totalProgress = Mathf.Clamp01(timer / totalDuration);
                UpdateExtending(totalProgress); // ★ totalProgress 전달
                break;
            case AnimationPhase.Fading:
                totalProgress = Mathf.Clamp01((extendDuration + timer) / totalDuration);
                UpdateFading(totalProgress); // ★ totalProgress 전달
                break;
        }
    }

    private void UpdateExtending(float totalProgress)
    {
        float progress = Mathf.Clamp01(timer / extendDuration);
        float curveValue = extendCurve.Evaluate(progress);

        // 뻗는 애니메이션
        Vector3 currentEndPos = Vector3.Lerp(startPos, endPos, curveValue);
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, currentEndPos);

        // 두께 애니메이션 (점점 두꺼워짐)
        if (useWidthAnimation)
        {
            float width = Mathf.Lerp(minWidth, maxWidth, curveValue);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width * 0.5f; // 끝은 좀 더 얇게
        }

        UpdateGradient(totalProgress, 1f);

        if (progress >= 1f)
        {
            currentPhase = AnimationPhase.Fading;
            timer = 0f;
        }
    }

    private void UpdateFading(float totalProgress)
    {
        float progress = Mathf.Clamp01(timer / fadeDuration);

        // 뒤에서부터 사라지기
        Vector3 currentStartPos = Vector3.Lerp(startPos, endPos, progress);
        lineRenderer.SetPosition(0, currentStartPos);
        lineRenderer.SetPosition(1, endPos);

        // 두께도 점차 감소
        if (useWidthAnimation)
        {
            float width = Mathf.Lerp(maxWidth, minWidth, progress);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width * 0.5f;
        }

        // 알파값 감소
        UpdateGradient(totalProgress, 1f - progress);

        if (progress >= 1f)
        {
            lineRenderer.enabled = false;
            isActive = false;
        }
    }

    private void UpdateGradient(float colorProgress, float alphaMultiplier)
    {
        int colorKeyCount = startColorKeys.Length;
        GradientColorKey[] newColorKeys = new GradientColorKey[colorKeyCount];

        for (int i = 0; i < colorKeyCount; i++)
        {
            // 시작 색상과 끝 색상 사이를 보간
            Color lerpedColor = Color.Lerp(
                startColorKeys[i].color,
                targetColorKeys[i].color,
                colorProgress
            );

            // 새 색상 키 생성 (시간은 원본 키의 시간 사용)
            newColorKeys[i] = new GradientColorKey(lerpedColor, startColorKeys[i].time);
        }

        // 2. 알파 계산 (기존 코드와 동일)
        GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[originalAlphaKeys.Length];
        for (int i = 0; i < originalAlphaKeys.Length; i++)
        {
            newAlphaKeys[i] = new GradientAlphaKey(
                originalAlphaKeys[i].alpha * alphaMultiplier,
                originalAlphaKeys[i].time
            );
        }

        // 3. 임시 그라데이션에 최종 키 적용 및 렌더러에 설정
        tempGradient.SetKeys(newColorKeys, newAlphaKeys);
        lineRenderer.colorGradient = tempGradient;
    }

    public void ShowSlashLineFromAttacker(Transform attacker)
    {
        if (attacker == null) return;

        Vector2 attackerCenter = GetCenterPosition(attacker);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = (mousePos - attackerCenter).normalized;
        Vector2 start = attackerCenter - direction * lineLength / 2;
        ShowSlashLine(start, direction);
    }

    public void ShowSlashLine(Vector3 start, Vector2 direction)
    {
        startPos = start;
        endPos = start + (Vector3)(direction.normalized * lineLength);

        // 초기 상태
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, startPos);

        if (useWidthAnimation)
        {
            lineRenderer.startWidth = minWidth;
            lineRenderer.endWidth = minWidth * 0.5f;
        }
        else
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }

        // ★ 수정된 부분
        // 시작 색상(lineColor)으로 초기화
        tempGradient.SetKeys(startColorKeys, originalAlphaKeys);
        lineRenderer.colorGradient = tempGradient;

        lineRenderer.enabled = true;
        isActive = true;
        currentPhase = AnimationPhase.Extending;
        timer = 0f;
    }

    private Vector2 GetCenterPosition(Transform target)
    {
        Collider2D col = target.GetComponent<Collider2D>();
        if (col != null)
        {
            return col.bounds.center;
        }
        return target.position;
    }

    private void SetupDefaultGradient()
    {
        lineColor = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(new Color(1f, 0.8f, 0.9f), 0f); // 밝은 분홍
        colorKeys[1] = new GradientColorKey(new Color(1f, 0.4f, 0.8f), 0.5f); // 진한 분홍
        colorKeys[2] = new GradientColorKey(new Color(0.8f, 0.2f, 0.6f), 1f); // 어두운 분홍

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f); // 시작 불투명
        alphaKeys[1] = new GradientAlphaKey(0.8f, 0.5f); // 중간 약간 투명
        alphaKeys[2] = new GradientAlphaKey(0f, 1f); // 끝 완전 투명

        lineColor.SetKeys(colorKeys, alphaKeys);
    }
}