using System;
using UnityEngine;

namespace RpgPractice
{
    public class Projectile : MonoBehaviour
    {
        private ProjectileData data;
        
        private float speed;
        private float damage;
        private float length;
        private float lifeTime;
        
        private float currentLength;
        private float currentTime;

        public void Init(Vector3 position, Vector3 direction, float length, float speed, float damage, float lifeTime)
        {
            this.length = length;
            this.damage = damage;
            this.speed = speed;
            this.lifeTime = lifeTime;
            
            currentLength = 0;
            currentTime = 0;
            
            transform.parent.position = position;
            transform.position = new Vector3(position.x, position.y+1, position.z);

            if (direction != Vector3.zero)
            {
                transform.parent.rotation = Quaternion.LookRotation(direction);    
            }

        }

        private void Update()
        {
            var movement  = transform.forward * (speed * Time.deltaTime);
            transform.position += movement;
            currentLength += movement.magnitude;
            currentTime += Time.deltaTime;

            // 거리 제한 또는 시간 제한
            if (currentLength > length || currentTime > lifeTime)
            {
                DeactivateProjectile();
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                var health = other.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
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