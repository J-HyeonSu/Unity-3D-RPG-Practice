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
        public float damage  = 1f; // 데미지
        public float length = 5f; // 사거리
        public float lifeTime = 3f; // 시간 제한
        
        
        [Header("Advanced")]
        public bool piercing = false; // 관통 여부
        public int maxHits = 1; // 최대 타격 수

        [Header("Player Settings")] 
        public float manaCost = 0f;
        public float cooldown = 0f;
        public float manaRestore = 0f;

        [Header("Effects")] 
        public float knockbackForce = 0f; // 넉백 힘
        public float slowDuration = 0f; // 슬로우 지속시간
        public float slowAmount = 0.5f; // 슬로우 강도 .5f = 50%감소

        [Header("Critical System")] 
        public bool canCritical = false;
        public float criticalChance = 0.1f;
        public float criticalMultiplier = 2f;

        [Header("Audio & Visual")] 
        public string skillName = "기본 공격";
        public AudioClip castSound;
        public AudioClip hitSound;



    }
}