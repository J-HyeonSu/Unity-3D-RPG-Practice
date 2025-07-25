using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace RpgPractice
{
    public class UITargetHealth : MonoBehaviour
    {
        [Header("UI Reference")] 
        [SerializeField] private GameObject healthBarPanel;
        [SerializeField] private Image currentHealthBar;
        [SerializeField] private TextMeshProUGUI distanceText;

        [Header("Settings")] 
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float hideDelay = 5f;

        private Transform currentTarget;
        private Health currentHealth;
        
        private float hideTimer;
        private bool isVisible;
        
        private void Start()
        {
            Hide();
        }

        private void Update()
        {
            
            if (isVisible && hideTimer > 0)
            {
                hideTimer -= Time.deltaTime;
                if (hideTimer <= 0)
                {
                    Hide();
                }
            }

            if (currentTarget)
            {
                if (currentHealth.IsDead)
                {
                    Hide();
                    return;
                }
                Vector3 playerPos = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
                Vector3 targetPos = new Vector3(currentTarget.transform.position.x, 0, currentTarget.transform.position.z);
                var distance = Vector3.Distance(playerPos, targetPos);
                UpdateDistance(distance);
                UpdateHealthBar(currentHealth.GetPercentage());
            }
        }

        public void NewTarget(Transform target)
        {
            if (!target)
            {
                Hide();
                return;
            }
            if (currentTarget != target)
            {
                currentTarget = target;
                currentHealth = currentTarget.GetComponent<Health>();
            }
            Show();
            hideTimer = hideDelay;
        }

        void UpdateHealthBar(float percentage)
        {
            if (currentHealthBar)
            {
                currentHealthBar.fillAmount = percentage;
            }
        }
        
        void UpdateDistance(float distance)
        {
            if (!distanceText) return;
            if (!playerTransform) return;

            
            distanceText.text = $"{distance:F1} m";
            
        }
        
        void Hide()
        {
            isVisible = false;
            currentTarget = null;
            currentHealth = null;
            hideTimer = 0;
            healthBarPanel.SetActive(false);
            
        }
        void Show()
        {
            if (!isVisible)
            {
                isVisible = true;
                healthBarPanel.SetActive(true);
            }
            
        }
    }
}