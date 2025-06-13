using UnityEngine;

namespace RpgPractice
{
    public abstract class EnemyBaseState : IState
    {
        protected readonly Enemy enemy;
        protected readonly Animator animator;

        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int DamageHash = Animator.StringToHash("Damage0");
        protected static readonly int AttackHash = Animator.StringToHash("Attack");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");
        protected static readonly int DeadHash = Animator.StringToHash("Dead");
        
        protected const float crossFadeDuration = 0.1f;
    
        protected EnemyBaseState(Enemy enemy, Animator animator)
        {
            this.enemy = enemy;
            this.animator = animator;
        }
        public virtual void OnEnter()
        {
            
        }

        public virtual void Update()
        {

        }

        public virtual void FixedUpdate()
        {
   
        }

        public virtual void OnExit()
        {
            
        }
    }
}