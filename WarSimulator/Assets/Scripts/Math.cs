using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Math : MonoBehaviour
{
    public static Vector3 RandomSphereInPoint(float radius, Transform pos)
    {
        float x = Random.Range(-1.0f, 1.0f);
        float temp = Mathf.Pow(1.0f, 2) - Mathf.Pow(x, 2);
        float z = Mathf.Sqrt(temp);

        return (new Vector3(x, 0.0f, z) * Random.Range(0.0f, radius))
            + new Vector3(pos.position.x, 0.0f, pos.position.z);
    }

    public static GameObject FindClosestTarget(string target, Transform pos, GameObject myobj = null, string team = "asdf")
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(target);

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = pos.position;
        foreach (GameObject go in gos)
        {
            if (go == myobj || go.ToString().Contains(team)) continue;

            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;

            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        return closest;
    }

    public static GameObject FindClosestClass(string Class, GameObject me)
    {
        GameObject[] gos = GameObject.FindObjectsOfType<GameObject>();

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = me.transform.position;

        foreach (GameObject go in gos)
        {
            if (go.name != Class || go == me) continue;
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        return closest;
    }

    public static GameObject FindClosestClass(string Class, GameObject me, string target)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag(target);

        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = me.transform.position;

        foreach (GameObject go in gos)
        {
            if (go.name != Class || go == me) continue;
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        return closest;
    }

    public static float getPercent(float value, float maxvalue)
    {
        return (value * 100) / maxvalue;
    }
}
