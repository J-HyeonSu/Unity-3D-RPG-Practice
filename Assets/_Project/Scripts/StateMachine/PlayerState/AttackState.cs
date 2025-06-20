﻿using UnityEngine;

namespace RpgPractice
{
    public class AttackState : BaseState
    {
        
        public AttackState(PlayerController player, Animator animator) : base(player, animator)
        {
        }

        public override void OnEnter()
        {
            animator.CrossFade(AttackHash, crossFadeDuration);
            //player.Attack();
            
  
        }

        public override void Update()
        {
            
        }


        public override void FixedUpdate()
        {
            player.HandleMovement();
            player.HandleJump();
        }
    }
}