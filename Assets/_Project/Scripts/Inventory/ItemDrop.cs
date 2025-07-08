using System;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace RpgPractice
{
    //월드에 떨어진 아이템
    public class ItemDrop : MonoBehaviour
    {
        [SerializeField] private Item item;
        [SerializeField] private float pickupRange = 2f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.3f;
        [SerializeField] private float rotateSpeed = 50f;

        private Vector3 startPosition;
        private Transform playerTransform;
        private bool canPickup = true;

        private void Start()
        {
            startPosition = transform.position;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
                playerTransform = player.transform;

            // 아이템 색상적용
            if (item?.data != null)
            {
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer)
                    renderer.material.color = item.data.GetRarityColor();
            }
            
        }
        
        
        void Update()
        {
            // 위아래로 둥둥 떠다니는 효과
            transform.position = startPosition + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            
            // 회전 효과
            transform.Rotate(Vector3.up*rotateSpeed*Time.deltaTime);
            
            // 플레이어와의 거리 체크
            if (playerTransform && canPickup)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                
                if (distance <= pickupRange)
                {
                    TryPickup();
                }
            }
        }
        
        // 아이템 설정
        public void SetItem(Item newItem)
        {
            item = newItem;
        }
        
        // 아이템 획득 시도
        void TryPickup()
        {
            if (item?.data == null) return;
            
            bool success = InventoryManager.Instance.AddItem(item);

            //성공시 소리효과, 아이템제거   
            if (success)
            {
                PlayPickupEffect();
                Destroy(gameObject);
            }
            
        }

        
        void PlayPickupEffect()
        {
            Debug.Log($"{item.data.itemName} x{item.quantity} 획득");
        }
        
        // 획득범위 표시
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
        }
    }
}