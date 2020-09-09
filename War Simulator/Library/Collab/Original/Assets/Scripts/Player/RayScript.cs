using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.AI;

public class RayScript : UIMgr {

    public Camera cam = null;
    RaycastHit hit;
    public GameObject InstObj = null;
    bool spawncheck = true;

    bool PreventInput = false;
    // Update is called once per frame
    void Update () 
    {
        if (!PreventInput)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!(EventSystem.current.IsPointerOverGameObject()))
                {
                    MouseCorsor.SetCursor(1);
                    if (Money.CurMoney <= 0) spawncheck = false;
                    else spawncheck = true;

                    if (spawncheck)
                        SpawnObject();
                }
            }
            else if (Input.GetMouseButton(1))
            {
                if (!(EventSystem.current.IsPointerOverGameObject()))
                {
                    MouseCorsor.SetCursor(2);
                    DeleteObject();
                }
            }
            else MouseCorsor.SetCursor(0);
        }
    }
    public void SpawnObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        GameObject newObject;
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("BlueArea"))
            {
                switch (spawn)
                {
                    case Spawn.warrior_m:
                        newObject = ObjectMgr.SpawnPool("WarriorM", hit.point, Quaternion.Euler(0, 0, 0));
                        Warrior warrior = newObject.GetComponent<Warrior>();
                        newObject.name = "A_Warrior";
                        warrior.gender = Warrior.Gender.Man;
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                    case Spawn.warrior_w:
                        newObject = ObjectMgr.SpawnPool("WarriorW", hit.point, Quaternion.Euler(0, 0, 0));
                        Warrior warrior_ = newObject.GetComponent<Warrior>();
                        newObject.name = "A_Warrior";
                        warrior_.gender = Warrior.Gender.Woman;
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                    case Spawn.shield_a:
                        newObject = ObjectMgr.SpawnPool("ShieldA", hit.point, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_ShieldA";
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                    case Spawn.shield_d:
                        newObject = ObjectMgr.SpawnPool("ShieldD", hit.point, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_ShieldD";
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                    case Spawn.archer:
                        newObject = ObjectMgr.SpawnPool("Archer", hit.point, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_Archer";
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                    case Spawn.gunner:
                        newObject = ObjectMgr.SpawnPool("Gunner", hit.point, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_Gunner";
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                    case Spawn.assassin:
                        newObject = ObjectMgr.SpawnPool("Assassin", hit.point, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_Assassin";
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                    case Spawn.none:
                        break;
                }
                if (spawn != Spawn.none)
                    Money.CurMoney -= 50;
                print("CurMoney : " + Money.CurMoney);
            }
            else if (hit.transform.CompareTag("RedArea"))
            {
                switch (spawn)
                {
                    case Spawn.assassin:
                        newObject = ObjectMgr.SpawnPool("Assassin", hit.point, Quaternion.Euler(0, 0, 0));
                        newObject.name = "A_Assassin";
                        newObject.tag = "A";
                        newObject.GetComponent<NavMeshAgent>().enabled = true;
                        break;
                }
                if (spawn == Spawn.assassin)
                    Money.CurMoney -= 50;
                print("CurMoney : " + Money.CurMoney);
            }
            print(hit.transform.name);
        }
    }

    public void DeleteObject()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("A"))
            {
                //Destroy(hit.transform.gameObject);
                hit.transform.gameObject.SetActive(false);
                Money.CurMoney += 50;
                print("CurMoney : " + Money.CurMoney);
            }
        }
    }
}
