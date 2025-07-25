using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{
    
    public class TargetDetector : MonoBehaviour
    {
        [Header("Performance Settings")] 
        [SerializeField] private float detectionInterval = 0.2f;
        [SerializeField] private float maxDetectionRange = 16f;
        
        [Header("Detection Settings")] 
        [SerializeField] private LayerMask enemyLayerMask;

        [SerializeField] private float resetTargetTime = 2f;
        
        [SerializeField] private TransformEventChannel currentTargetEventChannel;
        
        private Transform currentTarget;
        private Health currentHealth;
        private float targetDistance;

        
        private bool lostTarget;
        private float targetTimer;
        
        private void Start()
        {
            StartCoroutine(DetectionCoroutine());
        }

        private void Update()
        {
            if (lostTarget && targetTimer > 0)
            {
                targetTimer -= Time.deltaTime;
                if (targetTimer <= 0)
                {
                    currentTarget = null;
                    currentTargetEventChannel?.Invoke(currentTarget);
                }

            }
        }


        void OnDrawGizmos()
        {
            Vector3 start = transform.position;
            Vector3 direction = transform.forward;
            
            Vector3 boxCenter = start+ direction * (maxDetectionRange / 2f);
            Vector3 boxSize = new Vector3(2f, 2f, maxDetectionRange);
            
            // 타겟이 있으면 빨간색, 없으면 노란색
            Gizmos.color = currentTarget ? Color.red : Color.yellow;
            
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, Quaternion.LookRotation(direction), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = oldMatrix;
            
        }

        IEnumerator DetectionCoroutine()
        {
            while (true)
            {
                DetectEnemyInFront();
                
                yield return new WaitForSeconds(detectionInterval);
            }
        }

        void DetectEnemyInFront()
        {
            List<Transform> targets = AttackRangeDetector.GetTargetsInLine(
                transform.position,
                transform.forward,
                maxDetectionRange,
                1f, enemyLayerMask);
            

            Transform closesEnemy = null;
            float min = 30f;
            foreach (var target in targets)
            {
                if (target.gameObject.CompareTag("Enemy"))
                {
                    targetDistance = Vector3.Distance(target.position, transform.position);
                    if (min > targetDistance)
                    {
                        closesEnemy = target;
                        min = targetDistance;
                    }    
                }
            }
            

            if (closesEnemy)
            {
                currentTarget = closesEnemy;
          
                currentTargetEventChannel?.Invoke(currentTarget);
                
                targetTimer = resetTargetTime;
                lostTarget = false;
            }
            else
            {
                lostTarget = true;
            }

        }
        
    }
}