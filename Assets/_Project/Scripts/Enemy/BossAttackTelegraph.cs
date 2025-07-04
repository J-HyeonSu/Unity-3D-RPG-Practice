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
        [SerializeField] private DecalProjector backgroundDecalProjector;

        [SerializeField] private Material circleMaterial;
        [SerializeField] private Material cone180Material;
        [SerializeField] private Material cone90Material;
        [SerializeField] private Material boxMaterial;

        private BossAttackData currentAttackData;
        private Coroutine telegraphCoroutine;


        public event Action<BossAttackData, Vector3, Vector3> OnTelegraphComplete;
        
        
        

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
            
            
            OnTelegraphComplete?.Invoke(currentAttackData, center, forward);
            //텔레그래프 종료
            HideRangeIndicator();
        }

        void ShowRangeIndicator(Vector3 center, Vector3 forward)
        {
            
            if (!decalProjector) return;
            if (!backgroundDecalProjector) return;
            
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
            SetProjectorMaterial(circleMaterial);

            SetProjectorPosition(center);
            
            
            decalProjector.enabled = true;
            
            backgroundDecalProjector.enabled = true;
            backgroundDecalProjector.size = new Vector3(radius * 2, radius*2, decalProjector.size.z);
            
        }

        void ShowConeIndicator(Vector3 center, Vector3 forward, float range, float angle)
        {
            if (angle <= 90)
            {
                SetProjectorMaterial(cone90Material);
            }
            else if (angle <= 180)
            {
                SetProjectorMaterial(cone180Material);
            }

            transform.rotation = Quaternion.LookRotation(forward);
            
            SetProjectorPosition(center);
            
            
            decalProjector.enabled = true;
            
            backgroundDecalProjector.enabled = true;
            backgroundDecalProjector.size = new Vector3(range * 2, range*2, decalProjector.size.z);
            
        }

        void ShowLineIndicator(Vector3 center, Vector3 forward, float length, float width)
        {
            SetProjectorMaterial(boxMaterial);
            
            SetProjectorPosition(center);
            decalProjector.pivot = new Vector3(0, 0.5f, decalProjector.pivot.z);
            
            transform.rotation = Quaternion.LookRotation(forward);

            decalProjector.enabled = true;
            backgroundDecalProjector.enabled = true;

            
            backgroundDecalProjector.pivot = new Vector3(0, length/2, backgroundDecalProjector.pivot.z);
            
            backgroundDecalProjector.size = new Vector3(width, length, decalProjector.size.z);
            
        }

        void ShowDonutIndicator(Vector3 center, float outerRadius, float innerRadius)
        {
            ShowCircleIndicator(center, outerRadius);
        }
        
        void ShowRectangleIndicator(Vector3 center, Vector3 forward, float length, float width)
        {
            SetProjectorMaterial(boxMaterial);
            SetProjectorPosition(center);
            
            transform.rotation = Quaternion.LookRotation(forward);

            decalProjector.enabled = true;
            backgroundDecalProjector.enabled = true;

            backgroundDecalProjector.size = new Vector3(width, length, decalProjector.size.z);
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
            if (decalProjector && backgroundDecalProjector)
            {
                if (currentAttackData.rangeType == AttackRangeType.Line)
                {
                    //라인 일경우
                    //직선으로 뻗어나가는것만 표시
                    decalProjector.pivot = new Vector3(0, backgroundDecalProjector.pivot.y*multiplier, decalProjector.pivot.z);
                    
                    decalProjector.size = new Vector3(backgroundDecalProjector.size.x, backgroundDecalProjector.size.y*multiplier, decalProjector.size.z);
                }
                else
                {
                    //나머지의경우 중심부터 퍼저나가는 효과
                    decalProjector.size = new Vector3(backgroundDecalProjector.size.x * multiplier, backgroundDecalProjector.size.y*multiplier, decalProjector.size.z);
                }
                
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
                decalProjector.pivot = new Vector3(0, 0, decalProjector.pivot.z);
            }

            if (backgroundDecalProjector)
            {
                backgroundDecalProjector.enabled = false;
                backgroundDecalProjector.pivot = new Vector3(0, 0, backgroundDecalProjector.pivot.z);
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

        void SetProjectorMaterial(Material material)
        {
            if (!decalProjector && !backgroundDecalProjector) return;
            decalProjector.material = material;
            backgroundDecalProjector.material = material;
        }

        void SetProjectorPosition(Vector3 position)
        {
            if (!decalProjector && !backgroundDecalProjector) return;
            decalProjector.transform.position = position;
            backgroundDecalProjector.transform.position = position;
        }
    }
}