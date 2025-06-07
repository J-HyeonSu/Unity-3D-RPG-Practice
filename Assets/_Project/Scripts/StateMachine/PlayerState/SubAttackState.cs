using UnityEngine;

namespace RpgPractice
{
    public class SubAttackState : BaseState
    {
        public SubAttackState(PlayerController player, Animator animator) : base(player, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(SubAttackHash, crossFadeDuration);
        }
        
        public override void FixedUpdate()
        {
            player.HandleMovement();
        }

    }
}