using System;
using UnityEngine;

namespace RpgPractice
{
    public enum SkillType
    {
        Attack1, Attack2, SubAttack, Num1, Num2, Num3, Num4
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
                
                var proj = PoolManager.instance.Get(skillData.prefab);
                proj.GetComponentInChildren<Projectile>().Init(position, direction, skillData, attackPower);
            }
        }
        
        private WeaponSkillData GetCurrentWeaponSkillData()
        {
            var currentWeapon = weaponManager.CurrentWeapon;
            return Array.Find(weaponSkills, data => data.weaponType == currentWeapon);
        }
        
    }
}