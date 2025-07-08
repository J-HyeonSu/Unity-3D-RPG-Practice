using System;
using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{

    public class PoolManager : MonoBehaviour
    {
        public static PoolManager instance;
        private Dictionary<GameObject, List<GameObject>> pools;

        private void Awake()
        {
            instance = this;
            pools = new Dictionary<GameObject, List<GameObject>>();
        }

        public GameObject Get(ProjectileData projectileData)
        {
            return Get(projectileData.prefab);
        }
        public GameObject Get(GameObject prefab)
        {
            
            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new List<GameObject>();
            }

            GameObject select = null;

            foreach (var pool in pools[prefab])
            {
                if (!pool.activeSelf)
                {
                    select = pool;
                    select.SetActive(true);
                    break;
                }
            }

            if (!select)
            {
                select = Instantiate(prefab, transform);
                pools[prefab].Add(select);
            }

            return select;
        }
    }
    
    
    
}