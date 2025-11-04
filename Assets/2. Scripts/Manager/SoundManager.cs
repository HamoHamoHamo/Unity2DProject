using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사운드 관리 매니저
/// BGM과 효과음을 재생/관리
/// </summary>
public class SoundManager : MonoBehaviour
{
    [Header("Sound Data")]
    [SerializeField] private SoundData soundData;

    private float bgmVolume;
    private float sfxVolume;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, SoundData.Sound> sfxDictionary = new Dictionary<string, SoundData.Sound>();

    void Awake()
    {
        // SoundData가 이미 할당되어 있으면 초기화
        if (soundData != null)
        {
            Initialize(soundData);
        }
    }

    /// <summary>
    /// SoundData를 주입받아 초기화
    /// </summary>
    public void Initialize(SoundData data)
    {
        if (data == null)
        {
            Debug.LogError("SoundData가 null입니다!");
            return;
        }

        soundData = data;
        InitializeAudioSources();
        InitializeDictionaries();

        Debug.Log($"SoundManager 초기화 완료: BGM {soundData.bgmSounds.Length}개, SFX {soundData.sfxSounds.Length}개");
    }

    /// <summary>
    /// AudioSource 초기화
    /// </summary>
    private void InitializeAudioSources()
    {
        if (soundData == null)
        {
            Debug.LogError("SoundData가 할당되지 않았습니다!");
            return;
        }

        // 기본 볼륨 설정
        bgmVolume = soundData.defaultBGMVolume;
        sfxVolume = soundData.defaultSFXVolume;

        // BGM 전용 AudioSource
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.playOnAwake = false;

        // 효과음 전용 AudioSource
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.playOnAwake = false;
    }

    /// <summary>
    /// 사운드 Dictionary 초기화
    /// </summary>
    private void InitializeDictionaries()
    {
        if (soundData == null) return;

        // BGM Dictionary
        foreach (SoundData.Sound sound in soundData.bgmSounds)
        {
            if (sound.clip != null && !bgmDictionary.ContainsKey(sound.name))
            {
                bgmDictionary.Add(sound.name, sound.clip);
            }
        }

        // SFX Dictionary
        foreach (SoundData.Sound sound in soundData.sfxSounds)
        {
            if (sound.clip != null && !sfxDictionary.ContainsKey(sound.name))
            {
                sfxDictionary.Add(sound.name, sound);
            }
        }
    }

    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(string soundName)
    {
        if (bgmDictionary.TryGetValue(soundName, out AudioClip clip))
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying)
            {
                return; // 이미 같은 BGM이 재생 중이면 무시
            }

            bgmSource.clip = clip;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{soundName}'을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void Play(string soundName)
    {
        if (sfxDictionary.TryGetValue(soundName, out SoundData.Sound sound))
        {
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
        else
        {
            Debug.LogWarning($"효과음 '{soundName}'을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 효과음 재생 (볼륨 오버라이드)
    /// </summary>
    public void Play(string soundName, float volumeScale)
    {
        if (sfxDictionary.TryGetValue(soundName, out SoundData.Sound sound))
        {
            sfxSource.PlayOneShot(sound.clip, sound.volume * volumeScale);
        }
        else
        {
            Debug.LogWarning($"효과음 '{soundName}'을 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    /// <summary>
    /// 현재 BGM 볼륨 가져오기
    /// </summary>
    public float GetBGMVolume() => bgmVolume;

    /// <summary>
    /// 현재 효과음 볼륨 가져오기
    /// </summary>
    public float GetSFXVolume() => sfxVolume;

    /// <summary>
    /// BGM이 재생 중인지 확인
    /// </summary>
    public bool IsBGMPlaying() => bgmSource.isPlaying;
}
