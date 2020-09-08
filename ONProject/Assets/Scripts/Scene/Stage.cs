using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public GameObject option;

    public void ClickOption()
    {
        if (option.activeSelf)
            option.SetActive(false);
        else
            option.SetActive(true);
    }

    public void ClickTitle()
    {
        UIMgr.gameStart = false;
        SceneMgr.Load(SceneMgr.Scene.Title);
    }

    public void ClickExit()
    {
        Application.Quit();
    }
}
