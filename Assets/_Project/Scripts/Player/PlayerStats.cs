using System;
using UnityEngine;

namespace RpgPractice
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("레벨 & 경험치")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentExp = 0;
        [SerializeField] private int expToNextLevel = 100;
        
        [Header("기본 스탯")]
        [SerializeField] private int baseAttackPower = 10;
        [SerializeField] private int baseDefense = 5;
        [SerializeField] private int baseMaxHealth = 100;
        [SerializeField] private int baseMaxMana = 50;
        
        [Header("레벨업 증가량")]
        [SerializeField] private int attackPerLevel = 2;
        [SerializeField] private int defensePerLevel = 1;
        [SerializeField] private int healthPerLevel = 15;
        [SerializeField] private int manaPerLevel = 10;

        [Header("이벤트 채널")] 
        [SerializeField] private IntEventChannel levelUpEvent;
        [SerializeField] private FloatEventChannel expUpdateEvent;
        //[SerializeField] private FloatEventChannel healthUpdateEvent;
        //[SerializeField] private FloatEventChannel manaUpdateEvent;

        private int bonusAttack = 0;
        private int bonusDefense = 0;
        private int bonusHealth = 0;
        private int bonusMana = 0;

        public int Level => currentLevel;
        public int CurrentExp => currentExp;
        public int ExpToNextLevel => expToNextLevel;
        public float ExpProgress => (float)currentExp / expToNextLevel;

        public int TotalAttackPower => baseAttackPower + (attackPerLevel * (currentLevel - 1)) + bonusAttack;
        public int TotalDefense => baseDefense + (defensePerLevel * (currentLevel - 1)) + bonusDefense;
        public int TotalMaxHealth => baseMaxHealth + (healthPerLevel * (currentLevel - 1)) + bonusHealth;
        public int TotalMaxMana => baseMaxMana + (manaPerLevel * (currentLevel - 1)) + bonusMana;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            UpdateHealthAndMana();
            expUpdateEvent?.Invoke(ExpProgress);
            levelUpEvent?.Invoke(currentLevel);
        }

        public void GainExperience(int amount)
        {
            currentExp += amount;

            while (currentExp >= expToNextLevel)
            {
                LevelUp();
            }
            
            expUpdateEvent?.Invoke(ExpProgress);
        }

        void LevelUp()
        {
            currentExp -= expToNextLevel;
            currentLevel++;

            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.2f);

            UpdateHealthAndMana();
            
            levelUpEvent?.Invoke(currentLevel);
            Debug.Log($"레벨업! 레벨 {currentLevel} 달성!");
            Debug.Log($"공격력: {TotalAttackPower}, 방어력: {TotalDefense}");
        }

        private void UpdateHealthAndMana()
        {
            var health = GetComponent<Health>();
            var mana = GetComponent<Mana>();

            if (health)
            {
                health.SetMaxHealth(TotalMaxHealth);
            }

            if (mana)
            {
                mana.SetMaxMana(TotalMaxMana);
            }
        }

        public void UpdateItemBonuses(int attack, int defense, int health, int mana)
        {
            bonusAttack = attack;
            bonusDefense = defense;
            bonusHealth = health;
            bonusMana = mana;
            
            UpdateHealthAndMana();
            
        }
    }
}