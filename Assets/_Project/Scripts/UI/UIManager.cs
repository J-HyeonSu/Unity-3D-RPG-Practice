
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RpgPractice
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        public GameObject damagePopupPrefab;
        [SerializeField] private TextMeshProUGUI levelText;

        [SerializeField] private UITargetHealth targetHealth;
        
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            
        }


        public void SetLevelText(int level)
        {
            levelText.text = level.ToString();
        }
        
        
        public void CreatePopUp(Vector3 position, string text, Color color, float duration = 1f)
        {
            var popup = PoolManager.instance.Get(damagePopupPrefab);
            popup.transform.position = position;
            var v = popup.transform.GetComponent<DamagePopUpAnimation>();
            v.Init(text, color, duration);
            
        }
    }
}