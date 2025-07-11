using System;
using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{

    // 풀매니저 - 오브젝트풀링 관련 통합매니저
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
            // 해당프리팹을 처음 생성할때 게임오브젝트 리스트 생성
            if (!pools.ContainsKey(prefab))
            {
                
                pools[prefab] = new List<GameObject>();
            }

            GameObject select = null;

            foreach (var pool in pools[prefab])
            {
                // 해당프리팹의 게임오브젝트리스트 안에 비활성화된 게임오브젝트가 있을경우 선택
                if (!pool.activeSelf)
                {
                    select = pool;
                    select.SetActive(true);
                    break;
                }
            }

            // 비활성화된 게임오브젝트가 없을경우 새로 생성
            if (!select)
            {
                select = Instantiate(prefab, transform);
                pools[prefab].Add(select);
            }
            
            return select;
        }
    }
}