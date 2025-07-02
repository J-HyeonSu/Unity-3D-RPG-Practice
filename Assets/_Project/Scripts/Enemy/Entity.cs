using UnityEngine;
using UnityEngine.AI;

namespace RpgPractice
{
    
    public abstract class Entity : MonoBehaviour
    {
        [SerializeField] protected Health health;
        [SerializeField] protected Animator animator;

        public abstract void Die();
        public abstract void TakeDamage(float damage);
    }
}