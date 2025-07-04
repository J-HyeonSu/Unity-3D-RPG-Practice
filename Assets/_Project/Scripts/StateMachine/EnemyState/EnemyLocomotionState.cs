using UnityEngine;
using UnityEngine.AI;

namespace RpgPractice
{
    public class EnemyLocomotionState : EnemyBaseState
    {
        private readonly NavMeshAgent agent;
        private readonly Vector3 startPoint;
        private readonly float wanderRadius;
        private Transform player;
        
        public EnemyLocomotionState(Enemy enemy, Animator animator, NavMeshAgent agent, float wanderRadius, Transform player) : base(enemy, animator)
        {
            this.agent = agent;
            this.startPoint = enemy.transform.position;
            this.wanderRadius = wanderRadius;
            this.player = player;
        }

        public override void OnEnter()
        {
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

        public override void Update()
        {
            // if (enemy.isDetected)
            // {    
            //     agent.SetDestination(player.position);
            //     return;
            // }
            
            if (HasReachedDestination())
            {
                var randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += startPoint;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDirection,out hit, wanderRadius, 1);
                var finalPosition = hit.position;

                agent.SetDestination(finalPosition);

            }

            
        }

        bool HasReachedDestination()
        {
            return !agent.pathPending
                   && agent.remainingDistance <= agent.stoppingDistance
                   && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }
    }
}