using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverScale : MonoBehaviour {

	bool m_HasShake = false;
	void Start () {
		transform.localScale = Vector3.one * 8f;
		m_HasShake = false;
	}
	
	void Update () {
		transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one, Time.deltaTime * 12f);

		if(!m_HasShake && transform.localScale.sqrMagnitude <= 3f + 10e3f) {
			Game.instance.ShakeCamera();
			m_HasShake = true;
		}
	}
}
