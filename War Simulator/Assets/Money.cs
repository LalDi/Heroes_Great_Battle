using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Money : MonoBehaviour
{
    public static int CurMoney = 1500;
    public Text MoneyTxt = null;
    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(MoneyCheck());
    }
    private void Start()
    {
        CurMoney = 1500;
    }
    IEnumerator MoneyCheck()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            MoneyTxt.text = CurMoney.ToString();

            yield return null;

        }
    }
    //private void Update()
    //{
    //    MoneyTxt.text = CurMoney.ToString();
    //}
}
