using System;
using UnityEngine;

namespace RpgPractice
{
    public class UIBillboarding : MonoBehaviour
    {
        private Camera cam;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            transform.forward = cam.transform.forward;
        }
    }
}