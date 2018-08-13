using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour {

	static readonly int MAX_TENTACLES = 3;

	static readonly float TENTACLE_CD = 1.0f;	
	static readonly float SLIME_CD = 10f;
	static readonly float BURP_CD = 30f;

	static readonly int MAX_BLOBS = 512;

	bool m_IsUlting;

	[SerializeField] SpriteRenderer m_Body;

	[SerializeField] List<GameObject> m_Blobs;

	[SerializeField] List<GameObject> m_Slimes;
	[SerializeField] GameObject m_Iris;

	[SerializeField] GameObject m_TentaclePrefab;

	class Tentacle {
		public GameObject gameObject;
		public float cooldown;
	}

	List<Tentacle> m_Tentacles;
	public int tentacleCount {
		get { return null != m_Tentacles ? m_Tentacles.Count : 0; }
	}

	float m_SlimeCD;
	float m_BurpCD;
	float m_SpitCD;

	Vector3? m_SpitTarget;

	public float slimeCompletion {
		get { return 1f - m_SlimeCD / SLIME_CD;}
	}

	public float burpCompletion {
		get { return 1f - m_BurpCD / BURP_CD; }
	}

	public bool hasSlime {
		get { return m_SlimeEnabled; }
	}

	public bool hasBurp {
		get { return m_BurpEnabled; }
	}


	public enum MouthState {
		Idle,
		Open,
		Chew,
		Spit
	}

	public enum EyeState {
		Blink,
		Angry,
		Affraid,
	}

	EyeState m_EyeState;
	float m_EyeCD;

	[SerializeField] Vector3 m_IrisRestPosition;
	Vector3 m_IrisWantedPosition;

	[System.Serializable]
	public class EyeAnimKVP {
		public EyeState state;
		public GameObject gameObject;
	}

	[SerializeField] EyeAnimKVP[] m_EyeElements;

	float m_MouthCD;

	[System.Serializable]
	public class MouthAnimKVP {
		public MouthState state;
		public GameObject gameObject;
	}

	[SerializeField] MouthAnimKVP[] m_MouthElements;
	MouthState m_MouthState;

	int m_Victims = 0;
	int m_VictimsSinceLastBlob = 0;
	int m_Credits = 0;

	bool m_SlimeEnabled = false;
	bool m_BurpEnabled = false;

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

	public GameObject eye {
		get {
			return m_EyeElements[(int)m_EyeState].gameObject;
		}
	}

	void Start () {
		m_Credits = 0;

		SetMouthState(MouthState.Idle);
		SetEyeState(EyeState.Blink);
		AddTentacle();
		
		m_IrisWantedPosition = m_IrisRestPosition;
		m_Slimes = new List<GameObject>();

		m_SpitTarget = null;
	}

	public void BuyTentacle(int price) {
		if(m_Tentacles.Count >= MAX_TENTACLES) {
			return;
		}
		if(m_Credits >= price) {
			m_Credits -= price;
			AddTentacle();
		}
	}

	public void BuySlime(int price) {
		if(m_SlimeEnabled) {
			return;
		}
		if(m_Credits >= price) {
			m_Credits -= price;
			m_SlimeEnabled = true;
		}
	}

	public void BuyBurp(int price) {
		if(m_BurpEnabled) {
			return;
		}
		if(m_Credits >= price) {
			m_Credits -= price;
			m_BurpEnabled = true;
		}
	}
	void AddTentacle() {
		
		if (null == m_Tentacles) {
			m_Tentacles = new List<Tentacle>();
		}

		if(m_Tentacles.Count >= MAX_TENTACLES) {
			return;
		}

		Tentacle tentacle = new Tentacle();
		tentacle.gameObject = Instantiate(m_TentaclePrefab);
		tentacle.gameObject.SetActive(false);
		tentacle.cooldown = 0;
		m_Tentacles.Add(tentacle);
	}

	Tentacle GetFreeTentacle() {
		foreach(Tentacle t in m_Tentacles) {
			if(t.cooldown <= 0) {
				return t;
			} 
		}
		return null;
	}
	
	void Update () {

		if(m_IsUlting) {
			UpdateUltimate();
			UpdateMouth();
			UpdateEye();
			return;
		}

		if(Input.GetKeyDown(KeyCode.F2)) {
			Vector3 p = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			p.z = 0;
			SlimeAttack(p);
		} else if(Input.GetKeyDown(KeyCode.F3)) {
			BurpAttack();
		}

		UpdateTentacles();
		m_SlimeCD -= Time.deltaTime;
		m_BurpCD -= Time.deltaTime;
		m_SpitCD -= Time.deltaTime;

		if(m_SpitCD <= 0 && null != m_SpitTarget) {
			Spit();
		}

		UpdateMouth();
		UpdateEye();
	}

	public void Attack(Vector3 _position) {
		TentacleAttack(_position);
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
		Tentacle t = GetFreeTentacle();
		if(null != t) { 
			t.gameObject.transform.position = _position;
			t.gameObject.SetActive(true);
			t.cooldown = TENTACLE_CD;
			SetEyeState(EyeState.Angry);
		}
	}

	void SlimeAttack(Vector3 _position) {
		if(!m_SlimeEnabled) {
			return;
		}

		if(m_SlimeCD > 0) {
			return;
		}

		SetMouthState(MouthState.Spit);
		m_SpitCD = 0.5f;
		m_SpitTarget = _position;
		m_SlimeCD = SLIME_CD;
	}

	void Spit() {
		GameObject spitObject = Instantiate(Game.instance.slimeBulletSpr);
		spitObject.name = "spit";

		Spit spit = spitObject.AddComponent<Spit>();
		spit.startPosition = mouth.transform.position;
		spit.target = m_SpitTarget.Value;
		m_SpitTarget = null;
		spit.onDone = PutSlime;
	}

	void PutSlime(Vector3 _position) {
		GameObject go = Instantiate(Game.instance.slimeSpr);
		go.name = "slime";
		go.transform.position = _position;

		go.AddComponent<Appear>();
		Vanish vanish = go.AddComponent<Vanish>();
		vanish.OnVanishDone = (x) => { m_Slimes.Remove(x); Destroy(x); };
		m_Slimes.Add(go);
		SetEyeState(EyeState.Angry);
	}

	void BurpAttack() {
		if(!m_BurpEnabled) {
			return;
		}

		if(m_BurpCD > 0) {
			return;
		}

		int count = Game.instance.KillAllHumans();
		Game.instance.ShakeCamera();
		SetEyeState(EyeState.Angry);

		m_BurpCD = BURP_CD;
	}

	void CheckUnlockBlob() {
		int target = (m_Blobs.Count+1) * 5;
		if(m_VictimsSinceLastBlob >= target) {
			if(Grow()) {
				m_VictimsSinceLastBlob -= target;
			}
		}
	}

	bool Grow(float _distance = 0.85f) {
		int i = -1;
		m_Blobs.Shuffle();
		Vector3 parent = eye.transform.position;
		do {
			float step = 360f / 8f;
			Vector3 rndDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			rndDirection.Normalize();

			for(float angle = 0; angle < 360f; angle += step) {
				Vector3 direction = Quaternion.Euler(0, 0, angle) * rndDirection;
				direction.Normalize();
				Vector3 candidatePosition = parent + direction * _distance;
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
		if(eye.GetComponent<PolygonCollider2D>().OverlapPoint(_position)) {
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

		if(!m_IsUlting) {
			for(float angle = 0; angle < 360; angle += (360f / 8f)) {
				Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.up;
				if(!Game.instance.safeZone.OverlapPoint(_position + direction * 0.625f)) {
					return false;
				}
			}
		}
		return true;
	}

	public void Eat() {
		m_Victims++;
		m_VictimsSinceLastBlob++;
		m_Credits++;
		CheckUnlockBlob();

		
	}

	public void WallBuilt() {
		SetEyeState(EyeState.Affraid);
	}

	void UpdateTentacles() {
		foreach(Tentacle t in m_Tentacles) {
			t.cooldown -= Time.deltaTime;
			if(t.cooldown <= 0) {
				t.gameObject.SetActive(false);
			} else {
				GameObject h = Game.instance.GetHumanAtPoint(t.gameObject.transform.position, 1f);
				if (null != h) {
					
					Dude dude = h.GetComponent<Dude>();
					if(dude.isAlive && !dude.isFlying) {
						dude.Drop();
						dude.FlyTo(mouth.gameObject);
					}
				}
			}
		}
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
		if(m_MouthState == MouthState.Spit && m_MouthCD > 0) {
			return;
		}

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
				m_MouthCD = 0.55f;
			break;
		}
	}

	void UpdateEye() {
		m_EyeCD -= Time.deltaTime;
		if(m_EyeCD <= 0) {
			SetEyeState(EyeState.Blink);
		}

		Vector3? h = Game.instance.humans.FindNearestHumanPositionTo(Vector3.zero);
		if(null == h) {
			m_IrisWantedPosition = m_IrisRestPosition;
		} else {
			Vector3 direction = Vector3.Normalize(h.Value - Vector3.zero);
			direction.y *= 0.5f;
			m_IrisWantedPosition = m_IrisRestPosition + direction * 0.2f;
		}

		m_Iris.transform.position = Vector3.Slerp(m_Iris.transform.position, m_IrisWantedPosition, Time.deltaTime * 4f);
	}

	void SetEyeState(EyeState _state) {
		foreach(EyeAnimKVP mak in m_EyeElements) {
			mak.gameObject.SetActive(mak.state == _state);
		}
		m_EyeState = _state;
		m_EyeCD = 0.75f;

		if(m_EyeState == EyeState.Blink) {
			m_EyeCD = 6f;
		}
	}

	public float GetTentacleCompletion(int _id) {
		if(_id >= m_Tentacles.Count) {
			return 0;
		}

		return 1f - m_Tentacles[_id].cooldown / TENTACLE_CD;
	}

	public void Victory() {
		m_Credits -= Game.powerUps[(int)Game.PowerUpKind.Ultimate].price;
		m_IsUlting = true;
	}

	public void Defeat() {
		GameObject cover = new GameObject();
		cover.name = "cover";

		SpriteRenderer sr = cover.AddComponent<SpriteRenderer>();
		sr.sprite = Game.instance.coverSpr;
		sr.sortingOrder = 25;

		cover.AddComponent<CoverScale>();
	}

	void UpdateUltimate() {
		if(m_Blobs.Count < MAX_BLOBS) {
			Grow(2f);
		}
	}

	public void OpenMouth() {
		SetMouthState(MouthState.Open);
	}
}
