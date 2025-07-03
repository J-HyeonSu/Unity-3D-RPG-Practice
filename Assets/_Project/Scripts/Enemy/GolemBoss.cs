using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace RpgPractice
{
    public class GolemBoss : Enemy
    {
        protected override void Start()
        {
            SetupStateMachine();
        }

        protected override void SetupStateMachine()
        {
            //보스용 상태머신
            stateMachine = new StateMachine();
            attackTimer = new CountdownTimer(timeBetweenAttacks);

            attackTimer.OnTimerStart += () => { canMove = false; };
            attackTimer.OnTimerStop += () => { canMove = true; };

            var locomotionState = new EnemyLocomotionState(this, animator, agent, wanderRadius, playerDetector.Player);
            var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
            var deadState = new EnemyDeadState(this, animator);

            At(locomotionState, attackState, new FuncPredicate(()=> playerDetector.CanAttackPlayer()));
            At(attackState, locomotionState, new FuncPredicate(()=> canMove));
            Any(deadState, new FuncPredicate(()=> GetComponent<Health>().IsDead));
            
            stateMachine.SetState(locomotionState);
            
        }

        public override void Attack()
        {
            throw new System.NotImplementedException();
        }
    }
}