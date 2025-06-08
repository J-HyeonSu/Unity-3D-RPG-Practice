using System;
using TMPro;
using UnityEngine;

namespace RpgPractice
{
    public class DamagePopUpAnimation : MonoBehaviour
    {
        public AnimationCurve opacityCurve;
        public AnimationCurve scaleCurve;
        public AnimationCurve heightCurve;
        
        private TextMeshProUGUI tmp;
        private float time = 0;
        private float maxTime = 1f;
        private Vector3 origin;

        private void Awake()
        {
            tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            origin = transform.position;
        }
        
        public void Init(string text, Color color, float duration)
        {
            maxTime = duration;
            time = 0f;
            origin = transform.position;
            
            tmp.text = text;
            tmp.faceColor = color;
        }
        

        void Update()
        {
            if (time >= maxTime)
            {
                gameObject.SetActive(false);
                return;
            }
            tmp.color = new Color(1, 1, 1, opacityCurve.Evaluate(time));
            transform.localScale = Vector3.one * scaleCurve.Evaluate(time);
            transform.position = origin + new Vector3(0, 1 + heightCurve.Evaluate(time), 0);
            time += Time.deltaTime;
        }
    }
}
