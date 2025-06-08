using System;
using UnityEngine;

namespace RpgPractice
{
    public class Projectile : MonoBehaviour
    {
        private float speed = 30;
        private float damage = 5;
        private float length;
        private float len;

        private void OnEnable()
        {
            
        }

        public void Init(Vector3 position, Vector3 direction, float length, float speed, float damage)
        {
            this.length = length;
            this.damage = damage;
            this.speed = speed;
            len = 0;
            transform.parent.position = position;
            transform.position = new Vector3(position.x, position.y+1, position.z);

            if (direction != Vector3.zero)
            {
                transform.parent.rotation = Quaternion.LookRotation(direction);    
            }

        }


        private void Update()
        {
            var l = transform.forward * speed * Time.deltaTime;
            transform.position += l;
            len += l.magnitude;

            if (len > length) 
            {
                transform.parent.gameObject.SetActive(false);  
            }
            

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                other.gameObject.GetComponent<Health>().TakeDamage(damage);
            }
        }

        
    }
}