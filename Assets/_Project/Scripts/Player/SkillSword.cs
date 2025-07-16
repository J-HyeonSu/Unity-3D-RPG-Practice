using System;
using System.Collections;
using UnityEngine;

namespace RpgPractice
{
    
    [CreateAssetMenu]
    public class SkillSword : ScriptableObject, IWeaponBehaviour
    {
        [SerializeField] private ProjectileData[] skills;
        
        
        public void LeftClick(SkillData skillData)
        {
            UseSkill(skillData.IsCombo ? SkillType.Combo : SkillType.LeftClick, skillData.PlayerTransform.position,
                skillData.PlayerTransform.forward, skillData.PlayerTransform.parent.gameObject);
        }

        public void RightClick(SkillData skillData)
        {
            UseSkill(SkillType.RightClick, skillData.PlayerTransform.position, skillData.PlayerTransform.forward, skillData.PlayerTransform.parent.gameObject);
        }

        public void Skill1(SkillData skillData)
        {
            //막기 //
            skillData.PlayerHealth.AddStatus((int)StatusEffect.GUARD);
        }

        public void Skill2(SkillData skillData)
        {
            Debug.Log("skill2");
            //돌진
            var pc = skillData.PlayerTransform.GetComponent<PlayerController>();
            if (pc)
            {
                //돌진
                pc.RushAttack(skillData, 3f, 30f, skills[(int)SkillType.Skill2].damage);
            }

        }
        

        public void Skill3(SkillData skillData)
        {
            Debug.Log("skill3");
            //무력
            
        }

        public void Skill4(SkillData skillData)
        {
            Debug.Log("skill4");
            //버프
        }

        public float GetCooldown(int idx)
        {
            if (!skills[idx]) return 1;
            return skills[idx].cooldown;
        }

        public float GetManaCost(int idx)
        {
            if (!skills[idx]) return 1;
            return skills[idx].manaCost;
        }
        
        

        public void SetDamage(int idx, float damage)
        {
            skills[idx].damage = damage;
        }

        private void UseSkill(SkillType skillType, Vector3 position, Vector3 direction, GameObject shooter)
        {
            int skillIndex = (int)skillType;
            if (skillIndex < skills.Length)
            {
                var skillData = skills[skillIndex];
                
                var proj = PoolManager.instance.Get(skillData.prefab);
                proj.GetComponentInChildren<Projectile>().Init(shooter,position, direction, skillData);
            }
        }
        
        
        
        
    }
}