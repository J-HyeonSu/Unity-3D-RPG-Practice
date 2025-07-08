using UnityEngine;

namespace RpgPractice
{
    
    //실제 아이템 인스턴스
    [System.Serializable]
    public class Item
    {
        public ItemData data;
        public int quantity = 1; // 갯수

        public Item(ItemData itemData, int qty = 1)
        {
            data = itemData;
            quantity = Mathf.Clamp(qty, 1, itemData.maxStackSize);
        }

        //아이템 스택가능 여부 확인
        public bool CanStackWith(Item other)
        {
            return data == other.data &&
                   quantity < data.maxStackSize &&
                   other.quantity < other.data.maxStackSize;
        }

        // 아이템 합치기(스택)
        public int AddToStack(int amount)
        {
            int availableSpace = data.maxStackSize - quantity;
            int actualAdd = Mathf.Min(amount, availableSpace);
            quantity += actualAdd;
            return amount - actualAdd; // 남은개수 반환
        }
    }
}