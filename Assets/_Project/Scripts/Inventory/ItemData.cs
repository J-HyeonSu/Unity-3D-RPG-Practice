using UnityEngine;

namespace RpgPractice
{
    //아이템 기본정보 SriptableObject 클래스
    [CreateAssetMenu(fileName = "New Item", menuName = "RPG/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("기본 정보")] 
        public string itemName = "새 아이템";
        public string description = "아이템 설명";
        public Sprite icon;
        public GameObject prefab;

        [Header("아이템 속성")] 
        public ItemType itemType = ItemType.Material;
        public ItemRarity rarity = ItemRarity.Common;
        public int maxStackSize = 1;
        public int sellPrice = 10;

        [Header("스탯 (무기/방어구용")] 
        public int attackPower = 0;
        public int defense = 0;
        public int healthBonus = 0;
        public int manaBonus = 0;

        [Header("소모품 효과")] 
        public int healAmount = 0;
        public int manaRestoreAmount = 0;
        public int exp = 0;

        public Color GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => Color.blue,
                ItemRarity.Epic => new Color(0.5f, 0f, 1f),
                ItemRarity.Legendary => Color.yellow,
                _ => Color.white
            };
        }

        public float GetDropChance()
        {
            return rarity switch
            {
                ItemRarity.Common => 0.6f,
                ItemRarity.Uncommon => 0.25f,
                ItemRarity.Rare => 0.1f,
                ItemRarity.Epic => 0.04f,
                ItemRarity.Legendary => 0.01f,
                _=> 0.6f,
            };
        }

    }
}