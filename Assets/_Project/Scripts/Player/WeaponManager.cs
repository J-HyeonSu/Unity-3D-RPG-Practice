using System;
using UnityEngine;

namespace RpgPractice
{
    public enum WeaponType
    {
        Sword,
        Bow,
        Staff,
        Unarmed
    }
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Unarmed;
        [SerializeField] private GameObject[] weapons;
        
        public WeaponType CurrentWeapon => weaponType;
        
        public void ChangeWeapon(WeaponType newWeaponType)
        {
            // 모든 무기 비활성화
            foreach (var weapon in weapons)
            {
                if (weapon)
                {
                    weapon.SetActive(false);
                }
            }
            
            // 새 무기 활성화
            weaponType = newWeaponType;
            int weaponIndex = (int)weaponType;
            
            if (weaponIndex < weapons.Length && weapons[weaponIndex])
            {
                weapons[weaponIndex].SetActive(true);
            }
            
            
        }
    }
    
    
}