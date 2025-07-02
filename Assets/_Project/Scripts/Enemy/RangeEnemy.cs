using UnityEngine;

namespace RpgPractice
{
    public class RangeEnemy : Enemy
    {
        [Header("Range Settings")]
        [SerializeField] private float attackRange = 10f;
        [SerializeField] protected ProjectileData projectileData;

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
            if (!projectileData) return;
            var proj = PoolManager.instance.Get(projectileData.prefab);
            var direction = playerDetector.Player.position - transform.position;
            if(gameObject)
                proj.GetComponentInChildren<Projectile>().Init(gameObject, transform.position, direction, projectileData, attackDamage);
            
        }
    }
}