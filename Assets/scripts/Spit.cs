using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spit : MonoBehaviour {

	public Vector3 startPosition;
	public Vector3 target;
	public delegate void SpitDone(Vector3 _position);
	public SpitDone onDone;


	float m_Timer;

	static readonly float TRAVEL_TIME = 0.25f;

	void Start() {
		m_Timer = 0;
		transform.position = startPosition;
	}

	void Update() {
		transform.position = Vector3.Lerp(startPosition, target, m_Timer / TRAVEL_TIME);
		Vector3 direction = Vector3.Normalize(target - startPosition);

		float angle = Vector3.Angle(Vector3.right, direction);
		Vector3 cross = Vector3.Cross(Vector3.right, direction);

		if(cross.z < 0) {
			angle = -angle;
		}

		transform.localRotation = Quaternion.Euler(0, 0, angle);
		
		m_Timer += Time.deltaTime;

		if(m_Timer >= TRAVEL_TIME) {
			if(null != onDone) {
				onDone(target);
				Destroy(gameObject);
			}
		}
	}
}
