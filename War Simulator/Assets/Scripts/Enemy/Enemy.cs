using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Enemy : BT
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

    GameObject target;
    Vector3 targetPos;
    NavMeshAgent agent;

    // 몬스터 기본정보 [나중에 문서화 예정]
    [SerializeField] float patrolDist = 30f;
    [SerializeField] float trackDist = 10f;
    [SerializeField] float eventDist = 3f;

    [SerializeField] float enemyHP = 10f, enemyMaxHP;
    [SerializeField] float enemyDmg = 10f;
    [SerializeField] float enemyAttackSpeed = 2f;

    [SerializeField] string teamTag = "";
    [SerializeField] string targetTag = "";

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targetPos = transform.position;

        enemyMaxHP = enemyHP;
    }

    void Start()
    {
        Init();
        StartCoroutine("BehaviourTree");
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
        patrol.AddChild(arrive);
        patrol.AddChild(move);

        dead.actionFunc = Dead;
        track.actionFunc = Track;
        attack.actionFunc = Attack;
        defense.actionFunc = Defense;
        arrive.actionFunc = Arrive;
        move.actionFunc = Move;
    }

    public override IEnumerator BehaviourTree()
    {
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
                Debug.Log("순찰");
                agent.isStopped = false;
                agent.destination = targetPos;
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
        } else {
            agent.isStopped = false;
            agent.destination = targetPos;
            return true;
        }
    }

    // 가장 가까운 적을 찾고 그 적이 추적 범위안에 들어오면 추적시킴
    bool Track()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < trackDist)
        {
            Debug.Log("추적");
            agent.isStopped = false;
            agent.destination = target.transform.position;

            transform.LookAt(target.transform);
            return true;
        }
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 공격 범위안에 들어오면 때림
    bool Attack()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < eventDist)
        {
            Debug.Log("공격");
            agent.isStopped = false;
            agent.destination = target.transform.position;

            StartCoroutine(Delay(enemyAttackSpeed));

            return true;
        }
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 방어 범위안에 들어오면 방어
    bool Defense()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < eventDist)
        {
            Debug.Log("방어");
            agent.isStopped = false;
            agent.destination = target.transform.position;
            return true;
        }
        return false;
    }

    // HP 다 떨어지면 사망
    bool Dead()
    {
        if (enemyHP < 0f)
        {
            StopCoroutine("BehaviourTree");
            gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    IEnumerator Delay(float delay)
    {
        float curTime = 0.0f;

        while (curTime <= delay)
        {
            curTime += Time.deltaTime;
            yield return null;  
        }
        // 임시 공격
        GameObject attack = ObjectMgr.SpawnPool("Attack", transform.position, transform.rotation);

        Vector3 dir = target.transform.position - transform.position;
        dir.Normalize(); print(dir);

        attack.GetComponent<Rigidbody>().velocity += dir * 5.0f;
        // 임시 공격
    }
}