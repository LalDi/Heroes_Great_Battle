using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Load : MonoBehaviour
{
    public Image progress;

    private void Start()
    {
        SceneMgr.LoaderCallback();
    }

    private void FixedUpdate()
    {
        progress.fillAmount = SceneMgr.GetLoadingProgress();
        Debug.Log(progress.fillAmount);
    }
}
