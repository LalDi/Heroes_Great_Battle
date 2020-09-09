using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    [SerializeField] bool isableScreenShot = false;
    public GameObject Canvas;
    // Start is called before the first frame update
    void Start()
    {
        if (isableScreenShot)
            StartCoroutine(Capture());
    }
    IEnumerator Capture()
    {
        while(isableScreenShot)
        {
            if (Input.GetKeyDown(KeyCode.SysReq) || Input.GetKeyDown(KeyCode.Tab))
            {
                Canvas.SetActive(false);
                yield return new WaitForSeconds(0.1f);
                ScreenCapture.CaptureScreenshot("Screenshot.png");
                yield return new WaitForSeconds(0.5f);
                Canvas.SetActive(true);
            }
            yield return null;
        }
        
    }
}
