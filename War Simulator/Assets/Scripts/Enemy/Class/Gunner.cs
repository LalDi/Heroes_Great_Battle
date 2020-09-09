using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Gunner : BT, AnimInterface
{
    #region Behavior Tree
    Selector root = new Selector();

    Selector behavior = new Selector();      //행동
    Actions dead = new Actions();            //사망

    Selector atkOrdef = new Selector();      //공/방
    Actions track = new Actions();           //추적
    Sequence patrol = new Sequence();        //순찰

    Actions arrive = new Actions();          //
    Actions move = new Actions();            //

    Sequence defense = new Sequence();       //방어
    Selector attack = new Selector();        //공격

    Actions threat = new Actions();          //위협 사격
    Actions escape = new Actions();          //도주

    Actions reload = new Actions();          //장전
    Selector det_shielder = new Selector();  //아군 방패병 탐지
    Selector shooting = new Selector();      //사격

    Actions rush = new Actions();            //돌진

    Actions detection = new Actions();       //사거리 내 아군 탐지
    Actions shoot = new Actions();           //일직선 공격
    #endregion

    bool startBT;

    List<Dictionary<string, object>> data;

    GameObject muzzle; //총구의 정확한 위치를 계산하는것을 성가시므로 그 위치에 빈 오브젝트를 생성하여 좌표를 받음
    GameObject target;
    Vector3 targetPos;
    NavMeshAgent agent;
    Animator animator;

    [Range(0, 6)] int bullet = 6;

    [SerializeField] float attackDist = 20f; // 공격 범위
    [SerializeField] float trackDist = 30f;  // 추적 범위
    [SerializeField] float dangerDist = 5f;  // 도주 범위

    [SerializeField] float enemyHP, enemyMaxHP;
    [SerializeField] float enemyDmg;
    [SerializeField] float enemyAttackSpeed;
    [SerializeField] float reloadDelay;

    [SerializeField] string teamTag = "A";
    [SerializeField] string targetTag = "B";

    bool threating = false;
    bool reloading = false;

    public override void Init()
    {
        root.AddChild(dead);
        root.AddChild(behavior);

        behavior.AddChild(atkOrdef);
        behavior.AddChild(track);
        behavior.AddChild(patrol);

        atkOrdef.AddChild(reload);
        atkOrdef.AddChild(defense);
        atkOrdef.AddChild(attack);

        defense.AddChild(threat);
        defense.AddChild(escape);

        //attack.AddChild(reload);
        attack.AddChild(det_shielder);
        attack.AddChild(shooting);

        det_shielder.AddChild(rush);

        shooting.AddChild(detection);
        shooting.AddChild(shoot);

        patrol.AddChild(arrive);
        patrol.AddChild(move);

        track.actionFunc = Track;
        threat.actionFunc = Threat;
        escape.actionFunc = Escape;
        reload.actionFunc = Reload;
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
        animator = GetComponent<Animator>();
        targetPos = transform.position;
        muzzle = transform.Find("Cylinder").Find("Muzzle").gameObject;

        data = DataMgr.Read("ClassStats");
        InitStat();

        enemyMaxHP = enemyHP;
    }

    void Start()
    {
        Init();
        //StartCoroutine(BehaviourTree());
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

    public override IEnumerator BehaviourTree()
    {
        print("Start BT");
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
        dangerDist = (int)data[(int)Class.Gunner]["Dist1"];
        trackDist = (int)data[(int)Class.Gunner]["Dist2"];
        attackDist = (int)data[(int)Class.Gunner]["Dist3"];

        enemyHP = (int)data[(int)Class.Gunner]["HP"];
        enemyDmg = (int)data[(int)Class.Gunner]["Damage"];
        agent.speed = (int)data[(int)Class.Gunner]["MoveSpeed"];
        enemyAttackSpeed = (int)data[(int)Class.Gunner]["AttackSpeed"];
    }

    void CheckBT()
    {
        print("Check BT");

        if (gameObject.activeSelf == true)
            StartCoroutine(BehaviourTree());
        else
            Debug.Log("STOP BT");
    }

    // 가장 가까운 적을 찾고 그 적이 추적 범위안에 들어오면 추적시킴
    bool Track()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < trackDist && dist > attackDist)
        {
            agent.isStopped = false;
            agent.destination = target.transform.position;

            animator.SetBool("isMoving", true);

            transform.LookAt(target.transform);
            print("추적(거너)");
            return true;
        }
        return false;
    }

    // 위험범위 안에 들어온 적에게 정조준이 되지 않은 위협사격을 가함
    bool Threat()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist <= dangerDist && !threating)
        {
            agent.isStopped = true;
            animator.SetBool("isAttack", true);
            animator.SetBool("isMoving", false);
            StartCoroutine(ThreatShoot(target, 0));
            return true;
        }
        return false;
    }

    IEnumerator ThreatShoot(GameObject target, int count)
    {
        threating = true;
        if (bullet > 0 && count < 3)
        {
            Vector3 shot = (target.transform.position - transform.position) + new Vector3(Random.Range(-5f, 5f), 0, 0);
            //print("위협사격(거너) , shot : " + shot + "bullet : " + bullet);
            transform.LookAt(target.transform);
            ObjectMgr.SpawnPool("GunnerAttack", muzzle.transform.position, Quaternion.Euler(shot));
            bullet--;
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(ThreatShoot(target, ++count));
        }
        else
            threating = false;
        yield return null;
    }

    // 위험범위 안에 들어온 적을 피해 도망감
    bool Escape()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist <= dangerDist)
        {
            agent.isStopped = false;
            animator.SetBool("isMoving", true);
            while (true)
            {
                targetPos = Math.RandomSphereInPoint(dangerDist + 2, transform);
                float dist_ = Vector3.Distance(targetPos, target.transform.position);
                if (Physics.Raycast(targetPos, Vector3.down, 5f) && dist_ > dangerDist)
                {
                    agent.destination = targetPos;
                    //print("도주(거너)");
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
        if (bullet <= 0 && !reloading)
        {
            StartCoroutine(RelaodDelay());
            //print("재장전(거너)");
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
        yield return null;
    }

    // 자신의 주변에 돌격형 방패병이 있을 때, 돌진함
    bool Rush()
    {
        target = Math.FindClosestClass("ShieldA", gameObject, targetTag);
        if (target == null) return false;
        float dist = Vector3.Distance(transform.position, target.transform.position);
        if (dist < dangerDist * 2)
        {
            agent.isStopped = false;
            agent.destination = target.transform.position;
            animator.SetBool("isMoving", true);
            return true;
        }
        return false;
    }

    // 자신의 공격 사거리 내에 아군이 있는지 확인하여, 아군이 있다면 공격이 아군에게 맞지 않도록 이동
    bool Detection()
    {
        RaycastHit hit;
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (Physics.Raycast(muzzle.transform.position, target.transform.position, out hit, attackDist))
        {
            //Debug.DrawRay(muzzle.transform.position, target.transform.position, Color.red, 0.5f);
            Debug.Log("아군!! 피해욧!!");
            if (dist < attackDist &&
                !hit.transform.CompareTag(targetTag))
            {
                agent.isStopped = false;
                for (int i = 0; i < 5; i++)
                {
                    targetPos = Math.RandomSphereInPoint(5f, transform); //0.5는 임의로 설정한 수치, 너무 좁다 싶으면 수정 가능
                    if (Physics.Raycast(targetPos, target.transform.position, out hit, attackDist) &&
                        hit.transform.CompareTag(targetTag))
                    {
                        agent.destination = targetPos;
                        animator.SetBool("isMoving", true);
                        break;
                    }
                }
                return true;
            }
        }
        return false;
    }

    // 가장 가까운 적을 찾고, 그 적이 공격 범위안에 들어오면 일자로 공격
    bool Shoot()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);
        Vector3 vec = target.transform.position - transform.position;
        vec.Normalize();
        Quaternion rot = Quaternion.LookRotation(vec);

        if (dist < attackDist && bullet > 0)
        {
            agent.isStopped = true;
            animator.SetBool("isAttack", true);
            animator.SetBool("isMoving", false);
            transform.rotation = rot;
            transform.LookAt(target.transform);
            ObjectMgr.SpawnPool("GunnerAttack", muzzle.transform.position, rot);
            Debug.Log("Shoot (Target : " + target.name + ")");
            bullet--;
            return true;
        }
        return false;
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
            animator.SetBool("isMoving", false);
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

            //Debug.Log("새로운 좌표 찍음");
            //Debug.Log(targetPos);

            if (Physics.Raycast(targetPos, Vector3.down, 5f))
            {
                // 밑에 바닥임
                print("순찰(거너)");
                agent.isStopped = false;
                agent.destination = targetPos;
                animator.SetBool("isMoving", true);
                break;
            }
        }
        return true;
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

    private void OnTriggerEnter(Collider other)
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

    public void playingAnimationEvent()
    {

    }

    public void transitionAnimationEvent()
    {

    }

    public void endingAnimationEvent()
    {
        if (animator.GetBool("isMoving"))
        {
            animator.SetBool("isMoving", false);
        }
        if (animator.GetBool("isAttack"))
        {
            animator.SetBool("isAttack", false);
        }
        if (animator.GetBool("isDead"))
        {
            animator.SetBool("isDead", false);
            StopCoroutine("BehaviourTree");
            gameObject.SetActive(false);
        }
    }
}