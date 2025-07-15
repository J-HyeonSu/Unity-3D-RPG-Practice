using TMPro;
using UnityEngine;

namespace RpgPractice
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        public GameObject damagePopupPrefab;
        [SerializeField] private TextMeshProUGUI levelText;
        private void Awake()
        {
            instance = this;
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