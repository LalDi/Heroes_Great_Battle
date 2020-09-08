using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    public GameObject option;
    public GameObject developer;
    public Toggle toggle;

    public void GameStart()
    {
        SceneMgr.Load(SceneMgr.Scene.SeleteStage);
    }

    public void Option()
    {
        option.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Developer()
    {
        bool isOn = toggle.isOn;
        developer.SetActive(isOn);
    }

    //Option
    public void OffOption()
    {
        option.SetActive(false);
    }
}
