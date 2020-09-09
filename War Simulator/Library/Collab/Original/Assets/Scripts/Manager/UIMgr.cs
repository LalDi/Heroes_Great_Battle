using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIMgr : MonoBehaviour
{
    public static bool gameStart = false;  

    GameObject objectSetting;

    public List<GameObject> playerObject = new List<GameObject>();
    public List<Transform> playerTrans = new List<Transform>();
    public GameObject[] enemyObject;
    public List<Vector3> enemyTrans = new List<Vector3>();

    new Camera camera = Camera.main;
    RaycastHit hit;

    public enum Spawn
    {
        warrior_m, warrior_w, shield_a, shield_d, archer, gunner, ninja, none
    }
    public static Spawn spawn;

    public void SetEnemyInit()
    {
        enemyTrans.Clear();
        enemyObject = GameObject.FindGameObjectsWithTag("B");
        foreach (GameObject obj in enemyObject)
        {
            enemyTrans.Add(obj.transform.position);
        }
        Debug.LogWarning("성공");
    }

    private void Awake()
    {
        objectSetting = GameObject.Find("Object Setting");
        spawn = Spawn.none;
        SetEnemyInit();
    }

    public void ClickStart()
    {
        float time = Time.timeScale;
        if (!gameStart)
        {
            gameStart = true;
            objectSetting.SetActive(false);
        }
        else
        {
            gameStart = false;
            objectSetting.SetActive(true);
            SetEnemy();
        }
    }

    public void SetEnemy()
    {
        int count = 0;
        Debug.LogWarning("진입");
        foreach (Vector3 pos in enemyTrans)
        {
            Debug.LogWarning(enemyObject[count].name + pos);
            enemyObject[count++].transform.localPosition = pos;
        }
    }

    public void SpawnObject()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        GameObject newObject;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == "BlueArea")
            {
                switch (spawn)
                {
                    case Spawn.warrior_m:
                        newObject = ObjectMgr.SpawnPool("Warrior", Input.mousePosition, Quaternion.Euler(0, 0, 0));
                        Warrior warrior = newObject.GetComponent<Warrior>();
                        newObject.name = "A_Warrior";
                        warrior.gender = Warrior.Gender.Man;
                        break;
                    case Spawn.warrior_w:
                        newObject = ObjectMgr.SpawnPool("Warrior", Input.mousePosition, Quaternion.Euler(0, 0, 0));
                        Warrior warrior_ = newObject.GetComponent<Warrior>();
                        newObject.name = "A_Warrior";
                        warrior_.gender = Warrior.Gender.Woman;
                        break;
                    case Spawn.shield_a:
                        newObject = ObjectMgr.SpawnPool("ShieldA", Input.mousePosition, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_ShieldA";
                        break;
                    case Spawn.shield_d:
                        newObject = ObjectMgr.SpawnPool("ShieldD", Input.mousePosition, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_ShieldD";
                        break;
                    case Spawn.archer:
                        newObject = ObjectMgr.SpawnPool("Archer", Input.mousePosition, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_Archer";
                        break;
                    case Spawn.gunner:
                        newObject = ObjectMgr.SpawnPool("Gunner", Input.mousePosition, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_Gunner";
                        break;
                    case Spawn.ninja:
                        break;
                    case Spawn.none:
                        break;
                }
            }
        }
    }

    public void DeleteObject()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("A"))
            {
                Destroy(hit.transform.gameObject);
            }
        }
    }
}