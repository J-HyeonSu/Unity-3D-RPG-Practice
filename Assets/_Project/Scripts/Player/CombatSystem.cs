using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace RpgPractice
{
    public enum SkillType
    {
        LeftClick, RightClick, Skill1, Skill2, Skill3, Skill4, Combo
    }
    
    // 플레이어의 공격, 스킬 담당
    public class CombatSystem : MonoBehaviour
    {
        [SerializeField] private IntEventChannel onSkillEventChannel;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private FloatEventChannel[] skillFloatEventChannels;
        [SerializeField] private SkillSword sword;
        [SerializeField] private SkillBow bow;
        [SerializeField] private SkillStaff staff;
        
        
        private IWeaponBehaviour currentWeapon;
        private CountdownTimer[] timers;
        private bool[] onSkills;
        private bool isCasting;
        private SkillType currentAttack;

        private Mana playerMana;
        private Health playerHealth;

        private SkillData skillData;

        private const int MAX_SKILLSIZE = 6;

        private void Start()
        {
            skillData = new SkillData
            {
                PlayerTransform = transform,
                PlayerHealth = GetComponentInParent<Health>(),
                PlayerMana = GetComponentInParent<Mana>()
            };
            
            

            currentWeapon = sword;
            onSkills = new bool[MAX_SKILLSIZE];
            timers = new CountdownTimer[MAX_SKILLSIZE];
            
            
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
                if (!onSkills[i]) continue; // 스킬 동시시전 x
                var canUse = CanUseSkill(i);
                if(canUse==1)
                {
                    onSkillEventChannel?.Invoke(i);
                    timers[i].Start();
                    currentAttack = (SkillType)i;
                    skillData.IsCombo = false;
                    isCasting = true;
                    break;
                }
                if (i == 0 && canUse == 0)
                {
                    //콤보로직 이동, 좌클이고 쿨타임일때
                    onSkillEventChannel?.Invoke(i);
                    skillData.IsCombo = true;
                    currentAttack = SkillType.Combo;
                    isCasting = true;
                    break;
                }
            }
            
        }

        int CanUseSkill(int skillIndex)
        {
            // return 타입 -1 = 사망, 0 = 쿨타임, 1성공, 2마나 부족
            if (timers[skillIndex].IsRunning) return 0;
            if (playerHealth)
            {
                if (playerHealth.IsDead) return -1;
            }

            if (playerMana)
            {
                float requiredMana = currentWeapon.GetManaCost(skillIndex);
                if (!playerMana.UseMana(requiredMana)) return 2;
            }

            return 1;
        }
        

        public void CastingEndAnim()
        {
            switch (currentAttack)
            {
                case SkillType.LeftClick: currentWeapon.LeftClick(skillData); break;
                case SkillType.RightClick: currentWeapon.RightClick(skillData); break;
                case SkillType.Skill1: currentWeapon.Skill1(skillData); break;
                case SkillType.Skill2: currentWeapon.Skill2(skillData); break;
                case SkillType.Skill3: currentWeapon.Skill3(skillData); break;
                case SkillType.Skill4: currentWeapon.Skill4(skillData); break;
                case SkillType.Combo: currentWeapon.LeftClick(skillData); break;
            }
            isCasting = false;
            
        }

        public void OnAttackEndAnim()
        {
            // -1일시 isAttack = false;
            onSkillEventChannel?.Invoke(-1);
            isCasting = false;
        }

        public void NewTarget(Transform target)
        {
            skillData.Target = target;
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