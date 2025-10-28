using UnityEngine;

[CreateAssetMenu(fileName = "SpawnConfig", menuName = "Config/Spawn Config")]
public class SpawnConfig : ScriptableObject
{
    [Header("Enemy Settings")]
    public Enemy enemyPrefab;
    public int enemyPoolSize = 20;
    public float enemySpawnInterval = 2f;
    public int maxEnemies = 10;

    [Header("ThrowableItem Settings")]
    public ThrowableItem throwableItemPrefab;
    public int throwableItemPoolSize = 10;
    public float throwableItemSpawnInterval = 5f;
    public int maxThrowableItems = 3;

    [Header("General Settings")]
    public float minSpawnDistance = 3f;
}