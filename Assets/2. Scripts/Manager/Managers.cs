using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static GameObject _root;
    private static PoolManager _pool;
    private static GameManager _game;
    private static UIManager _ui;
    private static SpawnManager _spawn;
    private static TimeSlowManager _timeSlow;
    private static SoundManager _sound;

    private static void Init()
    {
        if (_root == null)
        {
            _root = new GameObject("@Manager");
            Object.DontDestroyOnLoad(_root);
        }
    }

    private static void CreateManager<T>(ref T manager, string name) where T : Component
    {
        if (manager == null)
        {
            Init();
            GameObject obj = new GameObject(name);
            manager = obj.AddComponent<T>();
            Object.DontDestroyOnLoad(obj);
            obj.transform.SetParent(_root.transform);
        }
    }

    public static PoolManager Pool
    {
        get
        {
            CreateManager(ref _pool, "PoolManager");
            return _pool;
        }
    }

    public static GameManager Game
    {
        get
        {
            CreateManager(ref _game, "GameManager");
            return _game;
        }
    }

    public static UIManager UI
    {
        get
        {
            CreateManager(ref _ui, "UIManager");
            return _ui;
        }
    }

    public static SpawnManager Spawn
    {
        get
        {
            CreateManager(ref _spawn, "SpawnManager");
            return _spawn;
        }
    }

    public static TimeSlowManager TimeSlow
    {
        get
        {
            CreateManager(ref _timeSlow, "TimeSlowManager");
            return _timeSlow;
        }
    }

    public static SoundManager Sound
    {
        get
        {
            CreateManager(ref _sound, "SoundManager");
            return _sound;
        }
    }
}
