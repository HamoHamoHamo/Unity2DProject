using System.Collections;
using UnityEngine;

/// <summary>
/// 타격 이펙트를 통합 관리 (히트스톱, 카메라 쉐이크, 사운드 등)
/// </summary>
public class HitEffectManager : MonoBehaviour
{
    [Header("Hit Stop Settings")]
    [SerializeField] private float hitStopDuration = 0.5f;
    [SerializeField] private float hitStopTimeScale = 0.02f;

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 0.4f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private Vector3 cameraOriginalPosition;
    private Coroutine hitStopCoroutine;
    private Coroutine shakeCoroutine;

    private static HitEffectManager instance;
    public static HitEffectManager Instance
    {
        get
        {
            if (instance == null) instance = new HitEffectManager();
            return instance;

        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        cameraOriginalPosition = cameraTransform.localPosition;
    }

    /// <summary>
    /// 기본 히트 이펙트 재생 (히트스톱 + 카메라 쉐이크)
    /// </summary>
    public void PlayHitEffect()
    {
        PlayHitEffect(hitStopDuration, hitStopTimeScale, shakeDuration, shakeMagnitude);
    }

    /// <summary>
    /// 커스텀 히트 이펙트 재생
    /// </summary>
    public void PlayHitEffect(float stopDuration, float timeScale, float shakeDur, float shakeMag)
    {
        UpdateCameraPosition();
        // 히트스톱 실행
        if (hitStopCoroutine != null) StopCoroutine(hitStopCoroutine);
        hitStopCoroutine = StartCoroutine(HitStopCo(stopDuration, timeScale));

        // 카메라 쉐이크 실행
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(CameraShakeCo(shakeDur, shakeMag));
    }

    private IEnumerator HitStopCo(float duration, float timeScale)
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
    }

    private IEnumerator CameraShakeCo(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            cameraTransform.localPosition = cameraOriginalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cameraTransform.localPosition = cameraOriginalPosition;
    }

    /// <summary>
    /// 카메라 원래 위치 업데이트 (카메라가 움직이는 게임용)
    /// </summary>
    public void UpdateCameraPosition()
    {
        cameraOriginalPosition = cameraTransform.localPosition;
    }
}