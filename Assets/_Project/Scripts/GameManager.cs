using System;
using TMPro;
using UnityEngine;

namespace RpgPractice
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public PoolManager poolManager;

        private void Awake()
        {
            instance = this;
        }
        
        public void CreatePopUp(Vector3 position, string text, Color color, float duration = 1f)
        {
            
            var popup = poolManager.Get(0);
            popup.transform.position = position;
            var v = popup.transform.GetComponent<DamagePopUpAnimation>();
            v.Init(text, color, duration);
            
        }
        
    }
}