using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slot : MonoBehaviour {
	public bool isFree = true;

	bool hasBuiltLeftPilar = false;
	bool hasBuiltRightPilar = false;

	[SerializeField] Slot m_Left;
	[SerializeField] Slot m_Right;

	public void Build() {
		isFree = false;
		if(!m_Left.hasBuiltRightPilar) {
			GameObject go = new GameObject();
			go.name = "pilar";
			SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
			sr.sprite = Game.instance.pilarSpr;
			sr.sortingOrder = 5;
			go.transform.position = transform.position + Quaternion.Euler(0,0,-90) * transform.right * 0.43f;
			go.transform.localRotation = transform.localRotation;
			hasBuiltLeftPilar = true;
		}

		if(!m_Right.hasBuiltLeftPilar) {
			GameObject go = new GameObject();
			go.name = "pilar";
			SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
			sr.sprite = Game.instance.pilarSpr;
			sr.sortingOrder = 5;
			go.transform.position = transform.position - Quaternion.Euler(0,0,-90) * transform.right * 0.43f;
			go.transform.localRotation = transform.localRotation;
			hasBuiltRightPilar = true;
		}
	}


}
