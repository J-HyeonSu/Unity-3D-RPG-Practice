using UnityEngine;

namespace RpgPractice
{
    public class SkillData
    {
        public Transform PlayerTransform;
        public Health PlayerHealth;
        public Mana PlayerMana;
        public bool IsCombo;

        public Transform TargetTransform;
    }
    public interface IWeaponBehaviour
    {
        public void LeftClick(SkillData skillData);
        public void RightClick(SkillData skillData);
        public void Skill1(SkillData skillData);
        public void Skill2(SkillData skillData);
        public void Skill3(SkillData skillData);
        public void Skill4(SkillData skillData);
        public float GetCooldown(int idx);
        public float GetManaCost(int idx);
    }
}