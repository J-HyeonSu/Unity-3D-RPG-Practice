using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RpgPractice
{
    public class BossAttackTelegraph : MonoBehaviour
    {
        [Header("시각적 피드백")] 
        [SerializeField] private ParticleSystem chargeEffect;
        [SerializeField] private DecalProjector decalProjector;
        [SerializeField] private DecalProjector decalProjector2;


        private Material decalMaterial;
        private Material decalMaterial2;

        private BossAttackData currentAttackData;
        private Coroutine telegraphCoroutine;

        private void Start()
        {
            if (decalProjector && decalProjector.material)
            {
                decalMaterial = new Material(decalProjector.material);
                decalProjector.material = decalMaterial;
                DebugMaterialProperties();
            }
            if (decalProjector2 && decalProjector2.material)
            {
                decalMaterial2 = new Material(decalProjector2.material);
                decalProjector2.material = decalMaterial2;
            }
        }
        
        void DebugMaterialProperties()
        {
            if (decalMaterial == null) return;
        
            Shader shader = decalMaterial.shader;
            int propertyCount = ShaderUtil.GetPropertyCount(shader);
        
            Debug.Log($"Material has {propertyCount} properties:");
            for (int i = 0; i < propertyCount; i++)
            {
                string propertyName = ShaderUtil.GetPropertyName(shader, i);
                ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(shader, i);
                Debug.Log($"Property {i}: {propertyName} ({propertyType})");
            }
        }

        public void ShowTelegraph(BossAttackData attackData, Vector3 center, Vector3 forward)
        {
            currentAttackData = attackData;
            if (telegraphCoroutine != null)
            {
                StopCoroutine(telegraphCoroutine);
            }

            telegraphCoroutine = StartCoroutine(TelegraphCoroutine(center, forward));
        }

        private IEnumerator TelegraphCoroutine(Vector3 center, Vector3 forward)
        {
            // 범위 시각화 시작
            ShowRangeIndicator(center, forward);

            if (chargeEffect)
            {
                chargeEffect.Play();
            }
            
            // 사전 시간 대기 (점점 강해지는 효과)
            float elapsed = 0f;
            Color originalColor = GetRangeColor();

            while (elapsed < currentAttackData.castTime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / currentAttackData.castTime;

                //색상강도증가
                Color currentColor = Color.Lerp(originalColor, Color.red, progress);
                currentColor.a = Mathf.Lerp(0.3f, 0.8f, progress);
                SetRangeColor(currentColor);
                
                //크기약간증가
                //float scaleMultiplier = 1f + (progress * 0.1f);
                //SetRangeScale(scaleMultiplier);
                SetRangeScale(progress);
                yield return null;
            }
            
            //텔레그래프 종료
            HideRangeIndicator();
        }

        void ShowRangeIndicator(Vector3 center, Vector3 forward)
        {
            switch (currentAttackData.rangeType)
            {
                case AttackRangeType.Circle:
                    ShowCircleIndicator(center, currentAttackData.range);
                    break;
                    
                case AttackRangeType.Cone:
                    ShowConeIndicator(center, forward, currentAttackData.range, currentAttackData.angle);
                    break;
                    
                case AttackRangeType.Line:
                    ShowLineIndicator(center, forward, currentAttackData.range, currentAttackData.width);
                    break;
                    
                case AttackRangeType.Donut:
                    ShowDonutIndicator(center, currentAttackData.range, currentAttackData.innerRadius);
                    break;
                    
                case AttackRangeType.Rectangle:
                    ShowRectangleIndicator(center, forward, currentAttackData.range, currentAttackData.width);
                    break;
            }
        }

        void ShowCircleIndicator(Vector3 center, float radius)
        {

            if (decalProjector)
            {
                decalProjector.enabled = true;
                
            }

            if (decalProjector2)
            {
                decalProjector2.enabled = true;
                decalProjector2.size = new Vector3(radius * 2, radius*2, decalProjector.size.z);
            }
        }

        void ShowConeIndicator(Vector3 center, Vector3 forward, float range, float angle)
        {
            if (decalProjector)
            {
                Debug.Log(decalMaterial);
                decalMaterial.SetFloat("_Angle", angle);
                decalMaterial.SetFloat("_StartAngle", 0);
                decalProjector.enabled = true;
            }

            if (decalProjector2)
            {
                decalMaterial2.SetFloat("_Angle", angle);
                decalMaterial2.SetFloat("_StartAngle", 0);
                decalProjector2.enabled = true;
                decalProjector2.size = new Vector3(range * 2, range*2, decalProjector.size.z);
            }
        }

        void ShowLineIndicator(Vector3 center, Vector3 forward, float length, float width)
        {
            // if (warningDecal)
            // {
            //     warningDecal.SetActive(true);
            //     Vector3 decalPosition = center + forward * (length / 2f);
            //     warningDecal.transform.position = decalPosition;
            //     warningDecal.transform.rotation = Quaternion.LookRotation(forward);
            //     warningDecal.transform.localScale = new Vector3(width, 1f, length);
            // }
        }

        void ShowDonutIndicator(Vector3 center, float outerRadius, float innerRadius)
        {
            ShowCircleIndicator(center, outerRadius);
        }
        
        void ShowRectangleIndicator(Vector3 center, Vector3 forward, float length, float width)
        {
            // if (warningDecal)
            // {
            //     warningDecal.SetActive(true);
            //     warningDecal.transform.position = center;
            //     warningDecal.transform.rotation = Quaternion.LookRotation(forward);
            //     warningDecal.transform.localScale = new Vector3(width, 1f, length);
            // }
        }
        
        private Color GetRangeColor()
        {
            return currentAttackData.rangeColor;
        }
        
        private void SetRangeColor(Color color)
        {
            // if (lineRenderer)
            // {
            //     lineRenderer.startColor = color;
            //     lineRenderer.endColor = color;
            // }
            //
            // if (warningDecal)
            // {
            //     var renderer = warningDecal.GetComponent<Renderer>();
            //     if (renderer)
            //     {
            //         renderer.material.color = color;
            //     }
            // }
        }
        
        private void SetRangeScale(float multiplier)
        {
            if (decalProjector && decalProjector2)
            {
                decalProjector.size = new Vector3(decalProjector2.size.x * multiplier, decalProjector2.size.y*multiplier,
                    decalProjector.size.z);
            }
        }
        
        private void HideRangeIndicator()
        {
            
            if (chargeEffect)
            {
                chargeEffect.Stop();
            }

            if (decalProjector)
            {
                decalProjector.enabled = false;
            }

            if (decalProjector2)
            {
                decalProjector2.enabled = false;
            }
        }
        
        public void CancelTelegraph()
        {
            if (telegraphCoroutine != null)
            {
                StopCoroutine(telegraphCoroutine);
                telegraphCoroutine = null;
            }
            
            HideRangeIndicator();
        }
        
    }
}