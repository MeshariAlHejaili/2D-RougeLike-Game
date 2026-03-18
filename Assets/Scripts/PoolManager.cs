using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Header("Singleton Instance")]
    [Tooltip("The static reference to this manager for global access.")]
    public static PoolManager Instance { get; private set; }

    // Internal dictionary to manage various object pools
    private Dictionary<GameObject, List<GameObject>> m_Pools = new Dictionary<GameObject, List<GameObject>>();

    private void Awake()
    {
        // Enforce the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Creates a set number of objects upfront to avoid performance spikes
    public void PrewarmPool(GameObject prefab, int amount)
    {
        if (prefab == null) return;

        // Create a new list for this prefab if it doesn't exist yet
        if (!m_Pools.ContainsKey(prefab))
        {
            m_Pools[prefab] = new List<GameObject>();
        }

        // Instantiate the requested amount and set them to inactive
        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            m_Pools[prefab].Add(obj);
        }
    }

    // Retrieves an available object from the pool; creates one if none are available
    public GameObject Get(GameObject prefab)
    {
        // Ensure the pool exists for this prefab
        if (!m_Pools.ContainsKey(prefab))
        {
            PrewarmPool(prefab, 1);
        }

        // Search for an inactive object to recycle
        foreach (GameObject obj in m_Pools[prefab])
        {
            if (!obj.activeInHierarchy)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // Fallback: If pool is empty, create a new instance
        GameObject newObj = Instantiate(prefab);
        m_Pools[prefab].Add(newObj);
        return newObj;
    }

    // Deactivates an object to "return" it to its pool
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}