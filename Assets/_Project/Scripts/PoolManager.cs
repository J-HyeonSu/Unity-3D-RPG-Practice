using System;
using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{

    public class PoolManager : MonoBehaviour
    {
        // public GameObject[] prefabs;
        // private List<GameObject>[] pools;
        public static PoolManager instance;
        private Dictionary<GameObject, List<GameObject>> pools;

        private void Awake()
        {
            instance = this;
            pools = new Dictionary<GameObject, List<GameObject>>();
            // //pools 리스트배열만들고 초기화
            // pools = new List<GameObject>[prefabs.Length];
            //
            // for (int index = 0; index < pools.Length; index++)
            // {
            //     pools[index] = new List<GameObject>();
            // }
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