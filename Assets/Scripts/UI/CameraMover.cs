using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {

    public Camera MainCamera;
    public float DefaultCameraSize = 540;
    public float ZoomSpeed = 1.1f;
	
	private void Update ()
    {
        MoveCamera();
        ZoomCamera();
    }


    public void ResetCamera()
    {
        var pos = transform.position;
        pos.x = 0;
        pos.y = 0;
        transform.position = pos;

        MainCamera.orthographicSize = DefaultCameraSize;
    }


    private void MoveCamera()
    {
        var pos = -transform.position;
        pos.z = MainCamera.transform.position.z;
        MainCamera.transform.position = pos;
    }

    private void ZoomCamera()
    {
        bool wheelUp = Input.GetAxis("Mouse ScrollWheel") > 0;
        bool wheelDown = Input.GetAxis("Mouse ScrollWheel") < 0;

        if (wheelUp)
            MainCamera.orthographicSize /= ZoomSpeed;
        else if (wheelDown)
            MainCamera.orthographicSize *= ZoomSpeed;
    }       

}
