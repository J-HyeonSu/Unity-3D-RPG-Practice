using System;
using TMPro;
using UnityEngine;

namespace RpgPractice
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        private void Awake()
        {
            instance = this;
        }
        
    }
}