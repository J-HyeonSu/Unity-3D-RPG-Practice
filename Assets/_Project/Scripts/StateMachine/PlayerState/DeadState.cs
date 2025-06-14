using Unity.VisualScripting;
using UnityEngine;

namespace RpgPractice
{
    public class DeadState : BaseState
    {
        public DeadState(PlayerController player, Animator animator) : base(player, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(DeadHash, crossFadeDuration);
            player.Dead();
        }
    }
}