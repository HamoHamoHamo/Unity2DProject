using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering;

public class SceneInitializer : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private List<Tilemap> groundTilemap;

    [Header("Configs")]
    [SerializeField] private SpawnConfig spawnConfig;
    [SerializeField] private SoundData soundData;

    [Header("Effects")]
    [SerializeField] private Volume slowMoVolume;

    [Header("UI")]
    [SerializeField] private UIManager uiManager;

    void Awake()
    {
        Debug.Log("SceneInitializer Awake");
        // SoundManager 초기화
        if (soundData != null)
        {
            Managers.Sound.Initialize(soundData);
            Managers.Sound.PlayBGM("Main");
        }

        // SpawnManager 초기화
        if (spawnConfig != null)
        {
            Managers.Spawn.Initialize(groundTilemap, spawnConfig);
            Managers.Spawn.StartSpawning();
        }

        if (slowMoVolume != null)
        {
            Managers.TimeSlow.InitializeVolume(slowMoVolume);
        }

        // 게임 시작
        Managers.Game.Initialize(uiManager);
        Managers.Game.StartGame();
    }
}
