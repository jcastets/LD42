﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Game : MonoBehaviour {
	[SerializeField] Monster m_Monster;
	[SerializeField] Humans m_Humans;

	[SerializeField] TMP_Text m_VictimsTxt;

	public List<GameObject> freeSlots;
	public Collider2D safeZone;
	public Sprite wallSpr;
	public Sprite slimeSpr;
	public Sprite pilarSpr;

	public GameObject humanFlyingSpr;

	public GameObject [] blobsSpr;

	static Game s_Instance;

	public enum PowerUpKind {
		Tentacle,
		Slime,
		Burp,
		Ultimate,
	}


	public class PowerUp {
		public PowerUpKind kind;
		public int price;

		public PowerUp(PowerUpKind k, int p) {
			kind = k;
			price = p;
		}
	}


	public static readonly PowerUp [] powerUps = new PowerUp [] { 
		new PowerUp(PowerUpKind.Tentacle, 10),
		new PowerUp(PowerUpKind.Slime, 40),
		new PowerUp(PowerUpKind.Burp, 300),
		new PowerUp(PowerUpKind.Ultimate, 1000)
	};

	public static Game instance {
		get {
			if(null == s_Instance) {
				throw new Exception("Singleton instance null");
			}
			return s_Instance;
		}
	}

	public Monster monster {
		get { return m_Monster; }
	}

	public Humans humans {
		get { return m_Humans; }
	}

	float m_CameraShakeCooldown;

	Game() {
		s_Instance = this;
	}

	void Start () {
		m_CameraShakeCooldown = 0;
	}

	void Update() {
		if(Input.GetMouseButtonDown(0)) {
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				p.z = 0;
				m_Monster.Attack(p);
			}
		}
		
		if(Input.GetKeyDown(KeyCode.Alpha1)) {
			int p = powerUps[(int)PowerUpKind.Tentacle].price;
			monster.BuyTentacle(p);
		}

		if(Input.GetKeyDown(KeyCode.Alpha2)) {
			int p = powerUps[(int)PowerUpKind.Slime].price;
			monster.BuySlime(p);
		}

		if(Input.GetKeyDown(KeyCode.Alpha3)) {
			int p = powerUps[(int)PowerUpKind.Burp].price;
			monster.BuyBurp(p);
		}

		UpdateCamera();
	}

	public GameObject GetHumanAtPoint(Vector3 _position, float _radius) {
		return m_Humans.GetHumanAtPoint(_position, _radius);
	}

	public int KillAllHumans() {
		return m_Humans.KillAll();
	}

	public void ShakeCamera() {
		m_CameraShakeCooldown = 1;
	}
	
	void UpdateCamera() {
		m_CameraShakeCooldown -= Time.deltaTime;

		Vector3 oPosition = Vector3.forward * -10;
		if(m_CameraShakeCooldown > 0) {

			float sin = Mathf.Sin(Time.time * 2000) * 0.09f;
			Vector3 position = oPosition;

			float rndSign = UnityEngine.Random.Range(0, 100) > 50 ? -1f : 1f;
			position.x += (sin * rndSign);
			
			rndSign = UnityEngine.Random.Range(0, 100) > 50 ? -1f : 1f;
			position.y += (sin * rndSign);
			Camera.main.transform.position = position; 
		} else {
			Vector3 position = Vector3.Slerp(Camera.main.transform.position, oPosition, Time.deltaTime * 16f);
			Camera.main.transform.position = position; 
		}
	}
}
