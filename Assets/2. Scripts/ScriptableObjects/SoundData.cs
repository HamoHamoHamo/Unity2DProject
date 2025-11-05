using UnityEngine;

/// <summary>
/// 사운드 데이터를 관리하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "SoundData", menuName = "ScriptableObjects/SoundData", order = 1)]
public class SoundData : ScriptableObject
{
    [System.Serializable]
    public class Sound
    {
        public string name;         // 사운드 이름
        public AudioClip clip;      // 오디오 클립
        [Range(0f, 1.5f)]
        public float volume = 1f;   // 볼륨
    }

    [Header("BGM Settings")]
    public Sound[] bgmSounds;
    [Range(0f, 1f)]
    public float defaultBGMVolume = 0.3f;

    [Header("SFX Settings")]
    public Sound[] sfxSounds;
    [Range(0f, 1f)]
    public float defaultSFXVolume = 0.7f;

    [Header("Slow BGM Settings")]
    public Sound[] slowMotionSounds;
}
