using System;
using UnityEngine;

namespace RpgPractice
{

    public class Mana : MonoBehaviour
    {


        [SerializeField] private float maxMana = 100;
        [SerializeField] private float manaRegenRate = 2;
        
        [SerializeField] private FloatEventChannel playerManaChannel;
        
        private float currentMana;
        private float manaRegenTimer;

        private void Awake()
        {
            currentMana = maxMana;
        }

        private void Start()
        {
            PublishManaPercentage();

        }

        void FixedUpdate()
        {
            RegenerateMana();
        }
        
        public bool UseMana(float cost)
        {
            if (currentMana >= cost)
            {
                currentMana -= cost;
                PublishManaPercentage();
                return true;
            }
            
            return false;
        }


        public void RestoreMana(float amount)
        {
            currentMana = Mathf.Clamp(currentMana+amount, 0, maxMana);
            PublishManaPercentage();
        }

        private void RegenerateMana()
        {
            if (manaRegenTimer > 1f)
            {
                RestoreMana(manaRegenRate);
                manaRegenTimer = 0;
            }

            manaRegenTimer += Time.fixedDeltaTime;
            
        }
        

        void PublishManaPercentage()
        {
            if (playerManaChannel)
            {
                playerManaChannel.Invoke(currentMana/maxMana);
            }
        }
    }
}