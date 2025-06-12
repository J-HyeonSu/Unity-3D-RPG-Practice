using UnityEngine;

namespace RpgPractice
{
    [CreateAssetMenu(fileName = "New Projectile Data", menuName = "RPG/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Prefab")] 
        public GameObject prefab;
        
        [Header("Basic Settings")]
        public float speed = 30f; // 투사체 속도
        public float damageCoefficient  = 1f; // 데미지 계수
        public float length = 5f; // 사거리
        public float lifeTime = 3f; // 시간 제한
        
        [Header("Advanced")]
        public bool piercing = false; // 관통 여부
        public int maxHits = 1; // 최대 타격 수
    }
}