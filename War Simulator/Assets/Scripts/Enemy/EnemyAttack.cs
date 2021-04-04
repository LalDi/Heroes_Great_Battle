using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public enum Class
    {
        Warrior, Archer, Shield, Gunner,
    };
    public Class attackType;

    Rigidbody rigd;

    Vector3 StartPos;
    Vector3 EndPos;

    #region Archer

    float vx; // x축 방향의 속도
    float vy; // y축 방향의 속도
    float vz; // z축 방향의 속도
    float g;  // y축 방향의 중력 가속도

    float dat; // 도착점 도달 시간
    float mh;  // 최고점 - 시작점(높이)
    float dh;  // 도착점 높이

    float t; // 진행시간
    float maxY; // 최고점 높이

    #endregion

    #region Gunner
    float speed = 2f;

    #endregion

    public GameObject Attacker;

    void Start()
    {
        rigd = GetComponent<Rigidbody>();
        //if (attackType == Class.Gunner)
        //{
        //    rigd.velocity = transform.localRotation * Vector3.forward * speed;
        //}
    }

    private void Update()
    {
        if (attackType == Class.Gunner)
            transform.position += transform.localRotation * Vector3.forward;
    }

    public void ArcherPre(Vector3 _StartPos, Vector3 _EndPos, float maxY)
    {
        StartPos = _StartPos;
        EndPos = _EndPos;

        dh = EndPos.y - StartPos.y;
        mh = maxY - StartPos.y;

        g = 9.8f;

        vy = Mathf.Sqrt(2 * g * mh);

        float a = g;
        float b = -2 * vy;
        float c = 2 * dh;

        dat = (-b + Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
        vx = -(StartPos.x - EndPos.x) / dat;
        vz = -(StartPos.z - EndPos.z) / dat;

        t = 0.0f;

        StartCoroutine("Shoot");
    }

    IEnumerator Shoot()
    {
        while (true)
        {
            yield return null;

            t += Time.deltaTime;

            var tx = StartPos.x + vx * t;
            var ty = StartPos.y + vy * t - 0.5f * g * t * t;
            var tz = StartPos.z + vz * t;

            var tpos = new Vector3(tx, ty, tz);

            transform.LookAt(tpos);
            transform.position = tpos;

            if (t >= this.dat)
                break;
        }
        //Debug.Log("도착");
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (attackType == Class.Warrior)
        {
            StartCoroutine(Delay(3.0f));
        }
        //if (attackType == Class.Gunner)
        //{
        //    rigd.velocity = transform.localRotation * Vector3.forward * speed;
        //}
    }

    void OnDisable()
    {
        if (attackType == Class.Gunner)
        {
            //rigd.velocity = Vector3.zero;
            transform.Rotate(Vector3.zero);
        }
    }

    IEnumerator Delay(float delay)
    {
        float curTime = 0.0f;

        while (curTime <= delay)
        {
            curTime += Time.deltaTime;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.activeSelf && !other.CompareTag("Attack"))
            if (attackType == Class.Gunner && Attacker != other.gameObject)
                gameObject.SetActive(false);
    }
}
