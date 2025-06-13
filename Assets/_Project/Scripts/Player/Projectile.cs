using System;
using RPGCharacterAnims.Actions;
using UnityEngine;

namespace RpgPractice
{
    public class Projectile : MonoBehaviour
    {
        private ProjectileData data;
        
        private float damage;
        private float currentLength;
        private float currentTime;
        private int currentHit;

        public void Init(Vector3 position, Vector3 direction, ProjectileData projectileData,float attackPower)
        {
            this.data = projectileData;
            
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
            
            if (other.CompareTag("Enemy"))
            {
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
                var health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    currentHit++;

                    //관통 로직
                    if (!data.piercing || currentHit >= data.maxHits)
                    {
                        DeactivateProjectile();
                    }
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