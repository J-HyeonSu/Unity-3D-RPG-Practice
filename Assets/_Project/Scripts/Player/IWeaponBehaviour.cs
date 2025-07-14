using UnityEngine;

namespace RpgPractice
{
    public interface IWeaponBehaviour
    {
        public void LeftClick(Transform transform,bool combo);
        public void RightClick(Transform transform);
        public void Skill1(Transform transform);
        public void Skill2(Transform transform);
        public void Skill3(Transform transform);
        public void Skill4(Transform transform);
        public float GetCooldown(int idx);
        public float GetManaCost(int idx);
    }
}