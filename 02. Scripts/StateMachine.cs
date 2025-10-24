using System;
using System.Collections.Generic;
using UnityEngine;
/************************************
 [상태패턴]
 - 게임 캐릭터는 항상 하나의 상태를 가지고 있다.
 - 예를 들어, 대기, 공격, 이동같은거...

[상태머신]
- 지금 캐릭터가 어떤 상태인지 기억하고, 그 상태에 맞는 코드를 실행하다가
  조건이 되면 다음 상태로 바꿔주는 구조
- 상태를 등록하고, 실행하고, 바꾸는 관리자의 역할
 ************************************/
public class StateMachine : MonoBehaviour
{
    //상태이름을 키, 상태의 객체를 값으로저장
    private Dictionary<string, BaseState> stateDic=  new Dictionary<string, BaseState>();

    private BaseState curState;
    void Start()
    {
        curState.Enter();   
    }

    void Update()
    {
        //현재 상태가 해야할 행동 실행
        curState.Update();
        //상태 전이(변경) 조건확인. 필요하면 다른 상태로 바꾸는 로직을 판단
        curState.Transition();
    }
    private void LateUpdate()
    {
        curState.LateUpdate();
    }
    private void FixedUpdate()
    {
        curState.FixedUpdate();
    }
    public void InitState(string stateName)
    {
        curState = stateDic[stateName];
    }
    //상태를 상태머신에 등록
    public void AddState(string stateName, BaseState state)
    {
        state.SetStateMachine(this);
        stateDic.Add(stateName, state);
    }
    //변경
    public void ChangeState(string stateName)
    {
        curState.Exit();
        //딕셔너리에서 새 상태를 꺼내 현재 상태로 교체
        curState = stateDic[stateName];

        curState.Enter();
    }

    public void InitState<T>(T stateType) where T : Enum
    {
        InitState(stateType.ToString());
    }
    public void AddState<T>(T stateType, BaseState state) where T: Enum
    {
        AddState(stateType.ToString(), state);
    }
    public void ChangeState<T>(T stateType) where T : Enum 
    {
        ChangeState(stateType.ToString());    
    }

}
//모든 상태의 공통부모
// 개별상태(점프, 달리기, 대기 등)는 이 클래스를 상속해서 필요한 메서드만 재정의(override)
public class BaseState
{
    //자신이 속한 상태머신. 전이(ChangeState)를 하려면 상태머신이 필요함.
    private StateMachine stateMachine;

    public void SetStateMachine(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    //상태전이
    protected void ChangeState(string stateName)
    {
        //내부에 저장된 상태머신한테 이 이름 상태로 바꿈
        stateMachine.ChangeState(stateName);
    }
    protected void ChangeState<T>(T stateType) where T : Enum
    {
        ChangeState(stateType.ToString());
    }
    ///////////////////////////////////
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void LateUpdate() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
    public virtual void Transition() { }

}
