using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Vanish : MonoBehaviour {

	public delegate void VanishDone(GameObject _go);
	public VanishDone OnVanishDone;

	enum State {
		Idle,
		Blink,
		None,
	}

	public float delay = 4;
	float m_Cooldown;

	State m_State;
	void Start () {
		m_Cooldown = delay;
		m_State = State.Idle;
	}
	
	void Update () {
		m_Cooldown -= Time.deltaTime;
		if(m_Cooldown <= 0) {
			switch(m_State) {
				case State.Idle:
					m_State = State.Blink;
					m_Cooldown = 2;
					break;

				case State.Blink:
					m_State = State.None;
					if(null != OnVanishDone) {
						OnVanishDone(gameObject);
					}
					break;
			}
		} else {
			if (m_State == State.Blink) {
				SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
				float sin = Mathf.Sin(Time.time * 50);
				sr.enabled = sin > 0f;
			}
		}
	}
}
