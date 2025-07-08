using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{
    // 아이템 드롭 관리
    public class ItemDropManager : MonoBehaviour
    {
        public static ItemDropManager Instance { get; private set; }

        [Header("드롭 설정")] 
        [SerializeField] private GameObject defaultItemDropPrefab;
        [SerializeField] private float dropForce = 5f;
        [SerializeField] private float dropRadius = 2f;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // 특정위치에 아이템 드롭
        public void DropItem(Item item, Vector3 position)
        {
            if (item?.data == null) return;

            // 드롭 위치 계산 (범위 내 랜덤)
            Vector3 randomOffset = new Vector3(
                Random.Range(-dropRadius, dropRadius), 1, Random.Range(-dropRadius, dropRadius));
            Vector3 dropPosition = position + randomOffset;

            // 아이템 프리펩 결정
            GameObject prefabToUse = item.data.prefab ? item.data.prefab : defaultItemDropPrefab;

            // 아이템 생성
            GameObject droppedItem = Instantiate(prefabToUse, dropPosition, Quaternion.identity);

            // ItemDrop 컴포넌트 설정
            ItemDrop dropComponent = droppedItem.GetComponent<ItemDrop>();
            if (dropComponent == null)
                dropComponent = droppedItem.AddComponent<ItemDrop>();
            dropComponent.SetItem(item);

            // 물리적 효과 추가 (rb가 있다면)
            Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
            if (rb)
            {
                Vector3 randomDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized;
                
                rb.AddForce(randomDirection * dropForce, ForceMode.Impulse);
            }
        }

        // 드롭 테이블 기반 아이템드롭
        public void DropFromTable(ItemDropTable dropTable, Vector3 position)
        {
            if (dropTable == null) return;

            List<Item> drops = dropTable.GenerateDrops();

            foreach (var item in drops)
            {
                DropItem(item, position);
            }
            
        }
        
        // 여러 아이템 동시드롭
        public void DropItems(List<Item> items, Vector3 position)
        {
            foreach (Item item in items)
            {
                DropItem(item, position);
            }
        }
        
    }
}