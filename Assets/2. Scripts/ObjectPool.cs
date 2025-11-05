using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour
{
    private T prefabs;
    private Queue<T> pool = new Queue<T>();

    public Transform Root { get; private set; }

    public ObjectPool(T prefab, int initCount, Transform parent = null)
    {
        this.prefabs = prefab;

        Root = new GameObject($"{prefab.name}_pool").transform;
        Object.DontDestroyOnLoad(Root.gameObject);

        for (int i = 0; i < initCount; i++)
        {
            var inst = Object.Instantiate(prefab, Root);
            inst.name = prefab.name;
            inst.gameObject.SetActive(false);
            pool.Enqueue(inst);
        }
    }

    public T Dequeue()
    {
        if (pool.Count == 0) return null;

        var inst = pool.Dequeue();
        inst.transform.SetParent(null); // ★ 3. 부모를 null로 설정 (풀에서 분리)
        inst.gameObject.SetActive(true);

        return inst;
    }

    public void Enqueue(T instance, bool isActive = false)
    {
        if (instance == null) return;

        instance.transform.SetParent(Root); // ★ 4. 부모를 다시 Root로 설정 (풀에 복귀)
        instance.gameObject.SetActive(isActive);
        pool.Enqueue(instance);
    }
}

