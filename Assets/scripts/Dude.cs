using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dude : MonoBehaviour {

	public bool hasWall = true;
	public bool isAlive = true;
	public bool isFlying = false;

	public float speed = 3;
	public float distance = 0;
	public float distanceToGo = 0;

	public Vector3 direction = Vector3.zero;
	

	public bool speedVariant = false;
	public bool lurching = false;

	public GameObject sweatFX;
	public GameObject steps;

	public GameObject target = null;

	GameObject m_Wall;

	public GameObject wall {
		get {
			return m_Wall;
		}
	}

	void Start() {
		m_Wall = new GameObject();
		m_Wall.name = "wall";
		SpriteRenderer sr = m_Wall.AddComponent<SpriteRenderer>();
		sr.sprite = Game.instance.wallSpr;
		sr.sortingOrder = 14;
		m_Wall.AddComponent<PolygonCollider2D>();
		m_Wall.transform.SetParent(transform, false);
		m_Wall.transform.localPosition = Vector3.left * 0.23f;
	}

	public void Build() {
		if(null == m_Wall) {
			return;
		}
		hasWall = false;
		m_Wall.transform.SetParent(null, true);
		m_Wall.transform.position = target.transform.position;
		m_Wall.transform.rotation = target.transform.rotation;
		Game.instance.freeSlots.Remove(target);
		target.GetComponent<Slot>().Build();
		m_Wall = null;
		Game.instance.monster.WallBuilt();
	}
	public void Drop() {
		if(null == m_Wall) {
			return;
		}
		hasWall = false;
		m_Wall.transform.SetParent(null, true);
		Vanish vanish = m_Wall.AddComponent<Vanish>();
		vanish.delay = 1;
		vanish.OnVanishDone = (x) => { Destroy(x); };
		m_Wall = null;
	}

	public void GoBack() {
		direction = -direction;
		speed = Random.Range(speed, speed * 2f);
		lurching = Random.Range(0,10) == 0;
	}

	public void FlyTo(GameObject _target) {
		target = _target;
		lurching = false;
		speedVariant = false;
		direction = Vector3.Normalize(target.transform.position - transform.position);
		speed = 15f;
		isFlying = true;
		steps.SetActive(false);
		sweatFX.SetActive(false);
		distanceToGo = (target.transform.position - transform.position).magnitude;
		distance = 0;

		SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
		Destroy(sr);

		PolygonCollider2D c = gameObject.GetComponent<PolygonCollider2D>();
		Destroy(c);

		GameObject flying = Instantiate(Game.instance.humanFlyingSpr);
		flying.transform.SetParent(transform, false);
	}
	
}
