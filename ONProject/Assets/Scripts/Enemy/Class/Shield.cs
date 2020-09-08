using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Shield : BT, AnimInterface
{
    #region Behaviour Tree

    Selector root = new Selector();

    Actions dead = new Actions();
    Selector sel = new Selector();

    Selector selEvent = new Selector();

    Actions attack = new Actions();

    // 지역 방어
    Actions zonedefense = new Actions();
    // 대인 방어
    Actions mantoman = new Actions();

    #endregion

    bool startBT;

    public enum ShieldKind
    {
        Attack, Defense
    }
    public ShieldKind shieldKind;

    GameObject target;
    GameObject shield;

    Animator animtor;
    NavMeshAgent agent;

    List<Dictionary<string, object>> data;

    // 몬스터 기본정보 [나중에 문서화 예정]
    [SerializeField] float eventDist;
    [SerializeField] float zoneDist;
    [SerializeField] float manDist;

    [SerializeField] float enemyHP, enemyMaxHP;
    [SerializeField] float enemyDmg;
    [SerializeField] float enemyAttackSpeed;

    [SerializeField] string teamTag = "A";
    [SerializeField] string targetTag = "B";

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animtor = GetComponent<Animator>();
        shield = transform.Find("Shield").gameObject;
        data = DataMgr.Read("ClassStats");

        InitStat();

        enemyMaxHP = enemyHP;
    }

    void Start()
    {
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
            if (shieldKind == ShieldKind.Defense)
            {
                animtor.SetBool("isBlock", false);
                animtor.SetBool("isBlocking", false);
            }
            animtor.SetBool("isMoving", false);
            animtor.SetBool("isDead", false);
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

        // 이벤트[공/방] / 대인방어 / 지역방어
        //sel.AddChild(selEvent);
        sel.AddChild(mantoman);
        sel.AddChild(zonedefense);

        // 공/방
        selEvent.AddChild(attack);

        dead.actionFunc = Dead;
        attack.actionFunc = Attack;
        mantoman.actionFunc = ManToMan;
        zonedefense.actionFunc = ZoneDefense;
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

    void InitStat()
    {
        if (shieldKind == ShieldKind.Attack)
        {
            eventDist = (int)data[(int)Class.Shield_A]["Dist1"];
            zoneDist = (int)data[(int)Class.Shield_A]["Dist2"];
            manDist = (int)data[(int)Class.Shield_A]["Dist3"];

            enemyHP = (int)data[(int)Class.Shield_A]["HP"];
            enemyDmg = (int)data[(int)Class.Shield_A]["Damage"];
            agent.speed = (int)data[(int)Class.Shield_A]["MoveSpeed"];
            enemyAttackSpeed = (int)data[(int)Class.Shield_A]["AttackSpeed"];

        }
        else
        {
            eventDist = (int)data[(int)Class.Shield_D]["Dist1"];
            zoneDist = (int)data[(int)Class.Shield_D]["Dist2"];
            manDist = (int)data[(int)Class.Shield_D]["Dist3"];

            enemyHP = (int)data[(int)Class.Shield_D]["HP"];
            enemyDmg = (int)data[(int)Class.Shield_D]["Damage"];
            agent.speed = (int)data[(int)Class.Shield_D]["MoveSpeed"];
            enemyAttackSpeed = (int)data[(int)Class.Shield_D]["AttackSpeed"];
        }
    }

    void CheckBT()
    {
        if (gameObject.activeSelf == true)
            StartCoroutine(BehaviourTree());
        else
            Debug.Log("STOP BT");
    }

    // 가장 가까운 적을 찾고 그 적이 공격 범위안에 들어오면 때림
    bool Attack()
    {
        target = Math.FindClosestTarget(targetTag, transform);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        if (dist < eventDist)
        {
            agent.isStopped = false;
            agent.destination = target.transform.position;

            StartCoroutine(Delay(enemyAttackSpeed));
            return true;
        }
        return false;
    }

    // 가장 가까운 적을 찾고 그 적이 방어 범위안에 들어오면 방어
    bool ManToMan()
    {
        target = Math.FindClosestTarget(targetTag, transform, "Shield");
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        // 대인방어 범위안에 들어오면 그 타겟을 쫓아가서 비빔
        if (dist < manDist)
        {
            //Debug.Log("대인 방어");
            if (shieldKind == ShieldKind.Attack)
                animtor.SetBool("isMoving", true);
            else
            { 
                animtor.SetBool("isBlock", true);
                animtor.SetBool("isBlocking", true);
                StartCoroutine(AnimationDelay());
            }

            agent.isStopped = false;
            agent.destination = target.transform.position;
            return true;
        }
        return false;
    }

    bool ZoneDefense()
    {
        target = Math.FindClosestTarget(teamTag, transform, teamTag.ToString() + "_Shield", gameObject);
        if (target == null) return false;

        float dist = Vector3.Distance(target.transform.position, transform.position);

        // 대인방어 범위안에 들어오면 그 타겟을 쫓아가서 비빔
        if (dist < zoneDist)
        {
            if (shieldKind == ShieldKind.Attack)
                animtor.SetBool("isMoving", true);
            else
            {
                animtor.SetBool("isBlock", true);
                animtor.SetBool("isBlocking", true);
                StartCoroutine(AnimationDelay());
            }

            agent.isStopped = false;
            agent.destination = target.transform.position;
            return true;
        }
        return false;
    }

    IEnumerator AnimationDelay()
    {
        yield return new WaitForSeconds(0.1f);
        animtor.SetBool("isBlock", false);
    }

    // HP 다 떨어지면 사망
    bool Dead()
    {
        if (enemyHP <= 0f)
        {
            animtor.SetBool("   ", true);
            return true;
        }
        else if (Math.getPercent(enemyHP, enemyMaxHP) < 15f)
        {
            shield.SetActive(false);
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
        if (target.activeSelf)
        {
            //animtor.SetBool("isAttack", true);
            GameObject attack = ObjectMgr.SpawnPool("Attack", transform.position, transform.rotation);

            Vector3 dir = target.transform.position - transform.position;
            dir.Normalize();

            attack.GetComponent<Rigidbody>().velocity += dir * 5.0f;
        }
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

    public void playingAnimationEvent()
    {
        
    }

    public void transitionAnimationEvent()
    {
        
    }

    public void endingAnimationEvent()
    {
        if (animtor.GetBool("isMoving"))
        {
            animtor.SetBool("isMoving", false);
        }
        if (animtor.GetBool("isAttack"))
        {
            animtor.SetBool("isAttack", false);
        }
        if (animtor.GetBool("isDead"))
        {
            animtor.SetBool("isDead", false);
            StopCoroutine("BehaviourTree");
            gameObject.SetActive(false);
        }
    }
}
