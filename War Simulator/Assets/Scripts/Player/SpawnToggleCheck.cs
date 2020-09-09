using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SpawnToggleCheck : UIMgr
{
    public Spawn spawn_child = Spawn.none;

    public void CheckToggle()
    {
        
        bool isOn = transform.GetComponent<Toggle>().isOn;

        if (isOn)
        {
            spawn = spawn_child;
            for (int j = 0; j < SpawnMenu.GetSpawnCheckToggleList().Count; j++)
            {
                if (!(SpawnMenu.GetSpawnCheckToggleList()[j] == transform.GetComponent<Toggle>()))
                    SpawnMenu.GetSpawnCheckToggleList()[j].isOn = false;

            }

        }

    }
}
