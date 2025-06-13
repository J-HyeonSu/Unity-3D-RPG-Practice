using UnityEngine;
using UnityEngine.AI;

namespace RpgPractice
{
    public class EnemyAttackState : EnemyBaseState
    {
        private readonly NavMeshAgent agent;
        private readonly Transform player;
        
        public EnemyAttackState(Enemy enemy, Animator animator, NavMeshAgent agent, Transform player) : base(enemy, animator)
        {
            this.agent = agent;
            this.player = player;
        }

        public override void OnEnter()
        {
            agent.isStopped = true;
            animator.CrossFade(AttackHash, crossFadeDuration);
            enemy.Attack();
            
        }

        public override void OnExit()
        {
            agent.isStopped = false;
        }

        public override void Update()
        {
            if (!enemy.attackTimer.IsRunning)
            {
                Debug.Log("At");
                animator.Play(AttackHash, 0, 0f);
                enemy.Attack();
            }
            
            LookAtPlayer();
        }
        
        private void LookAtPlayer()
        {
            Vector3 targetDirection = player.position - enemy.transform.position;
            targetDirection.y = 0; // 수평으로만 바라보기
           
            if (targetDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, 
                    5f * Time.deltaTime);
            }
        }
    }
}