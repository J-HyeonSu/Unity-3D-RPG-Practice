using System;
using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{
    public static class AttackRangeDetector
    {

        //원형 범위 공격
        public static List<Transform> GetTargetsInCircle(Vector3 center, float radius, LayerMask targetMask)
        {
            List<Transform> targets = new List<Transform>();
            Collider[] colliders = Physics.OverlapSphere(center, radius, targetMask);

            foreach (var collider in colliders)
            {
                targets.Add(collider.transform);
            }

            return targets;
        }
        
        //부채꼴 범위 공격
        public static List<Transform> GetTargetsInCone(Vector3 center, Vector3 forward, float radius, float angle, LayerMask targetMask)
        {
            List<Transform> targets = new List<Transform>();
            Collider[] colliders = Physics.OverlapSphere(center, radius, targetMask);

            foreach (var collider in colliders)
            {
                //센터 -> 타겟의 방향
                Vector3 directionToTarget = (collider.transform.position - center).normalized;
                //정면에서부터 타겟까지의 각도(0~180) 반환
                float angleToTarget = Vector3.Angle(forward, directionToTarget);
                
                if (angleToTarget <= angle / 2f)
                {
                    targets.Add(collider.transform);
                }
                
            }

            return targets;
        }

        //직선 범위 공격
        public static List<Transform> GetTargetsInLine(Vector3 start, Vector3 direction, float length, float width, LayerMask targetMask)
        {
            List<Transform> targets = new List<Transform>();
            Vector3 boxCenter = start + direction * (length / 2f);
            Vector3 boxSize = new Vector3(width, 2f, length);

            Collider[] colliders =
                Physics.OverlapBox(boxCenter, boxSize / 2f, Quaternion.LookRotation(direction), targetMask);

            foreach (var collider in colliders)
            {
                targets.Add(collider.transform);
            }

            return targets;
        }

        //도넛 범위 공격
        public static List<Transform> GetTargetsInDonut(Vector3 center, float outerRadius, float innerRadius, LayerMask targetMask)
        {
            List<Transform> targets = new List<Transform>();
            Collider[] colliders = Physics.OverlapSphere(center, outerRadius, targetMask);

            foreach (var collider in colliders)
            {
                float distance = Vector3.Distance(center, collider.transform.position);
                if (distance >= innerRadius && distance <= outerRadius)
                {
                    targets.Add(collider.transform);
                }
            }

            return targets;
        }
        
        //사각형 범위 공격
        public static List<Transform> GetTargetsInRectangle(Vector3 center, Vector3 forward, float length, float width, LayerMask targetMask)
        {
            List<Transform> targets = new List<Transform>();
            Vector3 boxSize = new Vector3(width, 2f, length);
            
            Collider[] colliders = Physics.OverlapBox(center, boxSize / 2f, Quaternion.LookRotation(forward), targetMask);
            
            foreach (var collider in colliders)
            {
                targets.Add(collider.transform);
            }
            
            return targets;
        }
        
        //통합 범위감지함수
        public static List<Transform> GetTargetsInRange(BossAttackData attackData, Vector3 center, Vector3 forward,
            LayerMask targetMask)
        {
            switch (attackData.rangeType)
            {
                case AttackRangeType.Circle:
                    return GetTargetsInCircle(center, attackData.range, targetMask);
                case AttackRangeType.Cone:
                    return GetTargetsInCone(center, forward, attackData.range, attackData.angle, targetMask);
                case AttackRangeType.Line:
                    return GetTargetsInLine(center, forward, attackData.range, attackData.width, targetMask);
                case AttackRangeType.Donut:
                    return GetTargetsInDonut(center, attackData.range, attackData.innerRadius, targetMask);
                case AttackRangeType.Rectangle:
                    return GetTargetsInRectangle(center, forward, attackData.range, attackData.width, targetMask);
                default:
                    return new List<Transform>();
            }
        }
        
    }
}