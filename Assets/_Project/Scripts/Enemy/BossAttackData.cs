using UnityEngine;

namespace RpgPractice
{
    public enum AttackRangeType
    {
        Circle, //원형
        Cone, // 부채꼴
        Line, // 직선
        Donut, //도넛
        Rectangle //사각형
    }
    
    [CreateAssetMenu(fileName = "New Boss Attack", menuName = "RPG/Boss Attack Data")]
    public class BossAttackData : ScriptableObject
    {
        [Header("기본 정보")] 
        public string attackName;
        public AttackRangeType rangeType;
        public float damage = 10f;
        public float castTime = 2f;
        public float cooldown = 5f;

        [Header("범위 설정")] 
        public float range = 10f;
        public float angle = 60f;
        public float width = 5f;
        public float innerRadius = 3f;

        [Header("시각 효과")] 
        public GameObject telegraphPrefab; 
        public GameObject attackEffectPrefab;
        public Color rangeColor = Color.red;

        
    }
}