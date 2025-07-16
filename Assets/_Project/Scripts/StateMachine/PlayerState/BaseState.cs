using UnityEngine;

namespace RpgPractice
{
    public abstract class BaseState : IState
    {
        protected readonly PlayerController player;
        protected readonly Animator animator;

        protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");
        protected static readonly int RunHash = Animator.StringToHash("Run");
        protected static readonly int LeftAttack1Hash = Animator.StringToHash("LeftAttack1");
        protected static readonly int LeftAttack2Hash = Animator.StringToHash("LeftAttack2");
        protected static readonly int RightAttackHash = Animator.StringToHash("RightAttack");
        protected static readonly int BlockHash = Animator.StringToHash("Block");
        protected static readonly int RushAttackHash = Animator.StringToHash("RushAttack");
        protected static readonly int DeadHash = Animator.StringToHash("Die");
        protected static readonly int LocomotionSwordHash = Animator.StringToHash("2Hand-Sword-WalkRun-Blend");
        
        

        protected const float crossFadeDuration = 0.1f;

        protected BaseState(PlayerController player, Animator animator)
        {
            this.player = player;
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
