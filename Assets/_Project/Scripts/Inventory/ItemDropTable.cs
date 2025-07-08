using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{
    
    // 드롭테이블 설정 ScriptalbeObject
    [CreateAssetMenu(fileName = "New Drop Table", menuName = "RPG/Item Drop Table")]
    public class ItemDropTable : ScriptableObject
    {

        [System.Serializable]
        public class DropEntry
        {
            public ItemData item;
            [Range(0f, 1f)] public float dropChance = 0.1f;
            public int minQuantity = 1;
            public int maxQuantity = 1;

            
            public bool ShouldDrop()
            {
                return Random.Range(0f, 1f) <= dropChance;
            }

            public int GetRandomQuantity()
            {
                return Random.Range(minQuantity, maxQuantity + 1);
            }
        }

        [Header("드롭 설정")] 
        public List<DropEntry> dropEntries = new List<DropEntry>();
        [Range(0, 100)] public int guaranteedDropCount = 0; // 보장 드롭개수
        [Range(0, 100)] public int maxDropCount = 3; // 최대 드롭개수

        public List<Item> GenerateDrops()
        {
            List<Item> drops = new List<Item>();
            List<DropEntry> availableDrops = new List<DropEntry>(dropEntries);

            // 보장 드롭 처리
            for (int i = 0; i < guaranteedDropCount && availableDrops.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableDrops.Count);
                var entry = availableDrops[randomIndex];

                Item newItem = new Item(entry.item, entry.GetRandomQuantity());
                drops.Add(newItem);
                
                //중복드롭방지?
                availableDrops.RemoveAt(randomIndex);
            }

            // 확률 드롭 처리
            foreach (var entry in dropEntries)
            {
                if (drops.Count >= maxDropCount) break;
                if (entry.ShouldDrop())
                {
                    // 개수 랜덤 드롭
                    Item newItem = new Item(entry.item, entry.GetRandomQuantity());
                    drops.Add(newItem);
                }
            }

            return drops;
        }

    }
}