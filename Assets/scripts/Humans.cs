using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Humans : MonoBehaviour {
	[SerializeField] Sprite m_HumanSpr;

	List<GameObject> m_Humans = new List<GameObject>();
	List<GameObject> m_Trash = new List<GameObject>();

	float m_SpawnCD;

	float m_SpeedMul = 1;
	
	static readonly int MAX_ALIVE_HUMANS = 64;

	// Use this for initialization
	void Start () {
		m_SpawnCD = GetSpawnCooldown();
	}
	
	// Update is called once per frame
	void Update () {
		m_SpawnCD -= Time.deltaTime;
		if(m_SpawnCD <= 0) {
			if(Spawn()) {
				m_SpawnCD = GetSpawnCooldown();
			}
		}

		m_SpeedMul += Time.deltaTime * 0.0025f;
		m_SpeedMul = Mathf.Clamp(m_SpeedMul, 1f, 1.5f);

		UpdateHumans();
	}

	float GetSpawnCooldown() {
		float slotFactor = ((float)Game.instance.freeSlots.Count+1) / 25f;
		return (slotFactor / m_SpeedMul) * 2f;
	}

	int GetSpawnCount() {
		return 3;
	}

	void UpdateHumans() {
		foreach(GameObject h in m_Humans) {

			Dude dude = h.GetComponent<Dude>();
			if(!dude.isAlive) {
				m_Trash.Add(h);
				continue;
			}

			if(dude.isFlying) {
				
				if((dude.target.transform.position - dude.transform.position).magnitude <= 2.5f) {
					Game.instance.monster.OpenMouth();
				}
				if((dude.target.transform.position - dude.transform.position).magnitude <= 0.5f) {
					dude.isAlive = false;
					Game.instance.monster.Eat();
				}

				Vector3 targetScale = Vector3.one;
				float completion = (dude.distance / dude.distanceToGo);
				if( completion <= 0.5f) {
					targetScale *= 3f;
				}

				dude.transform.localScale = Vector3.Slerp(dude.transform.localScale, targetScale, Time.deltaTime * 4f);
			}
			else {
				if(dude.hasWall && null != dude.wall) {
					if(!dude.target.GetComponent<Slot>().isFree) {
						dude.Drop();
						dude.GoBack();
					} else {
						if(dude.target.GetComponent<PolygonCollider2D>().OverlapPoint(dude.wall.transform.position)) {
							dude.Build();
							dude.GoBack();
						}
					}
				} else {
					if(!h.GetComponent<SpriteRenderer>().isVisible && !dude.hasWall) {
						m_Trash.Add(h);
					}
				}
			}

			Vector3 direction = dude.direction;
			if(dude.lurching) {
				float sin = Mathf.Sin(Time.time * 5f);
				direction.x += sin * 0.1f;
				direction.y += sin * 0.1f;
				direction.Normalize();
			}

			float speed = dude.speed;
			if(dude.speedVariant) {
				float sin = Mathf.Sin(Time.time*4) * 0.5f + 0.5f;
				speed += 2f * (sin * 0.7f + 0.3f);
			}

			if(!dude.isFlying && Game.instance.monster.PointInSlime(h.transform.position)) {
				speed = 0.1f;
				dude.sweatFX.SetActive(true);
			} else {
				dude.sweatFX.SetActive(false);
			}

			Vector3 move = direction * speed * Time.deltaTime;
			dude.distance += move.magnitude;
			h.transform.position += move;
			float angle = Vector3.Angle(Vector3.left, direction);
			Vector3 cross = Vector3.Cross(Vector3.left, direction);

			if(cross.z < 0) {
				angle = -angle;
			}

			h.transform.localRotation = Quaternion.Euler(0, 0, angle);
		}

		foreach(GameObject h in m_Trash) {
			m_Humans.Remove(h);
			Destroy(h);
		}

		m_Trash.Clear();
	}

	public GameObject GetHumanAtPoint(Vector3 _position, float _radius) {
		foreach(GameObject h in m_Humans) {
			Vector3 p = h.transform.position;
			if((p-_position).magnitude <= _radius) {
				return h;
			}
		}
		return null;
	}

	public int KillAll() {
		int count = 0;
		foreach(GameObject h in m_Humans) {
			Dude dude = h.GetComponent<Dude>();
			if(dude.isAlive) {
				dude.Drop();
				dude.FlyTo(Game.instance.monster.mouth);
				count++;
			}
		}
		return count;
	}

	bool Spawn() {

		if(Game.instance.state != Game.GameState.Play)
		{
			return false;
		}

		bool anySpawn = false;

		int spawnCount = GetSpawnCount();

		for(int i=0;i<spawnCount;++i) {
			GameObject target = null;
			Vector3? p = RandomizePosition(out target);
			if(null == p || null == target) {
				return anySpawn;
			}

			if(m_Humans.Count >= MAX_ALIVE_HUMANS) {
				return anySpawn;
			}

			GameObject h = new GameObject();
			h.name = "Jean_" + Time.time;
			Dude dude = h.AddComponent<Dude>();
			dude.target = target;
			dude.speed = Random.Range(0.2f, 0.8f) * m_SpeedMul;

			if(Random.Range(0, 100) < 10) {
				dude.lurching = true;
			}

			if(Random.Range(0, 100) < 5) {
				dude.speedVariant = true;
			}

			SpriteRenderer sr = h.AddComponent<SpriteRenderer>();
			sr.sortingOrder = 15;
			sr.sprite = m_HumanSpr;
			h.transform.SetParent(transform);
			m_Humans.Add(h);
			h.transform.position = p.Value;
			dude.direction =  Vector3.Normalize(target.transform.position - h.transform.position);

			GameObject sweat = Instantiate(Game.instance.humanSweat);
			sweat.transform.SetParent(h.transform, false);

			dude.sweatFX = sweat;
			sweat.SetActive(false);

			anySpawn = true;
		}
		return anySpawn;
	}

	Vector3? RandomizePosition(out GameObject _slot) {
		_slot = null;
		if( Game.instance.freeSlots.Count <= 0) {
			return null;
		}

		int index = Random.Range(0, Game.instance.freeSlots.Count);

		GameObject slot = Game.instance.freeSlots[index];
		Vector3 direction = slot.transform.rotation * Vector3.left;
		direction = -direction;
		direction.x += Random.Range(-0.3f, 0.3f);
		direction.y += Random.Range(-0.3f, 0.3f);
		direction.Normalize();
		_slot = slot;
		return direction * 11f;
	}

	public Vector3? FindNearestHumanPositionTo(Vector3 _position) {
		List<Vector3> candidates = new List<Vector3>();
		foreach(GameObject h in m_Humans) {
			Dude dude = h.GetComponent<Dude>();
			if(!dude.isAlive || !dude.hasWall) {
				continue;
			}
			candidates.Add(dude.transform.position);
		}

		candidates.Sort(( x, y) =>  
			(_position - x).magnitude.CompareTo((_position - y).magnitude) 
		);

		if(candidates.Count > 0) {
			return candidates[0];
		}
		return null;
	}

	public void Defeat() {
		
	}

	public void Victory() {

	}
}
