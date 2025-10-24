using UnityEngine;

public class Eagle : Monster
{
    //상태 정의
    public enum State { Idle, Trace, Return}
    [SerializeField] float moveSpeed;
    [SerializeField] float findRange;
    [SerializeField] private Transform target;

    private StateMachine stateMachine;
    private Vector2 startPos;
    private void Awake()
    {
        stateMachine = gameObject.AddComponent<StateMachine>();

        //상태등록
        stateMachine.AddState(State.Idle, new IdleState(this));     //대기
        stateMachine.AddState(State.Trace, new TraceState(this));   //추적
        stateMachine.AddState(State.Return, new ReturnState(this)); //복귀

        //첫 상태를 Idle로 설정
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
    //모든 독수리 상태의 공통 부모 클래스
    private class EagleState : BaseState
    {
        protected Eagle owner;

        //owner(독수리)의 트랜스폼을 가져옴
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
    //Idle :플레이어를 찾기전 대기상태
    private class IdleState : EagleState
    {
        public IdleState(Eagle owner) : base(owner) { }

        public override void Transition()
        {
            //플레이어가 일정거리 안에 들어오면 추적상태로 전환
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
            //방향계산
            Vector2 dir = (target.position - transform.position).normalized;
            //이동
            transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);
        }
        public override void Transition()
        {
            //탐지 범위를 벗어나면 복귀상태로 전환
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
