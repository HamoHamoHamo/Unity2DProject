using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnManager : MonoBehaviour
{
    private List<Tilemap> walkableTilemaps = new List<Tilemap>();

    private List<Enemy> enemyPrefabs;
    private int enemyPoolSize;
    private float enemySpawnInterval;
    private int maxEnemies;

    private ThrowableItem throwableItemPrefab;
    private int throwableItemPoolSize;
    private float throwableItemSpawnInterval;
    private int maxThrowableItems;

    private Bullet bulletPrefab;
    private int bulletPoolSize;

    private float minSpawnDistance;

    private List<Vector3Int> walkableTiles = new List<Vector3Int>();
    private Transform playerTransform;
    private float enemySpawnTimer;
    private float throwableItemSpawnTimer;

    private bool isInitialized = false;
    private bool isSpawning = false;

    public void Initialize(List<Tilemap> tilemaps, SpawnConfig config)

    {
        Debug.Log($"isInitialized {isInitialized}");
        if (isInitialized) return;

        if (config == null)
        {
            Debug.LogError("[SpawnManager] SpawnConfig가 null입니다!");
            return;
        }

        if (tilemaps == null || tilemaps.Count == 0)
        {
            Debug.LogError("[SpawnManager] Tilemap 리스트가 비어있습니다!");
            return;
        }

        walkableTilemaps = tilemaps;
        enemyPrefabs = config.enemyPrefabs;
        throwableItemPrefab = config.throwableItemPrefab;
        bulletPrefab = config.bulletPrefab;

        enemyPoolSize = config.enemyPoolSize;
        throwableItemPoolSize = config.throwableItemPoolSize;
        bulletPoolSize = config.bulletPoolSize;
        enemySpawnInterval = config.enemySpawnInterval;
        throwableItemSpawnInterval = config.throwableItemSpawnInterval;
        maxEnemies = config.maxEnemies;
        maxThrowableItems = config.maxThrowableItems;
        minSpawnDistance = config.minSpawnDistance;

        if (walkableTilemaps == null)
        {
            Debug.LogError("[SpawnManager] Tilemap이 null입니다!");
            return;
        }

        // Player 찾기
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogWarning("[SpawnManager] Player를 찾을 수 없습니다.");
        }

        // 이동 가능한 타일 캐싱
        CacheWalkableTiles();

        // PoolManager에 풀 생성 요청
        if (enemyPrefabs != null)
        {
            foreach (Enemy enemyPrefab in enemyPrefabs)
            {
                Managers.Pool.CreatePool(enemyPrefab, enemyPoolSize);
            }
            Debug.Log($"[SpawnManager] Enemy Pool 생성 완료 (크기: {enemyPoolSize})");
        }

        if (throwableItemPrefab != null)
        {
            Managers.Pool.CreatePool(throwableItemPrefab, throwableItemPoolSize);
            Debug.Log($"[SpawnManager] throwableItem Pool 생성 완료 (크기: {throwableItemPoolSize})");
        }

        if (bulletPrefab != null)
        {
            Managers.Pool.CreatePool(bulletPrefab, bulletPoolSize);
            Debug.Log($"[SpawnManager] Bullet Pool 생성 완료 (크기: {bulletPoolSize})");
        }

        isInitialized = true;
    }

    void Update()
    {
        Debug.Log($"Test {isSpawning} {playerTransform}");
        if (!isInitialized || !isSpawning || playerTransform == null) return;

        // 적 스폰 타이머
        enemySpawnTimer += Time.deltaTime;
        if (enemySpawnTimer >= enemySpawnInterval)
        {
            Debug.Log(CountActiveObjects("Enemy"));
            Debug.Log(maxEnemies);
            if (CountActiveObjects("Enemy") < maxEnemies)
            {
                SpawnEnemy();
            }
            enemySpawnTimer = 0f;
        }

        // 아이템 스폰 타이머
        throwableItemSpawnTimer += Time.deltaTime;
        if (throwableItemSpawnTimer >= throwableItemSpawnInterval)
        {
            if (CountActiveObjects("ThrowableItem") < maxThrowableItems)
            {
                SpawnItem();
            }
            throwableItemSpawnTimer = 0f;
        }
    }

    /// <summary>
    /// 모든 타일맵에서 이동 가능한 타일 위치를 캐싱
    /// </summary>
    void CacheWalkableTiles()
    {
        walkableTiles.Clear();

        // 각 타일맵을 순회
        foreach (Tilemap tilemap in walkableTilemaps)
        {
            if (tilemap == null)
            {
                Debug.LogWarning("[SpawnManager] null인 Tilemap이 있습니다!");
                continue;
            }

            BoundsInt bounds = tilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);

                    if (tilemap.HasTile(tilePos))
                    {
                        // 중복 체크 (다른 타일맵에 같은 위치 타일이 있을 수 있음)
                        if (!walkableTiles.Contains(tilePos))
                        {
                            walkableTiles.Add(tilePos);
                        }
                    }
                }
            }
        }

        Debug.Log($"[SpawnManager] {walkableTiles.Count}개의 스폰 가능한 타일을 찾았습니다.");
    }

    /// <summary>
    /// 적 스폰
    /// </summary>
    void SpawnEnemy()
    {
        Debug.Log("SpawnEnemy");
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

        // 랜덤으로 Enemy 타입 선택
        Enemy selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

        MonoBehaviour enemy = Managers.Pool.GetFromPool(selectedEnemyPrefab);

        if (enemy != null)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            enemy.transform.position = spawnPos;
            enemy.transform.rotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// 플레이어와 일정 거리 이상 떨어진 랜덤 위치 반환
    /// </summary>
    Vector3 GetRandomSpawnPosition()
    {
        if (walkableTiles.Count == 0)
        {
            Debug.LogWarning("[SpawnManager] 스폰 가능한 타일이 없습니다!");
            return Vector3.zero;
        }

        int maxAttempts = 50;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3Int randomTile = walkableTiles[Random.Range(0, walkableTiles.Count)];

            // 해당 타일이 속한 타일맵 찾기
            Tilemap ownerTilemap = GetTilemapContainingTile(randomTile);
            if (ownerTilemap == null) continue;

            Vector3 worldPos = ownerTilemap.GetCellCenterWorld(randomTile);

            // 플레이어와 거리 체크
            if (playerTransform != null)
            {
                if (Vector3.Distance(worldPos, playerTransform.position) >= minSpawnDistance)
                {
                    return worldPos;
                }
            }
            else
            {
                return worldPos;
            }
        }

        // 실패 시 랜덤 위치 반환
        Vector3Int fallbackTile = walkableTiles[Random.Range(0, walkableTiles.Count)];
        Tilemap fallbackTilemap = GetTilemapContainingTile(fallbackTile);

        if (fallbackTilemap != null)
        {
            return fallbackTilemap.GetCellCenterWorld(fallbackTile);
        }

        return Vector3.zero;
    }

    /// <summary>
    /// 특정 타일 위치를 포함하는 타일맵 찾기
    /// </summary>
    Tilemap GetTilemapContainingTile(Vector3Int tilePos)
    {
        foreach (Tilemap tilemap in walkableTilemaps)
        {
            if (tilemap != null && tilemap.HasTile(tilePos))
            {
                return tilemap;
            }
        }
        return null;
    }


    /// <summary>
    /// 아이템 스폰 (습득 가능한 투사체)
    /// </summary>
    void SpawnItem()
    {
        if (throwableItemPrefab == null) return;

        MonoBehaviour item = Managers.Pool.GetFromPool(throwableItemPrefab);

        if (item != null)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            item.transform.position = spawnPos;
            item.transform.rotation = Quaternion.identity;
        }
    }


    int CountActiveObjects(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        int count = 0;

        foreach (GameObject obj in objects)
        {
            if (obj.activeInHierarchy)
                count++;
        }

        return count;
    }

    public void StartSpawning()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[SpawnManager] Initialize()를 먼저 호출해주세요!");
            return;
        }

        isSpawning = true;
        ResetTimers();
        Debug.Log("[SpawnManager] 스폰 시작");
    }

    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("[SpawnManager] 스폰 중지");
    }

    public void ResetTimers()
    {
        enemySpawnTimer = 0f;
        throwableItemSpawnTimer = 0f;
    }

}
