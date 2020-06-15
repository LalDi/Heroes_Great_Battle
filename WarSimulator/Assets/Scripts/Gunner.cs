using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Gunner : BT
{
    #region Behavior Tree
    Selector root = new Selector();

    Selector behavior = new Selector();      //행동
    Action dead = new Action();              //사망

    Selector atkOrdef = new Selector();      //공/방
    Action track = new Action();             //추적
    Sequence patrol = new Sequence();        //순찰

    Action arrive = new Action();            //
    Action move = new Action();              //

    Sequence defense = new Sequence();       //방어
    Selector attack = new Selector();        //공격

    Action threat = new Action();            //위협 사격
    Action escape = new Action();            //도주

    Action reload = new Action();            //장전
    Selector det_shielder = new Selector();  //아군 방패병 탐지
    Selector shooting = new Selector();      //사격

    Action sniping = new Action();           //저격
    Action rush = new Action();              //돌진

    Action detection = new Action();         //사거리 내 아군 탐지
    Action shoot = new Action();             //일직선 공격
    #endregion

    GameObject muzzle; //총구의 정확한 위치를 계산하는것을 성가시므로 그 위치에 빈 오브젝트를 생성하여 좌표를 받음
    GameObject target;
    NavMeshAgent agent;

    Vector3 targetPos;

    [Range(0,6)] int bullet;
    [SerializeField] float attackDelay = 0;
    [SerializeField] float reloadDelay = 0;

    [SerializeField] float attackDist = 20f; // 공격 범위
    [SerializeField] float trackDist = 30f;  // 추적 범위
    [SerializeField] float riskDist = 5f;    // 도주 범위

    float HP, MaxHP = 10f;
    bool reloading = false;

    public override void Init()
    {
        root.AddChild(behavior);
        root.AddChild(dead);

        behavior.AddChild(atkOrdef);
        behavior.AddChild(track);
        behavior.AddChild(patrol);

        atkOrdef.AddChild(defense);
        atkOrdef.AddChild(attack);

        defense.AddChild(threat);
        defense.AddChild(escape);

        attack.AddChild(reload);
        attack.AddChild(det_shielder);
        attack.AddChild(shooting);

        det_shielder.AddChild(sniping);
        det_shielder.AddChild(rush);

        shooting.AddChild(detection);
        shooting.AddChild(shoot);

        patrol.AddChild(arrive);
        patrol.AddChild(move);

        track.actionFunc = Track;
        threat.actionFunc = Threat;
        escape.actionFunc = Escape;
        reload.actionFunc = Reload;
        sniping.actionFunc = Sniping;
        rush.actionFunc = Rush;
        detection.actionFunc = Detection;
        shoot.actionFunc = Shoot;
        arrive.actionFunc = Arrive;
        move.actionFunc = Move;
        dead.actionFunc = Dead;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        targetPos = transform.position;
        muzzle = transform.Find("Muzzle").gameObject;

        HP = MaxHP;
        bullet = 6;
    }

    void Start()
    {
        Init();
        StartCoroutine(BehaviourTree());
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
        
    // 가장 가까운 적을 찾고 그 적이 추적 범위안에 들어오면 추적시킴
    bool Track()
    {
        target = Math.FindClosestTarget("B", transform, gameObject);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < trackDist)
        {
            agent.isStopped = false;
            agent.destination = target.transform.position;

            transform.LookAt(target.transform);
            return true;
        }
        return false;
    }

    // 위험범위 안에 들어온 적에게 정조준이 되지 않은 위협사격을 가함
    bool Threat()
    {
        target = Math.FindClosestTarget("B", transform, gameObject);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);
        Vector3 shot = target.transform.position - transform.position;

        if (dist < riskDist)
        {
            agent.isStopped = true;
            for (int i = 0; i < 3; i++)
            {
                if (bullet == 0) break;
                ObjectMgr.SpawnPool("bullet", transform.position, Quaternion.Euler(shot + new Vector3(Random.Range(-2f, 2f), 0, 0)));
                bullet--;
                
            }
            return true;
        }
        return false;
    }

    // 위험범위 안에 들어온 적을 피해 도망감
    bool Escape()
    {
        target = Math.FindClosestTarget("B", transform, gameObject);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < riskDist)
        {
            agent.isStopped = false;
            while (true)
            {
                targetPos = Math.RandomSphereInPoint(riskDist, transform);
                float dist_ = Vector3.Distance(targetPos, target.transform.position);
                if (Physics.Raycast(targetPos, Vector3.down, 5f) && dist_ > riskDist)
                {
                    agent.destination = targetPos;
                    break;
                }
            }
            return true;
        }
        return false;
    }

    // 탄약이 다 떨어졌을때 6발로 재장전
    bool Reload()
    {
        if (bullet == 0 && !reloading)
        {
            StartCoroutine(RelaodDelay());
            return true;
        }
        else
            return false;
    }

    IEnumerator RelaodDelay()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadDelay);
        bullet = 6;
        reloading = false;
    }

    // 자신의 주변에 수비형 방패병이 있을 때, 적을 저격함
    bool Sniping()
    {
        target = Math.FindClosestClass("Shielder", gameObject, "A");
        if (target == null)
            return false;
        return true;
    }

    // 자신의 주변에 돌격형 방패병이 있을 때, 돌진함
    bool Rush()
    {
        return false;
    }

    // 자신의 공격 사거리 내에 아군이 있는지 확인하여, 아군이 있다면 공격이 아군에게 맞지 않도록 이동
    bool Detection()
    {
        RaycastHit hit;
        target = Math.FindClosestTarget("B", transform, gameObject);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < attackDist &&                                                                  
            Physics.Raycast(transform.position, target.transform.position, out hit, attackDist) &&
            !hit.transform.CompareTag("B"))                                                       
        {
            agent.isStopped = false;
            while (true)
            {
                targetPos = Math.RandomSphereInPoint(0.5f, transform); //0.5는 임의로 설정한 수치, 너무 좁다 싶으면 수정 가능
                if (Physics.Raycast(targetPos, target.transform.position, out hit, attackDist) &&
                    hit.transform.CompareTag("B"))
                {
                    agent.destination = targetPos;
                    break;
                }
            }
            return true;
        }
        return false;
    }

    // 가장 가까운 적을 찾고, 그 적이 공격 범위안에 들어오면 일자로 공격
    bool Shoot()
    {
        target = Math.FindClosestTarget("B", transform, gameObject);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < attackDist)
        {
            transform.LookAt(target.transform);
            agent.isStopped = true;
            ObjectMgr.SpawnPool("bullet", muzzle.transform.position, Quaternion.LookRotation(target.transform.position));
            bullet--;
            //StartCoroutine(AttackDelay());
            print("attack");
            return true;
        }
        return false;
    }

    IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(attackDelay);
    }

    // Move에서 찍은 좌표에 도착하면 true를 반환, 다시 Move로 들어가게 함
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

    // 새로운 좌표를 랜덤으로 찍어서 그곳으로 이동
    bool Move()
    {
        print("Move");
        agent.isStopped = true;
        while (true)
        {
            // 새로운 좌표를 찍어줌
            targetPos = (Math.RandomSphereInPoint(15, transform));

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

    // HP 다 떨어지면 사망
    bool Dead()
    {
        print("Dead");

        if (HP < 0f)
        {
            StopCoroutine("BehaviourTree");
            gameObject.SetActive(false);
            return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Attack"))
        {
            HP -= 2f;
        }
    }
}