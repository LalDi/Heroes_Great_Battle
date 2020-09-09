using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeleteStage : MonoBehaviour
{
    public void Stage1()
    {
        SceneMgr.Load(SceneMgr.Scene.Stage1);
    }

    public void Stage2()
    {
        SceneMgr.Load(SceneMgr.Scene.Stage2);
    }

    public void Stage3()
    {
        SceneMgr.Load(SceneMgr.Scene.Stage3);
    }
}
