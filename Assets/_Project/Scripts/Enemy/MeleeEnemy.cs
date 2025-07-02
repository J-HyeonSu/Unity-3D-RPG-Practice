using UnityEngine;

namespace RpgPractice
{
    public class MeleeEnemy : Enemy
    {
        [Header("Melee Settings")]
        [SerializeField] private float attackRange = 2f;


        protected override void Start()
        {
            base.Start();
            playerDetector.SetAttackRange(attackRange);
        }

        public override void Attack()
        {
            if (attackTimer.IsRunning) return;
            if (gameObject.GetComponent<Health>().IsDead) return;
            attackTimer.Start();
            playerDetector.PlayerHealth.TakeDamage(attackDamage);
            
        }
    }
}