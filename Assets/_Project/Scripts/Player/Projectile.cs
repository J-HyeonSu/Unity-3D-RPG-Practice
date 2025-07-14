using System;
using RPGCharacterAnims.Actions;
using UnityEngine;

namespace RpgPractice
{
    public class Projectile : MonoBehaviour
    {
        private ProjectileData data;
        private GameObject shooter;
        private Collider coll;
            
        private float currentLength;
        private float currentTime;
        private int currentHit;

        
        private void Start()
        {
            coll = GetComponent<Collider>();
        }

        public void Init(GameObject shooter, Vector3 position, Vector3 direction, ProjectileData data)
        {
            this.data = data;
            this.shooter = shooter;

            if (!data)
            {
                Debug.Log("projectileData 없음");
            }

            if (!data.prefab)
            {
                Debug.Log("projectileData prefab 없음");
            }
            currentLength = 0;
            currentTime = 0;
            currentHit = 0;

            if (coll)
            {
                coll.enabled = true;
            }
            
            
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
                health.TakeDamage(data.damage);
                currentHit++;
                
                //마나 관련
                if (shooter.CompareTag("Player"))
                {
                    shooter.GetComponent<Mana>().RestoreMana(data.manaRestore);
                }

                //관통 로직
                if (!data.piercing || currentHit >= data.maxHits)
                {
                    //DeactivateProjectile();
                    coll.enabled = false;
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