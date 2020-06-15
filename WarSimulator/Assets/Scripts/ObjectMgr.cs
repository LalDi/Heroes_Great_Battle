using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMgr : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        [Tooltip("오브젝트 이름")] public string tag;
        [Tooltip("오브젝트 개수")] public int size;
        [Tooltip("생성 오브젝트")] public GameObject prefab;

        public GameObject parentObj;
    };
    public List<Pool> pools;
    static Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.name = pool.tag + " " + i;

                if (pool.parentObj != null) obj.transform.parent = pool.parentObj.transform;

                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public static GameObject SpawnPool(string tag, Vector3 pos, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(tag))
            return null;

        foreach (GameObject obj in poolDictionary[tag])
        {
            if (!obj.activeSelf)
            {
                //print("Is not full");
                GameObject ObjectSpawn = poolDictionary[tag].Dequeue();

                ObjectSpawn.SetActive(true);
                ObjectSpawn.transform.position = pos;
                ObjectSpawn.transform.rotation = rot;

                poolDictionary[tag].Enqueue(ObjectSpawn);

                return ObjectSpawn;
            }
        }
        //print("Is full");
        GameObject newObject = Instantiate(poolDictionary[tag].Peek());

        newObject.transform.parent = poolDictionary[tag].Peek().transform.parent;

        newObject.SetActive(true);

        newObject.transform.position = pos;
        newObject.transform.rotation = rot;

        newObject.name = tag.ToString() + " " + poolDictionary[tag].Count;

        poolDictionary[tag].Enqueue(newObject);

        return newObject;
    }
}