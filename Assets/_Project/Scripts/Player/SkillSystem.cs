using System;
using UnityEngine;

namespace RpgPractice
{
    public enum SkillType
    {
        Attack1, Attack2, SubAttack
    }
    [System.Serializable]
    public class WeaponSkillData
    {
        public WeaponType weaponType;
        public ProjectileData[] skills; // [0] = Attack1, [1] = Attack2, [2] = SubAttack
    }
    public class SkillSystem : MonoBehaviour
    {
        [SerializeField] private WeaponSkillData[] weaponSkills;
        [SerializeField] private WeaponManager weaponManager;

        public void UseSkill(SkillType skillType, Vector3 position, Vector3 direction, float attackPower)
        {
            WeaponSkillData currentWeaponData = GetCurrentWeaponSkillData();
            if (currentWeaponData == null) return;
            
            int skillIndex = (int)skillType;
            if (skillIndex < currentWeaponData.skills.Length)
            {
                var skillData = currentWeaponData.skills[skillIndex];

                float finalDamage = attackPower * skillData.damageCoefficient;
                
                var proj = PoolManager.instance.Get(skillData.prefab);
                proj.GetComponentInChildren<Projectile>().Init(
                    position, direction,
                    skillData.length, skillData.speed, finalDamage, skillData.lifeTime
                );
            }
        }
        private WeaponSkillData GetCurrentWeaponSkillData()
        {
            var currentWeapon = weaponManager.CurrentWeapon;
            return Array.Find(weaponSkills, data => data.weaponType == currentWeapon);
        }
    
        private int GetProjectilePrefabIndex(int skillIndex)
        {
            // 무기별로 다른 프리팹 인덱스 계산
            int weaponOffset = (int)weaponManager.CurrentWeapon * 10; // 검:0~9, 활:10~19 등
            return weaponOffset + skillIndex;
        }
    }
}