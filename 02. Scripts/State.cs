using UnityEngine;
/******************************************
 [State Pattern]
 - 지금 이 객체가 어떤 상황에 있냐?
 - 객체가 한번에 하나의 상태만 가짐
 - 객체는 현재 상태에 해당하는 행동만 수행하도록 하는 패턴
 

[구현 절차]
1. 열거형 또는 클래스로 객체가 가질수 있는 상태들을 정의
2. 현재 상태를 저장하는 변수를 만들고, 초기 상태를 지정
3. 객체는 현재 상태에 따른 행동만 수행
4. 행동 수행 후 상태 전환조건을 판단
5. 상태 전환이 필요하면 현재 상태를 새로운 상태로 변경
6. 다음 프레임 (또는 호출)에서는 변경된 상태의 행동만 수행한다.

[장점]
 1. 조건문을 상태로 대체하므로 복잡한 조건 처리의 부담이 줄어듬
 2. 현재 상태의 연산만 수행하므로 코드의 효율성이 좋다.
 3. 각 상태별 동작이 분리되어 코드가 간결하고 가독성이 좋다.
 ******************************************/
public class Mobile
{
    public enum State { Off, Normal,charge, FullCharged}

    private State state = State.Normal;
    private bool charging = false;
    private float battery = 50.0f;


    private void Update() 
    {
        switch (state)
        {
            case State.Off:
                OffUpdate();
                break;
            case State.Normal:
                NormalUpdate();
                break;
            case State.FullCharged:
                FullChargedUpdate();
                break;
        }
    }

    private void OffUpdate()
    {
        if(charging)
        {
            state = State.charge;       
        }
    }
    private void NormalUpdate()
    {
        battery -= 1.5f * Time.deltaTime;

        if (charging)
        {
            state = State.charge;
        }
        else if (battery <= 0)
        {
            state = State.Off;
        }
    }
    private void FullChargedUpdate()
    {
        if(!charging)
        {
            state = State.Normal;
        }
    }
    public void ConnectCharger()
    {
        charging = true;
    }
    public void DisConnectCharger()
    {
        charging = false;
    }
}
