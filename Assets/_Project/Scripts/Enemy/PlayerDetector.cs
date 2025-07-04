using UnityEngine;
using Utilities;

namespace RpgPractice
{
    public class PlayerDetector : MonoBehaviour
    {
        [SerializeField] private float detectionAngle = 60f;
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private float innerDetectionRadius = 5f;
        [SerializeField] private float detectionCooldown = 1f;
        [SerializeField] private float attackRange = 2f;

        
        public Transform Player { get; private set; }
        public Health PlayerHealth { get; private set; }
        
        private CountdownTimer detectionTimer;

        private IDetectionStrategy detectionStrategy;

        void Awake()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            PlayerHealth = Player.GetComponent<Health>();
        }

        private void Start()
        {
            detectionTimer = new CountdownTimer(detectionCooldown);
            detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
        }

        private void Update()
        {
            detectionTimer.Tick(Time.deltaTime);
        }

        public bool CanDetectPlayer() 
        {
            return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
        }

        public bool CanAttackPlayer()
        {
            if (!Player) return false;
            
            var directionToPlayer = Player.position - transform.position;
            return directionToPlayer.magnitude <= attackRange;                
        }
        

        public void SetDetectionStrategy(IDetectionStrategy detectionStrategy)
        {
            this.detectionStrategy = detectionStrategy;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);

            Vector3 forwardConeDirection =
                Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRadius;
            Vector3 backwardConeDirection =
                Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRadius;
            
            Gizmos.DrawLine(transform.position, transform.position+forwardConeDirection);
            Gizmos.DrawLine(transform.position, transform.position+backwardConeDirection);
        }
        
        public void SetAttackRange(float attackRange)
        {
            this.attackRange = attackRange;
        }
    }
}