using System;
using RPGCharacterAnims.Actions;
using UnityEngine;

namespace RpgPractice
{
    public class Projectile : MonoBehaviour
    {
        private ProjectileData data;
        private GameObject shooter;
        
        private float damage;
        private float currentLength;
        private float currentTime;
        private int currentHit;

        public void Init(GameObject shooter, Vector3 position, Vector3 direction, ProjectileData projectileData,float attackPower)
        {
            this.data = projectileData;
            this.shooter = shooter;

            if (!projectileData)
            {
                Debug.Log("projectileData 없음");
            }

            if (!projectileData.prefab)
            {
                Debug.Log("projectileData prefab 없음");
            }
            
            damage = attackPower * data.damageCoefficient;
            currentLength = 0;
            currentTime = 0;
            currentHit = 0;
            
            transform.parent.position = position;
            transform.position = new Vector3(position.x, position.y+1, position.z);

            if (direction != Vector3.zero)
            {
                transform.parent.rotation = Quaternion.LookRotation(direction);    
            }

        }

        private void Update()
        {
            var movement  = transform.forward * (data.speed * Time.deltaTime);
            transform.position += movement;
            currentLength += movement.magnitude;
            currentTime += Time.deltaTime;

            // 거리 제한 또는 시간 제한
            if (currentLength > data.length || currentTime > data.lifeTime)
            {
                DeactivateProjectile();
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == shooter) return;

            if (IsSameTeam(other)) return;
            
            
            if (other.CompareTag("Enemy") || other.CompareTag("Player"))
            {
                
                HandleDamage(other);
            }
        }

        bool IsSameTeam(Collider other)
        {
            if (!shooter) return false;
            bool result = false;
            result =
                shooter.CompareTag("Enemy") && other.CompareTag("Enemy") ||
                shooter.CompareTag("Player") && other.CompareTag("Player");
            
            return result;
        }

        private void HandleDamage(Collider other)
        {
            //other은 player부모객체라 playermodel에 health가있어서 안되고 있음.
            var health = other.GetComponent<Health>();
            if (health != null)
            {
                if (health.IsDead) return;
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
                health.TakeDamage(damage);
                currentHit++;

                //관통 로직
                if (!data.piercing || currentHit >= data.maxHits)
                {
                    DeactivateProjectile();
                }
            }
        }

        private void DeactivateProjectile()
        {
            if (transform.parent != null)
                transform.parent.gameObject.SetActive(false);
            else
                gameObject.SetActive(false);
        }
        
        
    }
}