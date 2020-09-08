using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Assassin : BT
{
    #region Behaviour Tree

    Selector root = new Selector();

    Actions dead = new Actions();
    Selector sel = new Selector();

    Selector selEvent = new Selector();
    Actions track = new Actions();
    Sequence patrol = new Sequence();

    Actions attack = new Actions();
    Actions defense = new Actions();

    Actions arrive = new Actions();
    Actions move = new Actions();

    #endregion


    bool startBT;


    List<Dictionary<string, object>> data;

    GameObject target;
    NavMeshAgent agent;
    Animator animator;
    Vector3 targetPos;

    [SerializeField] float patrolDist;
    [SerializeField] float trackDist;
    [SerializeField] float eventDist;

    [SerializeField] float enemyHP, enemyMaxHP;
    [SerializeField] float enemyDmg;
    [SerializeField] float enemyAttackSpeed;

    [SerializeField] string teamTag = "A";
    [SerializeField] string targetTag = "B";

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        targetPos = transform.position;
        data = DataMgr.Read("ClassStats");

        //InitStat();
        enemyMaxHP = enemyHP;
    }

    void Start()
    {
        //print(gameObject.GetComponent<Assassin>().enabled);
        Init();
        //StartCoroutine("BehaviourTree");
    }

    private void Update()
    {
        if (!UIMgr.gameStart)
        {
            StopAllCoroutines();
            agent.isStopped = true;
            startBT = false;
            animator.SetBool("isMoving", false);
            animator.SetBool("isDead", false);
            animator.SetBool("isAttack", false);
        }
        else if (!startBT)
        {
            StartCoroutine(BehaviourTree());
            startBT = true;
        }
    }

    void InitStat()
    {
        patrolDist = (int)data[(int)Class.Ninja]["Dist1"];
        trackDist = (int)data[(int)Class.Ninja]["Dist2"];
        eventDist = (int)data[(int)Class.Ninja]["Dist3"];

        enemyHP = (int)data[(int)Class.Ninja]["HP"];
        enemyDmg = (int)data[(int)Class.Ninja]["Damage"];
        agent.speed = (int)data[(int)Class.Ninja]["MoveSpeed"];
        enemyAttackSpeed = (int)data[(int)Class.Ninja]["AttackSpeed"];
    }

    public override void Init()
    {
        // 기본적인 뿌리
        root.AddChild(dead);
        root.AddChild(sel);

        // 이벤트[공/방] / 추적 / 순찰
        sel.AddChild(selEvent);
        sel.AddChild(track);
        sel.AddChild(patrol);

        // 공/방
        selEvent.AddChild(attack);
        selEvent.AddChild(defense);

        // 순찰
        patrol.AddChild(move);
        patrol.AddChild(arrive);

        dead.actionFunc = Dead;
        track.actionFunc = Track;
        attack.actionFunc = Attack;
        defense.actionFunc = Defense;
        move.actionFunc = Move;
        arrive.actionFunc = Arrive;

    }

    public override IEnumerator BehaviourTree()
    {
        print("암살자 : Start BT");
        // Enter

        while (!root.Invoke())
        {
            // Update

            yield return null;
        }

        yield return new WaitForSeconds(2f);
        // Exit
        CheckBT();
    }

    void CheckBT()
    {
        //print("Check BT");

        if (gameObject.activeSelf == true)
            StartCoroutine("BehaviourTree");
        else
            Debug.Log("STOP BT");
    }

    // 기본적인 AI를 BT 클래스에 넣을수도 있지만 나중에 수정할 수도 있으니까 그냥 나둠
    bool Move()
    {
        agent.isStopped = true;
        while (true)
        {
            // 새로운 좌표를 찍어줌
            targetPos = (Math.RandomSphereInPoint(30, transform));
            if (Physics.Raycast(targetPos, Vector3.down, 5f))
            {
                // 밑에 바닥임
                //Debug.Log("순찰");
                //print("현위치 :" + transform.localPosition + "도착위치 :" + targetPos);
                agent.isStopped = false;
                //agent.SetDestination(targetPos);
                agent.destination = targetPos;
                animator.SetBool("isMoving", true);
                return true;
            }
        }
    }

    bool Arrive()
    {

        float dist = Vector3.Distance(targetPos, transform.position);
        if (dist < 1f) // 순찰지점 도착 
        {
            Debug.Log("순찰지점 도착");
            return true;
        }
        else
        {
            agent.isStopped = false;
            agent.destination = targetPos;
            return true;
        }
    }

    bool CheckingShieldSoldier(GameObject target)       //방패병인지 아닌지 체크해주는 함수
    {
        if (target.GetComponent<Shield>() != null)
            return true;
        else return false;
    }

    // 가장 가까운 적을 찾고 그 적이 추적 범위안에 들어오면 추적시킴
    bool Track()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        
        //적이 방패병이면 방패병을 피해서 아군한테 감 아닐시 그냥 적 공격
        if (!CheckingShieldSoldier(target))     //방패병이 아닐때
        {
            float dist = Vector3.Distance(target.transform.position, transform.position);
            if (dist < trackDist)
            {
                Debug.Log("암살자:추적");
                agent.isStopped = false;
                agent.destination = target.transform.position;
                animator.SetBool("isMoving", true);
                return true;
            }
        }
        else   //방패병 피해서 아군추적
        {
            print("암살자 : 방패병 피해서 아군추적");
            target = Math.FindClosestTarget(teamTag, transform);
            float dist = Vector3.Distance(target.transform.position, transform.position);
            if (dist < trackDist)
            {
                Debug.Log("피해서 추적");
                agent.isStopped = false;
                agent.destination = target.transform.position;
                animator.SetBool("isMoving", true);
                return true;
            }
        }
        
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 공격 범위안에 들어오면 때림
    bool Attack()
    {

        target = Math.FindClosestTarget(targetTag, transform);
        float dist = Vector3.Distance(target.transform.position, transform.position);
        if (dist < eventDist)
        {
            Debug.Log("공격");
            agent.isStopped = false;
            agent.destination = target.transform.position;
            animator.SetBool("isAttack", true);
            animator.SetBool("isMoving", false);
            return true;
        }
        
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 방어 범위안에 들어오면 방어
    bool Defense()
    {

        target = Math.FindClosestTarget(targetTag, transform);
        float dist = Vector3.Distance(target.transform.position, transform.position);
        if (dist < eventDist)
        {
            Debug.Log("방어");
            agent.isStopped = false;
            agent.destination = target.transform.position;
            //agent.SetDestination(target.transform.position);
            return true;
        }
       
        return false;
    }

    // HP 다 떨어지면 사망
    bool Dead()
    {

        if (enemyHP < 0f)
        {
            animator.SetBool("isDead", true);
            StopCoroutine("BehaviourTree");
            gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("WarriorMAttack"))
        {
            enemyHP -= (int)data[(int)Class.Warrior_M]["Damage"];
        }
        if (other.name.Contains("WarriorWAttack"))
        {
            enemyHP -= (int)data[(int)Class.Warrior_W]["Damage"];
        }
        if (other.name.Contains("AcherAttack"))
        {
            enemyHP -= (int)data[(int)Class.Archer]["Damage"];
        }
        if (other.name.Contains("GunnerAttack"))
        {
            enemyHP -= (int)data[(int)Class.Gunner]["Damage"];
        }
        if (other.name.Contains("NinjaAttack"))
        {
            enemyHP -= (int)data[(int)Class.Ninja]["Damage"];
        }
    }
}
