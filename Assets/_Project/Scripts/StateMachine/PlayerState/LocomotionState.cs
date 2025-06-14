using UnityEngine;

namespace RpgPractice
{
    public class LocomotionState : BaseState
    {
        public LocomotionState(PlayerController player, Animator animator) : base(player, animator)
        {
            
        }

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
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
            player.HandleMovement();
        }
    }
}