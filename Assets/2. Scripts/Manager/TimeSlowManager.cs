using System.Collections;
using UnityEngine;

/// <summary>
/// 카타나 제로 스타일의 시간 감속 시스템
/// Shift 키를 누르면 에너지를 소모하며 시간이 느려짐
/// </summary>
public class TimeSlowManager : MonoBehaviour
{
    [Header("Time Slow Settings")]
    [SerializeField] private float slowMotionScale = 0.2f;  // 시간 감속 정도 (0.2 = 20% 속도)
    [SerializeField] private float normalScale = 1f;        // 일반 시간 속도
    [SerializeField] private float transitionSpeed = 5f;    // 시간 속도 전환 부드러움

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 200f;        // 최대 에너지
    [SerializeField] private float energyDrainRate = 20f;   // 초당 에너지 소모율
    [SerializeField] private float energyRegenRate = 20f;   // 초당 에너지 회복률
    [SerializeField] private float energyRegenDelay = 1f;   // 에너지 회복 시작 전 대기 시간

    [Header("Audio Pitch")]
    [SerializeField] private bool adjustAudioPitch = true;  // 오디오 피치를 시간 속도에 맞춰 조정할지 여부

    private SoundManager soundManager;     // SoundManager 참조

    private float currentEnergy;
    private float targetTimeScale = 1f;
    private bool isSlowMotionActive = false;
    private float lastEnergyUseTime;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyPercent => currentEnergy / maxEnergy;
    public bool IsSlowMotionActive => isSlowMotionActive;

    void Awake()
    {
        currentEnergy = maxEnergy;

        // SoundManager가 할당되지 않았으면 자동으로 찾기
        if (soundManager == null)
        {
            soundManager = Managers.Sound;
        }
    }

    void Update()
    {
        HandleTimeSlowInput();
        UpdateEnergy();
        UpdateTimeScale();
    }

    /// <summary>
    /// Shift 키 입력 처리
    /// </summary>
    private void HandleTimeSlowInput()
    {
        // Shift 키를 누르고 있고 에너지가 있을 때
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (currentEnergy > 0)
            {
                ActivateSlowMotion();
            }
            else
            {
                DeactivateSlowMotion();
            }
        }
        else
        {
            DeactivateSlowMotion();
        }
    }

    /// <summary>
    /// 에너지 업데이트
    /// </summary>
    private void UpdateEnergy()
    {
        if (isSlowMotionActive)
        {
            // 슬로우 모션 중 에너지 소모 (실제 시간 기준)
            currentEnergy -= energyDrainRate * Time.unscaledDeltaTime;
            currentEnergy = Mathf.Max(0, currentEnergy);
            lastEnergyUseTime = Time.unscaledTime;

            // 에너지가 다 떨어지면 슬로우 모션 해제
            if (currentEnergy <= 0)
            {
                DeactivateSlowMotion();
            }
        }
        else
        {
            // 슬로우 모션이 아닐 때 에너지 회복
            if (Time.unscaledTime - lastEnergyUseTime >= energyRegenDelay)
            {
                currentEnergy += energyRegenRate * Time.unscaledDeltaTime;
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy);
            }
        }
    }

    /// <summary>
    /// Time.timeScale을 부드럽게 전환
    /// </summary>
    private void UpdateTimeScale()
    {
        Time.timeScale = Mathf.Lerp(
            Time.timeScale,
            targetTimeScale,
            transitionSpeed * Time.unscaledDeltaTime
        );

        // 물리 시뮬레이션 타이밍 조정
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // 오디오 피치를 시간 속도에 맞춰 조정
        if (adjustAudioPitch && soundManager != null)
        {
            soundManager.SetSFXPitch(Time.timeScale);
        }
    }

    /// <summary>
    /// 슬로우 모션 활성화
    /// </summary>
    public void ActivateSlowMotion()
    {
        if (!isSlowMotionActive)
        {
            isSlowMotionActive = true;
            targetTimeScale = slowMotionScale;

            // 슬로우 모션 사운드 재생
            soundManager.PauseBGM();
            soundManager.PlaySlowMotionSounds();

        }
    }

    /// <summary>
    /// 슬로우 모션 비활성화
    /// </summary>
    public void DeactivateSlowMotion()
    {
        if (isSlowMotionActive)
        {
            isSlowMotionActive = false;
            targetTimeScale = normalScale;

            // 슬로우 모션 사운드 정지 및 초기화
            soundManager.StopSlowMotionSounds();
            soundManager.ResumeBGM();
        }
    }

    /// <summary>
    /// 에너지 추가
    /// </summary>
    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
    }

    /// <summary>
    /// 에너지 설정
    /// </summary>
    public void SetEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(amount, 0, maxEnergy);
    }

    void OnDestroy()
    {
        // 게임이 종료되거나 씬이 변경될 때 시간 속도 및 오디오 피치 복원
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (soundManager != null)
        {
            soundManager.SetGlobalPitch(1f);
        }
    }
}
