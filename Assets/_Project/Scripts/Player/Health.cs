using System;
using UnityEngine;

namespace RpgPractice
{
    public class Health : MonoBehaviour
    {

        [SerializeField] private float maxHealth = 100;
        [SerializeField] private FloatEventChannel playerHealthChannel;
        
        private float currentHealth;

        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            PublishHealthPercentage();
        }

        public void TakeDamage(float damage)
        {
            DamagePopUpGenerator.current.CreatePopUp(transform.position, damage.ToString(), Color.cyan);
            currentHealth -= damage;
            PublishHealthPercentage();
        }

        void PublishHealthPercentage()
        {
            if (playerHealthChannel)
            {
                playerHealthChannel.Invoke(currentHealth/maxHealth);
            }
        }
    }
}