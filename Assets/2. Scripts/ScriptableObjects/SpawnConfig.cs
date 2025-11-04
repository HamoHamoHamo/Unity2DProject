using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnConfig", menuName = "ScriptableObjects/Spawn Config")]
public class SpawnConfig : ScriptableObject
{
    [Header("Enemy Settings")]
    public List<Enemy> enemyPrefabs;
    public int enemyPoolSize = 20;
    public float enemySpawnInterval = 3f;
    public int maxEnemies = 10;

    [Header("ThrowableItem Settings")]
    public ThrowableItem throwableItemPrefab;
    public int throwableItemPoolSize = 10;
    public float throwableItemSpawnInterval = 5f;
    public int maxThrowableItems = 3;

    [Header("Bullet Settings")]
    public Bullet bulletPrefab;
    public int bulletPoolSize = 50;

    [Header("General Settings")]
    public float minSpawnDistance = 4f;
}