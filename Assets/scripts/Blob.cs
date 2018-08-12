using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob : MonoBehaviour {

	
	void Start () {
		transform.localScale = Vector3.one * 0.5f;
	}
	
	void Update () {
		transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one, Time.deltaTime * 2f);
	}
}
