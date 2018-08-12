using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Appear : MonoBehaviour {

	
	void Start () {
		transform.localScale = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one, Time.deltaTime * 16f);
	}
}
