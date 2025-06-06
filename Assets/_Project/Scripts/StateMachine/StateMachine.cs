using System;
using System.Collections.Generic;

namespace RpgPractice
{
    public class StateMachine 
    {
        /*
         StateMachine
            │
            ├── Dictionary<Type, StateNode> nodes
            │   └── StateNode
            │       ├── IState State
            │       └── HashSet<ITransition> Transitions
            │           └── Transition : ITransition
            │               ├── IState To
            │               └── IPredicate Condition
            │
            ├── HashSet<ITransition> anyTransitions
            │   └── Transition (조건만 맞으면 어떤 상태에서든 이동)
            │
            ├── 현재 상태: StateNode current
            │   └── StateNode.State : IState
            │       └── BaseState : IState
            │           ├── LocomotionState
            │           ├── JumpState
            │           └── DashState
            │
            └── 상태 전이 조건: IPredicate
                └── FuncPredicate(Func<bool>)
         
         */
        
        //현재 활성상태 노드
        StateNode current;
        //모든 상태 노드를 타입기반 저장
        // Key : 상태 타입(typeof(IdleState)), Value: 해당 상태의 노드
        Dictionary<Type, StateNode> nodes = new();
        
        //어떤 상태에서든 발생가능한 전환들
        HashSet<ITransition> anyTransitions = new();

        public void Update() 
        {
            //현재 상태에서 전환가능한 조건이 있는지 체크
            var transition = GetTransition();
            
            //전환조건이 만족되면 새로운 상태로 변환
            if (transition != null) 
                ChangeState(transition.To);
            
            //현재 상태의 Update 실행
            current.State?.Update();
        }
        
        public void FixedUpdate() 
        {
            current.State?.FixedUpdate();
        }

        
        
        //외부에서 직접 상태를 설정할 때 사용
        public void SetState(IState state) 
        {
            current = nodes[state.GetType()];
            current.State?.OnEnter();
        }
        
        
        //상태 전환 실행
        void ChangeState(IState state) 
        {
            //같은상태이면 무시
            if (state == current.State) return;
            
            //이전상태와 다음상태 준비
            var previousState = current.State;
            var nextState = nodes[state.GetType()].State;
            
            previousState?.OnExit();
            nextState?.OnEnter();
            
            //현재상태갱신
            current = nodes[state.GetType()];
        }

        //전환가능한 조건을 찾는 함수
        ITransition GetTransition() 
        {
            //AnyTransition 체크
            foreach (var transition in anyTransitions)
                if (transition.Condition.Evaluate())
                    return transition;
            
            //현재 상태에서만 가능한 전환 체크
            foreach (var transition in current.Transitions)
                if (transition.Condition.Evaluate())
                    return transition;
            
            //조건업으면 null
            return null;
        }

        //상태간 전환 규칙 추가
        //from: 출발상태, to: 도착상태, condition: 전환조건
        public void AddTransition(IState from, IState to, IPredicate condition) 
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }
        
        //어떤 상태에서든 가능한 전환규칙 추가
        //to: 도착상태, condition: 전환조건
        public void AddAnyTransition(IState to, IPredicate condition) 
        {
            anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }

        //상태의 노드를 가져오거나, 없으면 새로 만들어서 반환
        StateNode GetOrAddNode(IState state) 
        {
            //딕셔너리에서 해당 상태 타입의 노드 찾기
            var node = nodes.GetValueOrDefault(state.GetType());
            
            //노드가 없으면 새로 생성
            if (node == null) {
                //새 노드생성
                node = new StateNode(state);
                //딕셔너리에 추가
                nodes.Add(state.GetType(), node);
            }
            
            return node;
        }

        // 상태 노드 클래스 (상태 + 그 상태에서 가능한 전환들)
        class StateNode 
        {
            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }
            
            public StateNode(IState state) {
                State = state;
                Transitions = new HashSet<ITransition>();
            }
            
            public void AddTransition(IState to, IPredicate condition) {
                Transitions.Add(new Transition(to, condition));
            }
        }
    }
    
}