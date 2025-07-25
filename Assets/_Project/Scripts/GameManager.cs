using System;
using TMPro;
using UnityEngine;

namespace RpgPractice
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        
        public bool IsUIOpen { get; private set; }

        private void Awake()
        {
            instance = this;
        }


        public void SetUIState(bool isOpen)
        {
            IsUIOpen = isOpen;
        }
    }
}