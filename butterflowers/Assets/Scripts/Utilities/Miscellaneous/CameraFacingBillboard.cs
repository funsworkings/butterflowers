// http://wiki.unity3d.com/index.php?title=CameraFacingBillboard

//cameraFacingBillboard.cs v03
//by Neil Carter (NCarter)
//modified by Juan Castaneda (juanelo)
//modified by Brendan Matkin
//
//added in-between GRP object to perform rotations on
//added auto-find main camera
//added un-initialized state, where script will do nothing
//BM - added color tinting
using UnityEngine;
using System.Collections;


public class CameraFacingBillboard : MonoBehaviour
{
    Camera mainCam;
	
	void Awake(){
        mainCam = Camera.main;
	}

	void Update(){
        transform.LookAt(mainCam.transform.position, Vector3.up);
	}

}
