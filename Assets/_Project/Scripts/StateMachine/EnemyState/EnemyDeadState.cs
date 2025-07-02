using UnityEngine;

namespace RpgPractice
{
    public class EnemyDeadState : EnemyBaseState
    {
        public EnemyDeadState(Enemy enemy, Animator animator) : base(enemy, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(DeadHash, crossFadeDuration);
            enemy.Die();
        }
        
    }
}