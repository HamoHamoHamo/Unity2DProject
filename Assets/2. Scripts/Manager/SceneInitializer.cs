using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SceneInitializer : MonoBehaviour
{
    [SerializeField] private List<Tilemap> groundTilemap;
    [SerializeField] private SpawnConfig spawnConfig;

    void Awake()
    {
        Managers.Spawn.Initialize(groundTilemap, spawnConfig);
        Managers.Spawn.StartSpawning();

    }
}
