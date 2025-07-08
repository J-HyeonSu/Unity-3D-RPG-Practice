using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;


namespace RpgPractice
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI 참조")] 
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform itemSlotParent;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private Button sortButton;
        [SerializeField] private Button closeButton;

        [Header("아이템 정보 패널")]
        [SerializeField] private GameObject itemInfoPanel;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TextMeshProUGUI itemStatsText;

        private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();
        private bool isInventoryOpen = false;

        private void Start()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
            }

            if (sortButton != null)
            {
                sortButton.onClick.AddListener(SortInventory);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInventory);
            }

            CreateItemSlots();
            inventoryPanel.SetActive(false);
            itemInfoPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }
        }

        private void OnDestroy()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
            }
        }
        
        // 아이템 슬롯 생성
        void CreateItemSlots()
        {
            int maxSlots = 30;

            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObject = Instantiate(itemSlotPrefab, itemSlotParent);
                ItemSlotUI slotUI = slotObject.GetComponent<ItemSlotUI>();

                if (slotUI != null)
                {
                    slotUI.Initialize(i, this);
                    itemSlots.Add(slotUI);
                }
            }
        }

        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);

            if (isInventoryOpen)
            {
                RefreshInventory();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                HideItemInfo();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void CloseInventory()
        {
            isInventoryOpen = false;
            inventoryPanel.SetActive(false);
            HideItemInfo();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // 인벤토리 새로고침
        void RefreshInventory()
        {
            if (InventoryManager.Instance == null) return;

            List<Item> items = InventoryManager.Instance.GetItems();
            
            // 모든 슬롯 초기화
            foreach (ItemSlotUI slot in itemSlots)
            {
                slot.ClearSlot();
            }

            //아이템 배치
            for (int i = 0; i < items.Count && i < itemSlots.Count; i++)
            {
                itemSlots[i].SetItem(items[i]);
            }
        }

        void SortInventory()
        {
            InventoryManager.Instance?.SortInventory();
        }
        
        // 아이템 정보 표시
        public void ShowItemInfo(Item item, Vector3 position)
        {
            if (item?.data == null)
            {
                HideItemInfo();
                return;
            }
            
            itemInfoPanel.SetActive(true);
            
            // 아이템 정보 설정
            itemNameText.text = item.data.itemName;
            itemNameText.color = item.data.GetRarityColor();
            Debug.Log(item.data.itemName);

            itemDescriptionText.text = item.data.description;
            itemIconImage.sprite = item.data.icon;
            
            // 스탯 정보 구성
            string statsText = "";

            if (item.data.attackPower > 0)
                statsText += $"공격력: +{item.data.attackPower}\n";

            if (item.data.defense > 0)
                statsText += $"방어력: +{item.data.defense}\n";

            if (item.data.healthBonus > 0)
                statsText += $"체력: +{item.data.healthBonus}\n";

            if (item.data.manaBonus > 0)
                statsText += $"마나: +{item.data.manaBonus}\n";

            if (item.data.healAmount > 0)
                statsText += $"체력 회복: {item.data.healAmount}\n";
            
            if (item.data.manaRestoreAmount > 0)
                statsText += $"마나 회복: {item.data.manaRestoreAmount}\n";

            statsText += $"\n등급: {GetRarityText(item.data.rarity)}";
            statsText += $"\n판매가: {item.data.sellPrice}골드";

            if (item.quantity > 1)
                statsText += $"\n개수: {item.quantity}";

            itemStatsText.text = statsText;

            
            
            // 위치 조절 (마우스근처 표시)
            RectTransform rectTransform = itemInfoPanel.GetComponent<RectTransform>();

            
            // position.x = Mathf.Clamp(position.x, -540, 540);
            // position.y = Mathf.Clamp(position.y, -140, 140);
            
            rectTransform.position = position;
            
            var clampX = Mathf.Clamp(rectTransform.anchoredPosition.x, -540, 540);
            var clampY = Mathf.Clamp(rectTransform.anchoredPosition.y, -140, 140);

            rectTransform.anchoredPosition = new Vector2(clampX, clampY);
            Debug.Log(position);
            Debug.Log(rectTransform.anchoredPosition);
        }

        public void HideItemInfo()
        {
            itemInfoPanel.SetActive(false);
        }

        string GetRarityText(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => "일반",
                ItemRarity.Uncommon => "고급",
                ItemRarity.Rare => "희귀",
                ItemRarity.Epic => "영웅",
                ItemRarity.Legendary => "전설",
                _ => "일반"
            };
        }

        public void UseItem(Item item)
        {
            if (item?.data == null) return;

            switch (item.data.itemType)
            {
                case ItemType.Consumable:
                    UseConsumableItem(item);
                    break;
                case ItemType.Armor:
                case ItemType.Weapon:
                    // 장비시스템 구현시 추가
                    break;
                default:
                    Debug.Log($"{item.data.itemName}은(는) 사용할 수 없습니다.");
                    break;
            }
        }

        void UseConsumableItem(Item item)
        {
            // 소모 아이템 사용시 
            // 체력, 마나 적용

            InventoryManager.Instance.RemoveItem(item, 1);
        }
        
    }

}
