using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIMgr : MonoBehaviour
{
    public static bool gameStart = false;  

    GameObject objectSetting;
    GameObject moneyObject;
    GameObject spawnArea;

    public GameObject[] playerObject;
    public List<Vector3> playerTrans = new List<Vector3>();

    public GameObject[] enemyObject;
    public List<Vector3> enemyTrans = new List<Vector3>();

    public enum Spawn
    {
        warrior_m, warrior_w, shield_a, shield_d, archer, gunner, assassin, none
    }
    public static Spawn spawn;


    private void Awake()
    {
        objectSetting = GameObject.Find("Object Setting");
        moneyObject = GameObject.Find("MoneyFrame");
        spawnArea = GameObject.Find("SpawnArea");
        spawn = Spawn.none;
        GetEnemy();
    }

    float time = 0;
    private void Update()
    {
        //time = Time.deltaTime;
        if (!gameStart && time < 2)
        {
            SetEnemy();
            SetPlayer();
            time += Time.deltaTime;
        }
    }

    public void ClickStart()
    {
        if (!gameStart)
        {
            GetPlayer();
            time = 0;
            gameStart = true;
            objectSetting.SetActive(false);
            moneyObject.SetActive(false);
            spawnArea.SetActive(false);
        }
        else
        {
            gameStart = false;
            objectSetting.SetActive(true);
            moneyObject.SetActive(true);
            spawnArea.SetActive(true);
            SetEnemy();
            SetPlayer();
        }
    }

    public void SetEnemy()
    {
        int count = 0;
        foreach (Vector3 pos in enemyTrans)
        {
            if (!enemyObject[count].activeSelf)
                enemyObject[count].SetActive(true);
            enemyObject[count].transform.localRotation = Quaternion.Euler(0, 90, 0);
            enemyObject[count++].transform.localPosition = pos;
        }
    }

    public void SetPlayer()
    {
        int count = 0;
        //Debug.LogWarning("진입");
        foreach (Vector3 pos in playerTrans)
        {
            if (!playerObject[count].activeSelf)
                playerObject[count].SetActive(true);
            playerObject[count].transform.localRotation = Quaternion.Euler(0, -90, 0);
            playerObject[count++].transform.localPosition = pos;
        }
    }

    public void GetEnemy()
    {
        enemyTrans.Clear();
        enemyObject = GameObject.FindGameObjectsWithTag("B");
        foreach (GameObject obj in enemyObject)
        {
            enemyTrans.Add(obj.transform.position);
        }
    }

    public void GetPlayer()
    {
        playerTrans.Clear();
        //GameObject parent = GameObject.Find("Team A");
        //playerObject = parent.GetComponentsInChildren<GameObject>();
        playerObject = GameObject.FindGameObjectsWithTag("A");
        foreach (GameObject obj in playerObject)
        {
            playerTrans.Add(obj.transform.position);
        }
        Debug.LogWarning("성공");
    }
}