using UnityEngine;

namespace RpgPractice
{
    public class LocomotionState : BaseState
    {
        private bool change;
        public LocomotionState(PlayerController player, Animator animator) : base(player, animator)
        {
            
        }

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
            player.RemoveEffect((int)StatusEffect.GUARD);
            // if (player.weaponManager.CurrentWeapon == WeaponType.Unarmed)
            // {
            //     animator.CrossFade(LocomotionHash, crossFadeDuration);
            // }
            // else if (player.weaponManager.CurrentWeapon == WeaponType.Sword)
            // {
            //     animator.CrossFade(LocomotionSwordHash, crossFadeDuration);
            // }
            
        }

        public override void FixedUpdate()
        {
            if (!change && player.GetSprint())
            {
                animator.CrossFade(RunHash, crossFadeDuration);    
                change = true;
            }
            else if (change && !player.GetSprint())
            {
                animator.CrossFade(LocomotionHash, crossFadeDuration);
                change = false;
            }
            
            player.HandleMovement();
        }
    }
}