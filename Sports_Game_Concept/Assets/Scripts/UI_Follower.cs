using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Follower : MonoBehaviour {

    public GameObject target;
    public Vector3 offset;
    private Camera cam;

	// Use this for initialization
	void Start () {
        cam = Camera.main;
	}

    private void LateUpdate()
    {
        transform.position = cam.WorldToScreenPoint(new Vector3(target.transform.position.x + offset.x,
               target.transform.position.y + offset.y, target.transform.position.z + offset.z));
    }
}
