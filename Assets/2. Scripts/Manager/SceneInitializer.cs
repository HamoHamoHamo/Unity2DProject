using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private List<Tilemap> groundTilemap;
    [SerializeField] private SpawnConfig spawnConfig;
    [SerializeField] private SoundData soundData;

    void Awake()
    {
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

    }
}
