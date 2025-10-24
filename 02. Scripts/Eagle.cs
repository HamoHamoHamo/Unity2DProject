using UnityEngine;

public class Eagle : Monster
{
    //���� ����
    public enum State { Idle, Trace, Return}
    [SerializeField] float moveSpeed;
    [SerializeField] float findRange;
    [SerializeField] private Transform target;

    private StateMachine stateMachine;
    private Vector2 startPos;
    private void Awake()
    {
        stateMachine = gameObject.AddComponent<StateMachine>();

        //���µ��
        stateMachine.AddState(State.Idle, new IdleState(this));     //���
        stateMachine.AddState(State.Trace, new TraceState(this));   //����
        stateMachine.AddState(State.Return, new ReturnState(this)); //����

        //ù ���¸� Idle�� ����
        stateMachine.InitState(State.Idle);
    }

    void Start()
    {
        if(target==null)
        {
            target = GameObject.FindWithTag("Player")?.transform;
        }
        startPos = transform.position;
    }
    //��� ������ ������ ���� �θ� Ŭ����
    private class EagleState : BaseState
    {
        protected Eagle owner;

        //owner(������)�� Ʈ�������� ������
        protected Transform transform
        {
            get { return owner.transform; }
        }
        protected float moveSpeed
        {
            get { return owner.moveSpeed; }
        }
        protected Transform target
        {
            get { return owner.target; }
        }
        protected Vector2 startPos
        {
            get { return owner.startPos; }
        }
        protected float findRange
        {
            get { return owner.findRange; }
        }
        public EagleState(Eagle owner)
        {
            this.owner = owner;
        }
    }
    //Idle :�÷��̾ ã���� ������
    private class IdleState : EagleState
    {
        public IdleState(Eagle owner) : base(owner) { }

        public override void Transition()
        {
            //�÷��̾ �����Ÿ� �ȿ� ������ �������·� ��ȯ
            if(Vector2.Distance(target.position,transform.position)<findRange)
            {
                ChangeState(State.Trace);
            }
        }
    }
    private class TraceState : EagleState
    {
        public TraceState(Eagle owner) : base(owner) { }

        public override void Update()
        {
            //������
            Vector2 dir = (target.position - transform.position).normalized;
            //�̵�
            transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        }
        public override void Transition()
        {
            //Ž�� ������ ����� ���ͻ��·� ��ȯ
            if(Vector2.Distance(target.position, transform.position)> findRange)
            {
                ChangeState(State.Return);
            }
        }
    }
    private class ReturnState : EagleState
    {
        public ReturnState(Eagle owner) : base(owner) { }

        public override void Update()
        {
            Vector2 dir = ((Vector3)startPos - transform.position).normalized;

            transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        }
        public override void Transition()
        {
            if(Vector2.Distance(startPos, transform.position)<0.1f)
            {
                ChangeState(State.Idle);
            }
        }
    }
}
