using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SpawnMenu : MonoBehaviour
{

    private static List<Toggle> SpawnCheckToggle;

    private void Awake()
    {
        UIMgr.spawn = UIMgr.Spawn.none;
        print(UIMgr.spawn);
        SpawnCheckToggle = new List<Toggle>();
        for (int i = 0; i < transform.childCount; i++)
        {
            SpawnCheckToggle.Add(transform.GetChild(i).GetComponent<Toggle>());
        }
    }
    public static List<Toggle> GetSpawnCheckToggleList()
    {
        return SpawnCheckToggle;
    }
}
