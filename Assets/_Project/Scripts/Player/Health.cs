using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace RpgPractice
{
    public enum StatusEffect
    {
        BLEED,
        POISON,
        GUARD
    }
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100;
        [SerializeField] private float healthRegenRate = 1;
        
        [SerializeField] private FloatEventChannel playerHealthChannel;

        
        
        private HashSet<StatusEffect> activeEffect = new HashSet<StatusEffect>();
        private CountdownTimer bleedTimer;
        private CountdownTimer poisonTimer;
        
        
        private float currentHealth;
        private float healthRegenTimer;

        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            currentHealth = maxHealth;
            healthRegenRate = maxHealth * 0.01f;
        }

        private void Start()
        {
            PublishHealthPercentage();
            bleedTimer = new CountdownTimer(2);
            poisonTimer = new CountdownTimer(2);

            
            bleedTimer.OnTimerStop += () => RemoveStatus(StatusEffect.BLEED);
            poisonTimer.OnTimerStop += () => RemoveStatus(StatusEffect.POISON);
        }

        private void Update()
        {
            bleedTimer.Tick(Time.deltaTime);
            poisonTimer.Tick(Time.deltaTime);

            foreach (var statusEffect in activeEffect)
            {
                Debug.Log(statusEffect);
            }
        }

        private void FixedUpdate()
        {
            
            if (healthRegenTimer > 1f)
            {
                RestoreHealth(healthRegenRate);
                if (activeEffect.Contains(StatusEffect.BLEED))
                {
                    float bleedDamage = maxHealth * 0.04f;
                    Debug.Log($"출혈 : {bleedDamage}");
                    UIManager.instance.CreatePopUp(transform.position, bleedDamage.ToString(), Color.red, 0.7f);
                    currentHealth = Mathf.Clamp(currentHealth - bleedDamage, 0, maxHealth);
                    PublishHealthPercentage();
                }
                healthRegenTimer = 0;
            }

            healthRegenTimer += Time.fixedDeltaTime;
            
        }

        public void AddStatus(int effect, float time=0)
        {
            if (activeEffect.Add((StatusEffect)effect))
            {
                //타이머시작
                if (effect == (int)StatusEffect.POISON)
                {
                    poisonTimer.Reset(time);
                    poisonTimer.Start();
                }
                else if (effect == (int)StatusEffect.BLEED)
                {
                    bleedTimer.Reset(time);
                    bleedTimer.Start();
                }
            }
            else
            {
                //타이머갱신
                if (effect == (int)StatusEffect.POISON)
                {
                    poisonTimer.Reset(time);
                }
                else if (effect == (int)StatusEffect.BLEED)
                {
                    bleedTimer.Reset(time);
                }
            }
            
        }

        
        
        public void RemoveStatus(int effect)
        {
            RemoveStatus((StatusEffect)effect);
        }
        public void RemoveStatus(StatusEffect effect)
        {
            activeEffect.Remove(effect);
        }

        public void RestoreHealth(float amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            PublishHealthPercentage();
        }

        public void TakeDamage(float damage)
        {
            float finalDamage = damage;
            foreach (var statusEffect in activeEffect)
            {
                switch (statusEffect)
                {
                    case StatusEffect.GUARD:
                        finalDamage *= 0.2f;
                        break;
                    case StatusEffect.POISON:
                        finalDamage *= 1.2f;
                        break;
                }    
            }
            
            UIManager.instance.CreatePopUp(transform.position, finalDamage.ToString(), Color.cyan, 1f);
            currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0, maxHealth);
            PublishHealthPercentage();
        }
        

        void PublishHealthPercentage()
        {
            if (playerHealthChannel)
            {
                playerHealthChannel.Invoke(currentHealth/maxHealth);
            }
        }

        public float GetPercentage()
        {
            return currentHealth / maxHealth;
        }

        public void SetMaxHealth(int amount)
        {
            float healthRatio = currentHealth / maxHealth;
            maxHealth = amount;
            currentHealth = maxHealth * healthRatio;
            
            PublishHealthPercentage();
        }
    }
}