using System;
using System.Linq;
using UnityEngine;
using Utilities;

namespace RpgPractice
{
    public enum SkillType
    {
        LeftClick, RightClick, Skill1, Skill2, Skill3, Skill4
    }
    
    // 플레이어의 공격, 스킬 담당
    public class CombatSystem : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private FloatEventChannel[] skillFloatEventChannels;
        [SerializeField] private SkillSword sword;
        [SerializeField] private SkillBow bow;
        [SerializeField] private SkillStaff staff;
        
        
        private IWeaponBehaviour currentWeapon;
        private CountdownTimer[] timers;
        private bool[] onSkills;
        private bool isCasting;

        private void Start()
        {
            currentWeapon = sword;
            onSkills = new bool[6];
            timers = new CountdownTimer[6];
            
            for (int i = 0; i < timers.Length; i++)
            {
                int idx = i;
                timers[i] = new CountdownTimer(currentWeapon.GetCooldown(i));
                timers[i].OnTimerStop += () => OnSkillCooldownComplete(idx);
            }
            
            ChangeWeapon(WeaponType.Sword);
            
        }

        void OnSkillCooldownComplete(int skillIndex)
        {
            if (skillFloatEventChannels[skillIndex])
            {
                skillFloatEventChannels[skillIndex]?.Invoke(0);
                isCasting = false;
            }
        }

        public void ChangeWeapon(WeaponType weaponType)
        {
            IWeaponBehaviour newWeapon = sword;
            switch (weaponType)
            {
                case WeaponType.Sword:
                    newWeapon = sword;
                    break;
                case WeaponType.Bow:
                    newWeapon = bow;
                    break;
                case WeaponType.Staff:
                    newWeapon = staff;
                    break;
            }
            currentWeapon = newWeapon;
                
            for (int i = 0; i < timers.Length; i++)
            {
                timers[i].Reset(currentWeapon.GetCooldown(i));
            }
        }
        
        

        private void Update()
        {
            for (int i = 0; i < timers.Length; i++)
            {
                timers[i].Tick(Time.deltaTime);
                if (timers[i].IsRunning)
                {
                    if (skillFloatEventChannels[i])
                    {
                        // UI 타이머용
                        skillFloatEventChannels[i]?.Invoke(timers[i].Progress);
                    }
                    
                }
            }
            
            if (isCasting) return;

            for(int i=0; i<onSkills.Length; i++)
            {
                if (onSkills[i] && !timers[i].IsRunning)
                {
                    ExecuteSkill((SkillType)i);
                    isCasting = true;
                    break;
                }
            }
            
        }

        private void ExecuteSkill(SkillType skillType)
        {
            //쿨타임
            timers[(int)skillType].Start();

            switch (skillType)
            {
                case SkillType.LeftClick: currentWeapon.LeftClick(transform); break;
                case SkillType.RightClick: currentWeapon.RightClick(transform); break;
                case SkillType.Skill1: currentWeapon.Skill1(transform); break;
                case SkillType.Skill2: currentWeapon.Skill2(transform); break;
                case SkillType.Skill3: currentWeapon.Skill3(transform); break;
                case SkillType.Skill4: currentWeapon.Skill4(transform); break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null);
            }
        }

        public void CastingEndAnim()
        {
            //애니메이션 이벤트
            isCasting = false;
        }
        
        private void OnEnable()
        {
            inputReader.LeftClick += OnLeftClick;
            inputReader.RightClick += OnRightClick;
            inputReader.Skill1 += OnSkill1;
            inputReader.Skill2 += OnSkill2;
            inputReader.Skill3 += OnSkill3;
            inputReader.Skill4 += OnSkill4;
        }
        private void OnDisable()
        {
            inputReader.LeftClick -= OnLeftClick;
            inputReader.RightClick -= OnRightClick;
            inputReader.Skill1 -= OnSkill1;
            inputReader.Skill2 -= OnSkill2;
            inputReader.Skill3 -= OnSkill3;
            inputReader.Skill4 -= OnSkill4;
        }
        private void OnLeftClick(bool value) => onSkills[(int)SkillType.LeftClick] = value;
        private void OnRightClick(bool value) => onSkills[(int)SkillType.RightClick] = value;
        private void OnSkill1(bool value) => onSkills[(int)SkillType.Skill1] = value;
        private void OnSkill2(bool value)=> onSkills[(int)SkillType.Skill2] = value;
        private void OnSkill3(bool value)=> onSkills[(int)SkillType.Skill3] = value;
        private void OnSkill4(bool value)=> onSkills[(int)SkillType.Skill4] = value;
    }
}