using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Archer : BT
{
    #region Behaviour Tree

    Selector root = new Selector();

    Actions dead = new Actions();
    Selector sel = new Selector();

    Sequence seqRader = new Sequence();
    Selector selEvent = new Selector();
    Actions track = new Actions();
    Sequence patrol = new Sequence();

    Actions rader = new Actions();

    Actions attack = new Actions();
    Actions defense = new Actions();

    Actions arrive = new Actions();
    Actions move = new Actions();

    #endregion

    bool startBT;

    GameObject target;
    Vector3 targetPos;
    NavMeshAgent agent;
    Animator animator;
    List<Dictionary<string, object>> data;

    // 몬스터 기본정보 [나중에 문서화 예정]
    [SerializeField] float trackDist;
    [SerializeField] float eventDist;
    [SerializeField] float dangerDist;

    [SerializeField] float enemyHP, enemyMaxHP;
    [SerializeField] float enemyDmg;
    [SerializeField] float enemyAttackSpeed;

    [SerializeField] string teamTag = "A";
    [SerializeField] string targetTag = "B";

    [SerializeField] Transform modelTop; // 추가
    [SerializeField] Transform shootingPoint; // 추가
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targetPos = transform.position;
        data = DataMgr.Read("ClassStats");
        animator = GetComponent<Animator>();
        InitStat();

        enemyMaxHP = enemyHP;
        dangerDist = eventDist / 2;
    }

    void Start()
    {
        Init();
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

    public override void Init()
    {
        // 기본적인 뿌리
        root.AddChild(dead);
        root.AddChild(sel);

        // 적 감지 / 이벤트[공/방] / 추적 / 순찰
        sel.AddChild(seqRader);
        sel.AddChild(selEvent);
        sel.AddChild(track);
        sel.AddChild(patrol);

        seqRader.AddChild(rader);
        seqRader.AddChild(move);

        // 공/방
        selEvent.AddChild(attack);

        // 순찰
        patrol.AddChild(arrive);
        patrol.AddChild(move);

        dead.actionFunc = Dead;
        track.actionFunc = Track;
        rader.actionFunc = Rader;
        attack.actionFunc = Attack;
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

        yield return new WaitForSeconds(1f);
        // Exit
        CheckBT();
    }

    void InitStat()
    {
        trackDist = (int)data[(int)Class.Archer]["Dist2"];
        eventDist = (int)data[(int)Class.Archer]["Dist3"];

        enemyHP = (int)data[(int)Class.Archer]["HP"];
        enemyDmg = (int)data[(int)Class.Archer]["Damage"];
        agent.speed = (int)data[(int)Class.Archer]["MoveSpeed"];
        enemyAttackSpeed = (int)data[(int)Class.Archer]["AttackSpeed"];
    }

    void CheckBT()
    {
        if (gameObject.activeSelf == true)
            StartCoroutine(BehaviourTree());
        else
            Debug.Log("STOP BT");
    }

    bool Rader()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < dangerDist)
        {
            modelTop.rotation = new Quaternion(0f, 0f, 0f, 0f); // 추가
            // 주변에 가장 가까운 팀 찾기
            //print("위험");

            target = Math.FindClosestTarget(teamTag, transform, teamTag, gameObject);
            if (target == null) return true;

            //print("이동");
            agent.isStopped = false;
            agent.destination = target.transform.position;

            return true;
        }
        return false;
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
                agent.isStopped = false;
                agent.destination = targetPos;
                animator.SetBool("isMoving", true);
                animator.SetBool("isAttack", false);
                return true;
            }
        }
    }

    bool Arrive()
    {
        float dist = Vector3.Distance(targetPos, transform.position);
        if (dist < 1f) // 순찰지점 도착 
        {
            return true;
        }
        else
        {
            agent.isStopped = false;
            agent.destination = targetPos;
            return true;
        }
    }

    // 가장 가까운 적을 찾고 그 적이 추적 범위안에 들어오면 추적시킴
    bool Track()
    {
        target = Math.FindClosestTarget(targetTag, transform, "Archer");
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < trackDist)
        {
            agent.isStopped = false;
            agent.destination = target.transform.position;
            animator.SetBool("isMoving", true);
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
            agent.isStopped = true;
            StartCoroutine(Delay(enemyAttackSpeed));
            return true;
        }
        return false;
    }

    // HP 다 떨어지면 사망
    bool Dead()
    {
        if (enemyHP <= 0f)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isDead", true);
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
        animator.SetBool("isAttack", true);
        animator.SetBool("isMoving", false);
        GameObject attack = ObjectMgr.SpawnPool("ArcherAttack", transform.position, transform.rotation);
        attack.GetComponent<EnemyAttack>().ArcherPre(transform.position, target.transform.position, 35f);
        transform.LookAt(target.transform); // 추가
        modelTop.LookAt(shootingPoint); // 추가
        // 임시 공격
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
