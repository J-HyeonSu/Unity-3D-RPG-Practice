using System;
using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{

    public class PoolManager : MonoBehaviour
    {
        public GameObject[] prefabs;
        private List<GameObject>[] pools;

        private void Awake()
        {
            //pools 리스트배열만들고 초기화
            pools = new List<GameObject>[prefabs.Length];

            for (int index = 0; index < pools.Length; index++)
            {
                pools[index] = new List<GameObject>();
            }
        }

        public GameObject Get(int prefabNum)
        {
            GameObject select = null;

            foreach (var pool in pools[prefabNum])
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
                select = Instantiate(prefabs[prefabNum], transform);
                pools[prefabNum].Add(select);
            }

            return select;
        }
    }
    
    
    
}