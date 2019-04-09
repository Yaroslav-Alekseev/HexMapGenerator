using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexScaler : MonoBehaviour {

    public void ResetScale()
    {
        transform.localScale = Vector3.one;
        transform.position = Vector3.zero;
    }

    public void ScaleHexesToScreenSize()
    {
        //Vector3 newPos = new Vector3();
        //newPos.x = -Screen.width / 2f;
        //newPos.y = -Screen.height / 2f;
        //transform.position = newPos;

        float scaleFactor = Screen.height / 1080f;
        transform.localScale = Vector3.one * scaleFactor;
    }

}
