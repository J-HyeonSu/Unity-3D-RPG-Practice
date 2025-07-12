// ScriptableObject 기반 이벤트 시스템

using System.Collections.Generic;
using UnityEngine;

namespace RpgPractice
{
    /// <typeparam name="T">이벤트와 함께 전달할 데이터 타입</typeparam>
    public abstract class EventChannel<T> : ScriptableObject
    {
        // Observer 패턴의 구독자 목록을 HashSet으로 관리
        // HashSet으로 사용하는 이유는 중복등록방지, O(1) 추가/제거 성능
        private readonly HashSet<EventListener<T>> observers = new();

        // 이벤트 발행 메서드 - 모든 구독자에게 데이터 전달
        public void Invoke(T value)
        {
            // 모든 구독자들에게 순차적으로 이벤트 전달
            foreach (var obervers in observers)
            {
                obervers.Raise(value);
            }
        }

        // 구독자 등록 - EventListener가 자동으로 호출
        public void Register(EventListener<T> observer)
        {
            observers.Add(observer);
        }
        
        // 구독자 해제 - EventListener가 OnDestroy에서 자동 호출
        public void DeRegister(EventListener<T> observer)
        {
            observers.Remove(observer);
        }
    }


    // 데이터 없는 이벤트를 위한 빈 구조체
    // 공격 시작, 게임 종료 같은 단순 신호용
    public readonly struct Empty
    {
    }

    // 구체적인 이벤트 채널 클래스 - Empty 타입 특화
    [CreateAssetMenu(menuName = "Events/EventChannel")]
    public class EventChannel : EventChannel<Empty>
    {
    }
}
