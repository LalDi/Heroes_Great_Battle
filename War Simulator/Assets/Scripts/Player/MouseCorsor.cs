using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCorsor : MonoBehaviour
{
    public Texture2D[] TempCursorTexure;
    private static Texture2D[] CursorTexture;
    public bool hotSpotIsCenter = false;
    private Vector2 adjustHotSpot = Vector2.zero;

    private static Vector2 hotspot;

    private static bool isFrameSucceed = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CustomCursor());
    }

    IEnumerator CustomCursor()
    {
        yield return new WaitForEndOfFrame();
        if (hotSpotIsCenter)
        {
            hotspot.x = TempCursorTexure[0].width / 2;
            hotspot.y = TempCursorTexure[0].height / 2;
        }
        else
            adjustHotSpot = hotspot;
        CursorTexture = new Texture2D[3];
        for (int i = 0; i < TempCursorTexure.Length; i++)
        {
            CursorTexture[i] = TempCursorTexure[i];
        }
        if (CursorTexture[0] != null
            && CursorTexture[1] != null
            && CursorTexture[2] != null)
            isFrameSucceed = true;


        SetCursor(0);
    }
    //0 = Default 1 = SpawnCursor 2 = DeleteCursor
    public static void SetCursor(int CursorKey = 0)
    {
        if (isFrameSucceed)
        {
            Cursor.SetCursor(CursorTexture[CursorKey], hotspot, CursorMode.Auto);
        }
    }
}
