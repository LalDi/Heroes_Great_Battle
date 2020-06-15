using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Enemy : BT
{
    #region Behaviour Tree

    Selector root = new Selector();

    Action dead = new Action();
    Selector sel = new Selector();

    Selector selEvent = new Selector();
    Action track = new Action();
    Sequence patrol = new Sequence();

    Action attack = new Action();
    Action defense = new Action();

    Action arrive = new Action();
    Action move = new Action();

    #endregion

    GameObject target;
    NavMeshAgent agent;
    /*
     * destination : 추적 대상 
     * isstopped : 추적 ON
     */
    Vector3 targetPos;

    float eventDist = 3f;
    float trackDist = 10f;

    float enemyHP = 10f, enemyMaxHP;

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
        print("Start BT");
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
        print("Check BT");

        if (gameObject.activeSelf == true)
            StartCoroutine("BehaviourTree");
        else
            Debug.Log("STOP BT");
    }

    // 기본적인 AI를 BT 클래스에 넣을수도 있지만 나중에 수정할 수도 있으니까 그냥 나둠
    bool Move()
    {
        print("Move");
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
                break;
            }
        }
        return true;
    }

    bool Arrive()
    {
        print("Arrive");

        float dist = Vector3.Distance(targetPos, transform.position);
        if (dist < 1f) // 순찰지점 도착 
        {
            Debug.Log("순찰지점 도착");
            return true;
        }
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 추적 범위안에 들어오면 추적시킴
    bool Track()
    {
        print("Track");

        target = Math.FindClosestTarget("B", transform, gameObject);
        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < trackDist)
        {
            Debug.Log("추적");
            agent.isStopped = false;
            agent.destination = target.transform.position;
            return true;
        }
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 공격 범위안에 들어오면 때림
    bool Attack()
    {
        print("Attack");

        target = Math.FindClosestTarget("B", transform, gameObject);
        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < eventDist)
        {
            Debug.Log("공격");
            agent.isStopped = false;
            agent.destination = target.transform.position;
            return true;
        }
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 방어 범위안에 들어오면 방어
    bool Defense()
    {
        print("Defense");

        target = Math.FindClosestTarget("B", transform, gameObject);
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
        print("Dead");

        if (enemyHP < 0f)
        {
            StopCoroutine("BehaviourTree");
            gameObject.SetActive(false);
            return true;
        }
        return false;
    }
}
