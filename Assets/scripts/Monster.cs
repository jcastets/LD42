using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour {

	enum AttackMode {
		Tentacle,
		Slime,
		Burp,

	}

	[SerializeField] SpriteRenderer m_Tentacle;
	[SerializeField] SpriteRenderer m_Eye;

	[SerializeField] SpriteRenderer m_Body;

	[SerializeField] List<GameObject> m_Blobs;

	[SerializeField] List<GameObject> m_Slimes;

	public enum MouthState {
		Idle,
		Open,
		Chew,
		Spit
	}

	float m_MouthCD;

	[System.Serializable]
	public class MouthAnimKVP {
		public MouthState state;
		public GameObject gameObject;
	}

	[SerializeField] MouthAnimKVP[] m_MouthElements;
	MouthState m_MouthState;

	float m_TentacleCD;

	static readonly float TENTACLE_CD = 0.3f;

	int m_Victims = 0;
	int m_VictimsSinceLastBlob = 0;
	int m_Credits = 0;

	AttackMode m_AttackMode;

	public int victims {
		get { return m_Victims; }
	}

	public int dna {
		get { return m_Credits; }
	}

	public GameObject mouth {
		get {
			return m_MouthElements[(int)m_MouthState].gameObject;
		}
	}

	void Start () {
		SetMouthState(MouthState.Idle);
		m_AttackMode = AttackMode.Tentacle;
		m_Tentacle.enabled = false;
		m_Slimes = new List<GameObject>();
	}
	
	void Update () {

		if(Input.GetKeyDown(KeyCode.F1)) {
			m_AttackMode = AttackMode.Tentacle;
		} else if(Input.GetKeyDown(KeyCode.F2)) {
			m_AttackMode = AttackMode.Slime;
		} else if(Input.GetKeyDown(KeyCode.F3)) {
			m_AttackMode = AttackMode.Burp;
			BurpAttack();
		} 

		m_TentacleCD -= Time.deltaTime;
		if(m_TentacleCD <= 0) {
			m_Tentacle.enabled = false;
		} else {
			GameObject h = Game.instance.GetHumanAtPoint(m_Tentacle.transform.position, 0.75f);
			if (null != h) {
				//DIE !
				Dude dude = h.GetComponent<Dude>();
				dude.Drop();
				dude.FlyTo(mouth.gameObject);
			}
		}

		UpdateMouth();
	}

	public void Attack(Vector3 _position) {

		switch(m_AttackMode) {
			case AttackMode.Tentacle:
				TentacleAttack(_position);
			break;

			case AttackMode.Slime:
				SlimeAttack(_position);
			break;
		}
	}

	public bool PointInSlime(Vector3 _point) {
		foreach(GameObject slime in m_Slimes) {
			PolygonCollider2D collider = slime.GetComponent<PolygonCollider2D>();
			if(collider.OverlapPoint(_point)) {
				return true;
			}
		}

		return false;
	}

	void TentacleAttack(Vector3 _position) {
		if(m_TentacleCD >= 0) {
			return;
		}

		m_Tentacle.transform.position = _position;
		m_Tentacle.enabled = true;
		m_TentacleCD = TENTACLE_CD;
	}

	void SlimeAttack(Vector3 _position) {

		GameObject go = new GameObject();
		go.name = "slime";
		go.transform.position = _position;

		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = Game.instance.slimeSpr;
		sr.sortingOrder = 1;
		go.AddComponent<PolygonCollider2D>();
		go.AddComponent<Appear>();
		Vanish vanish = go.AddComponent<Vanish>();
		vanish.OnVanishDone = (x) => { m_Slimes.Remove(x); Destroy(x); };
		m_Slimes.Add(go);
		m_AttackMode = AttackMode.Tentacle;
		SetMouthState(MouthState.Spit);
	}

	void BurpAttack() {
		int count = Game.instance.KillAllHumans();
		Game.instance.ShakeCamera();
		m_AttackMode = AttackMode.Tentacle;
	}

	void CheckUnlockBlob() {
		int target = (m_Blobs.Count+1) * 10;
		if(m_VictimsSinceLastBlob >= target) {
			if(Grow()) {
				m_VictimsSinceLastBlob -= target;
			}
		}
	}

	bool Grow() {
		int i = -1;
		m_Blobs.Shuffle();
		Vector3 parent = m_Eye.transform.position;
		do {
			float step = 360f / 8f;
			Vector3 rndDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			rndDirection.Normalize();

			for(float angle = 0; angle < 360f; angle += step) {
				Vector3 direction = Quaternion.Euler(0, 0, angle) * rndDirection;
				direction.Normalize();
				Vector3 candidatePosition = parent + direction * 0.85f;
				if(CanGrowAt(candidatePosition)) {
					//build blob
					GameObject blob = Instantiate(Game.instance.blobsSpr[Random.Range(0, Game.instance.blobsSpr.Length)]);
					blob.transform.parent = transform;
					blob.transform.position = candidatePosition;
					blob.AddComponent<PolygonCollider2D>();
					blob.AddComponent<Blob>();
					m_Blobs.Add(blob);
					blob.GetComponent<SpriteRenderer>().sortingOrder = 6;
					return true;
				}
			}
			i++;
			if(i < m_Blobs.Count) {
				parent = m_Blobs[i].transform.position;
			}
		} while(i < m_Blobs.Count);

		return false;
	}

	bool CanGrowAt(Vector3 _position) {
		if(m_Eye.GetComponent<PolygonCollider2D>().OverlapPoint(_position)) {
			return false;
		}

		if(mouth.GetComponent<PolygonCollider2D>().OverlapPoint(_position)) {
			return false;
		}

		foreach(GameObject blob in m_Blobs) {
			if(blob.GetComponent<PolygonCollider2D>().OverlapPoint(_position)) {
				return false;
			}
		}

		for(float angle = 0; angle < 360; angle += (360f / 8f)) {
			Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
			if(!Game.instance.safeZone.OverlapPoint(_position + direction * 0.625f)) {
				return false;
			}
		}
		return true;
	}

	public void Eat() {
		m_Victims++;
		m_VictimsSinceLastBlob++;
		m_Credits++;
		CheckUnlockBlob();

		SetMouthState(MouthState.Open);
	}

	void UpdateMouth() {
		m_MouthCD -= Time.deltaTime;
		if(m_MouthCD <= 0) {
			if(m_MouthState == MouthState.Open) {
				SetMouthState(MouthState.Chew);
			} else if(m_MouthState == MouthState.Chew) {
				SetMouthState(MouthState.Idle);
			} else if(m_MouthState == MouthState.Spit) {
				SetMouthState(MouthState.Idle);
			}
		}
	}

	void SetMouthState(MouthState _state) {
		foreach(MouthAnimKVP mak in m_MouthElements) {
			mak.gameObject.SetActive(mak.state == _state);
		}
		m_MouthState = _state;

		switch(m_MouthState) {
			case MouthState.Open:
				m_MouthCD = 0.5f;
			break;

			case MouthState.Chew:
				m_MouthCD = 0.5f;
			break;

			case MouthState.Spit:
				m_MouthCD =1.5f;
			break;
		}
	}
}
