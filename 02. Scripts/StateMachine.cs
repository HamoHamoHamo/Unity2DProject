using System;
using System.Collections.Generic;
using UnityEngine;
/************************************
 [��������]
 - ���� ĳ���ʹ� �׻� �ϳ��� ���¸� ������ �ִ�.
 - ���� ���, ���, ����, �̵�������...

[���¸ӽ�]
- ���� ĳ���Ͱ� � �������� ����ϰ�, �� ���¿� �´� �ڵ带 �����ϴٰ�
  ������ �Ǹ� ���� ���·� �ٲ��ִ� ����
- ���¸� ����ϰ�, �����ϰ�, �ٲٴ� �������� ����
 ************************************/
public class StateMachine : MonoBehaviour
{
    //�����̸��� Ű, ������ ��ü�� ����������
    private Dictionary<string, BaseState> stateDic=  new Dictionary<string, BaseState>();

    private BaseState curState;
    void Start()
    {
        curState.Enter();   
    }

    void Update()
    {
        //���� ���°� �ؾ��� �ൿ ����
        curState.Update();
        //���� ����(����) ����Ȯ��. �ʿ��ϸ� �ٸ� ���·� �ٲٴ� ������ �Ǵ�
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
    //���¸� ���¸ӽſ� ���
    public void AddState(string stateName, BaseState state)
    {
        state.SetStateMachine(this);
        stateDic.Add(stateName, state);
    }
    //����
    public void ChangeState(string stateName)
    {
        curState.Exit();
        //��ųʸ����� �� ���¸� ���� ���� ���·� ��ü
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
//��� ������ ����θ�
// ��������(����, �޸���, ��� ��)�� �� Ŭ������ ����ؼ� �ʿ��� �޼��常 ������(override)
public class BaseState
{
    //�ڽ��� ���� ���¸ӽ�. ����(ChangeState)�� �Ϸ��� ���¸ӽ��� �ʿ���.
    private StateMachine stateMachine;

    public void SetStateMachine(StateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }
    //��������
    protected void ChangeState(string stateName)
    {
        //���ο� ����� ���¸ӽ����� �� �̸� ���·� �ٲ�
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
