using UnityEngine;
/******************************************
 [State Pattern]
 - ���� �� ��ü�� � ��Ȳ�� �ֳ�?
 - ��ü�� �ѹ��� �ϳ��� ���¸� ����
 - ��ü�� ���� ���¿� �ش��ϴ� �ൿ�� �����ϵ��� �ϴ� ����
 

[���� ����]
1. ������ �Ǵ� Ŭ������ ��ü�� ������ �ִ� ���µ��� ����
2. ���� ���¸� �����ϴ� ������ �����, �ʱ� ���¸� ����
3. ��ü�� ���� ���¿� ���� �ൿ�� ����
4. �ൿ ���� �� ���� ��ȯ������ �Ǵ�
5. ���� ��ȯ�� �ʿ��ϸ� ���� ���¸� ���ο� ���·� ����
6. ���� ������ (�Ǵ� ȣ��)������ ����� ������ �ൿ�� �����Ѵ�.

[����]
 1. ���ǹ��� ���·� ��ü�ϹǷ� ������ ���� ó���� �δ��� �پ��
 2. ���� ������ ���길 �����ϹǷ� �ڵ��� ȿ������ ����.
 3. �� ���º� ������ �и��Ǿ� �ڵ尡 �����ϰ� �������� ����.
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
