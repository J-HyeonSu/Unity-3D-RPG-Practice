using System;
using UnityEngine;

namespace RpgPractice
{
    [CreateAssetMenu]
    public class SkillSword : ScriptableObject, IWeaponBehaviour
    {
        [SerializeField] private ProjectileData[] skills;


        public void LeftClick(Transform transform)
        {
            UseSkill(SkillType.LeftClick, transform.position, transform.forward, transform.parent.gameObject);
            
        }

        public void RightClick(Transform transform)
        {
            UseSkill(SkillType.RightClick, transform.position, transform.forward, transform.parent.gameObject);
        }

        public void Skill1(Transform transform)
        {
            Debug.Log("skill1");
            //막기
        }

        public void Skill2(Transform transform)
        {
            Debug.Log("skill2");
            //돌진
        }

        public void Skill3(Transform transform)
        {
            Debug.Log("skill3");
            //무력
        }

        public void Skill4(Transform transform)
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