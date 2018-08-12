using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Game : MonoBehaviour {
	[SerializeField] Monster m_Monster;
	[SerializeField] Humans m_Humans;

	[SerializeField] TMP_Text m_VictimsTxt;

	public List<GameObject> freeSlots;
	public Collider2D safeZone;
	public Sprite wallSpr;
	public Sprite slimeSpr;

	public GameObject [] blobsSpr;

	static Game s_Instance;
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
            Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			p.z = 0;
			m_Monster.Attack(p);
		}
		UpdateCamera();
		UpdateUI();
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

	void UpdateUI() {
		m_VictimsTxt.text = "VICTIMS: " + m_Monster.victims;
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
