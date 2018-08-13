using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TextGrow : MonoBehaviour {


	[SerializeField] float m_MinSize = 0;
	[SerializeField] float m_MaxSize = 220;
	[SerializeField] float m_Delay = 0;

	float m_Timer = 0;

	TMP_Text m_Text;

	void Start() {
		m_Text = GetComponent<TMP_Text>();
	}

	void OnEnable() {
		if(null == m_Text) {
			m_Text = GetComponent<TMP_Text>();
		}
		
		m_Text.fontSize = m_MinSize;
		m_Timer = 0;
	}

	void Update() {
		if(m_Timer >= m_Delay) {
			m_Text.fontSize = Mathf.Lerp(m_Text.fontSize, m_MaxSize, Time.deltaTime * 16f);
		}
		m_Timer += Time.deltaTime;
	}

}
