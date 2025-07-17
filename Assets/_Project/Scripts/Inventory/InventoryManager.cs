using System;
using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{
    // 인벤토리 관리
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("인벤토리 설정")] 
        [SerializeField] private int maxSlots = 30;

        private List<Item> items = new List<Item>();
        public Item equippedWeapon;
        public Item equippedArmor;
        public Item equippedRing1;
        public Item equippedRing2;
        
        

        public Action<Item> OnItemAdded;
        public Action<Item> OnItemRemoved;
        public Action OnInventoryChanged;

        
        

        private void Awake()
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
        
        public bool AddItem(Item newItem)
        {
            if (newItem?.data == null) return false;
            
            // 바로 사용되는 아이템
            if (newItem.data.itemType == ItemType.Instant)
            {
                // 경험치
                if (newItem.data.exp > 0)
                {
                    PlayerStats.Instance.GainExperience(newItem.data.exp);
                }
                return true;
            }
            
            // 스택 가능한 아이템 찾기
            if (newItem.data.maxStackSize > 1)
            {
                foreach (Item existingItem in items)
                {
                    if (existingItem.CanStackWith(newItem))
                    {
                        int remaining = existingItem.AddToStack(newItem.quantity);
                        newItem.quantity = remaining;
                        
                        if (remaining <= 0)
                        {
                            OnItemAdded?.Invoke(existingItem);
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }
            
            // 새 슬롯에 추가
            if (items.Count < maxSlots)
            {
                items.Add(newItem);
                OnItemAdded?.Invoke(newItem);
                OnInventoryChanged?.Invoke();
                return true;
            }
            
            Debug.Log("인벤토리가 가득 찼습니다!");
            return false;
        }
        
        // 아이템 제거
        public bool RemoveItem(Item item, int quantity = 1)
        {
            if (!items.Contains(item)) return false;
            
            item.quantity -= quantity;
            
            if (item.quantity <= 0)
            {
                items.Remove(item);
                OnItemRemoved?.Invoke(item);
            }
            
            OnInventoryChanged?.Invoke();
            return true;
        }

        // 아이템 장착
        public void EquipItem(Item item)
        {

            Item previousEquipment = null;
            
            switch (item.data.itemType)
            {
                case ItemType.Weapon:
                    previousEquipment = equippedWeapon;
                    equippedWeapon = item;
                    break;
                case ItemType.Armor:
                    previousEquipment = equippedArmor;
                    equippedArmor = item;
                    break;
                case ItemType.Accessory:
                    Debug.Log($"악세사리 장착 시도: {item.data.itemName}");
                    if (!equippedRing1.data)
                    {
                        equippedRing1 = item;
                    }
                    else if (!equippedRing2.data)
                    {
                        equippedRing2 = item;
                    }
                    else
                    {
                        Debug.Log("elsesess");
                        previousEquipment = equippedRing1;
                        equippedRing1 = item;
                    }
                    break;
            }

            if (previousEquipment != null)
            {
                AddItem(previousEquipment);
            }

            RemoveItem(item);
            
            OnInventoryChanged?.Invoke();
        }
        
        // 특정 아이템 개수 확인
        public int GetItemCount(ItemData itemData)
        {
            int total = 0;
            foreach (Item item in items)
            {
                if (item.data == itemData)
                    total += item.quantity;
            }
            return total;
        }
        
        // 인벤토리 아이템 목록 반환
        public List<Item> GetItems()
        {
            return new List<Item>(items);
        }
        
        // 인벤토리 정리
        public void SortInventory()
        {
            items.Sort((a, b) => {
                // 타입별 정렬
                int typeCompare = a.data.itemType.CompareTo(b.data.itemType);
                if (typeCompare != 0) return typeCompare;
                
                // 등급별 정렬
                int rarityCompare = b.data.rarity.CompareTo(a.data.rarity);
                if (rarityCompare != 0) return rarityCompare;
                
                // 이름순 정렬
                return a.data.itemName.CompareTo(b.data.itemName);
            });
            
            OnInventoryChanged?.Invoke();
        }
    }
}