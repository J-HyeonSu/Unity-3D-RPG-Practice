﻿using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RpgPractice
{
    public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image backGroundImage;
        [SerializeField] private Image rarityBorder;

        private Item currentItem;
        private int slotIndex;
        private InventoryUI parentInventoryUI;

        public void Initialize(int index, InventoryUI inventoryUI)
        {
            slotIndex = index;
            parentInventoryUI = inventoryUI;
            ClearSlot();
        }

        public void SetItem(Item item)
        {
            currentItem = item;

            if (item?.data != null)
            {
                // 아이템 아이콘 설정
                itemIcon.sprite = item.data.icon;
                itemIcon.color = Color.white;
                itemIcon.gameObject.SetActive(true);

                // 수량 표기
                if (item.quantity > 1)
                {
                    quantityText.text = item.quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
                
                // 등급별 테두리 색상
                if (rarityBorder != null)
                {
                    var color = item.data.GetRarityColor();
                    color.a = 0.5f;
                    rarityBorder.color = color;
                    
                    rarityBorder.gameObject.SetActive(true);
                }
                
            }
            else
            {
                ClearSlot();
            }
        }

        public void ClearSlot()
        {
            currentItem = null;
            itemIcon.gameObject.SetActive(false);
            quantityText.gameObject.SetActive(false);

            if (rarityBorder != null)
            {
                rarityBorder.gameObject.SetActive(false);
            }
        }
        
        // 아이템 호버 시 아이템 정보 표시
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null && parentInventoryUI != null)
            {
                parentInventoryUI.ShowItemInfo(currentItem, eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (parentInventoryUI != null)
            {
                parentInventoryUI.HideItemInfo();
            }
        }

        // 마우스 클릭 시 아이템 사용
        public void OnPointerClick(PointerEventData eventData)
        {
            
            if (currentItem != null && parentInventoryUI != null)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    // 좌클릭: 아이템 사용
                    
                    // 장착된아이템일경우
                    if (slotIndex >= 100)
                    {
                        Debug.Log("장착해제");
                        InventoryManager.Instance.UnEquipItem(slotIndex);
                    }
                    else
                    {
                        parentInventoryUI.UseItem(currentItem);    
                    }
                    
                }
                else if (eventData.button == PointerEventData.InputButton.Right)
                {
                    // 우클릭
                    Debug.Log($"[{currentItem.data.rarity}]{currentItem.data.itemName} 우클릭");
                }
            }
        }

        public Item GetItem()
        {
            return currentItem;
        }
    }
}