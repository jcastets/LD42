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
	[SerializeField] SpriteRenderer m_Mouth;
	[SerializeField] SpriteRenderer m_Body;

	[SerializeField] List<GameObject> m_Blobs;

	[SerializeField] List<GameObject> m_Slimes;

	int [] blobUnlocks;

	float m_TentacleCD;

	static readonly float TENTACLE_CD = 0.5f;

	int m_Victims = 0;
	int m_VictimsSinceLastBlob = 0;
	int m_Credits = 0;

	int m_Level = 0;

	AttackMode m_AttackMode;

	public int victims {
		get { return m_Victims; }
	}

	public int dna {
		get { return m_Credits; }
	}

	public GameObject mouth {
		get {
			return m_Mouth.gameObject;
		}
	}

	void Start () {

		m_AttackMode = AttackMode.Tentacle;
		m_Tentacle.enabled = false;
		blobUnlocks = new int[m_Blobs.Count];
		int unlock = 10;
		for(int i=0; i< m_Blobs.Count; ++i) {
			
			blobUnlocks[i] = unlock;
			unlock += i * 10;
		}
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

		if(Input.GetKeyDown(KeyCode.G)) {
			Grow();
		}

		m_TentacleCD -= Time.deltaTime;
		if(m_TentacleCD <= 0) {
			m_Tentacle.enabled = false;
		}
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

		GameObject h = Game.instance.GetHumanAtPoint(_position, 0.75f);
		if (null != h) {
			//DIE !
			Dude dude = h.GetComponent<Dude>();
			dude.Drop();
			dude.FlyTo(m_Mouth.gameObject);
		}
	}

	void SlimeAttack(Vector3 _position) {

		GameObject go = new GameObject();
		go.name = "slime";
		go.transform.position = _position;

		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = Game.instance.slimeSpr;
		sr.sortingOrder = 1;
		go.AddComponent<PolygonCollider2D>();
		Vanish vanish = go.AddComponent<Vanish>();
		vanish.OnVanishDone = (x) => { m_Slimes.Remove(x); Destroy(x); };
		m_Slimes.Add(go);
		m_AttackMode = AttackMode.Tentacle;
	}

	void BurpAttack() {
		int count = Game.instance.KillAllHumans();
		Game.instance.ShakeCamera();
		m_AttackMode = AttackMode.Tentacle;
	}

	void CheckUnlockBlob() {
		if(m_Level >= blobUnlocks.Length) {
			return;
		}
		if(m_VictimsSinceLastBlob >= blobUnlocks[m_Level]) {
		}
	}

	void Grow() {
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
					return;
				}
			}
			i++;
			if(i < m_Blobs.Count) {
				parent = m_Blobs[i].transform.position;
			}
		} while(i < m_Blobs.Count);
	}

	bool CanGrowAt(Vector3 _position) {
		if(m_Eye.GetComponent<PolygonCollider2D>().OverlapPoint(_position)) {
			return false;
		}

		if(m_Mouth.GetComponent<PolygonCollider2D>().OverlapPoint(_position)) {
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
	}
}
