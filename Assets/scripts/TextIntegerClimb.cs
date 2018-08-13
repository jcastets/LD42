using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextIntegerClimb : MonoBehaviour {

	

	public int target { get; set;}
	float m_Current;
	float m_Speed;
	TMP_Text m_Text;

	[SerializeField] float m_Delay;
	[SerializeField] string m_Format;
	float m_Timer;

	// Use this for initialization
	void Start () {
		m_Current = 0;
		m_Speed = 10;
		m_Text = GetComponent<TMP_Text>();
		m_Text.text = string.Format(m_Format, m_Current);
		m_Timer = 0;
	}
	
	// Update is called once per frame
	void Update () {

		if(m_Timer >= m_Delay) {
			m_Current += Time.deltaTime * m_Speed;
			if(m_Current > target) {
				m_Current = target;
			}
			m_Text.text = string.Format(m_Format, (int)m_Current);
			m_Speed *= 1.1f;
		}

		m_Timer += Time.deltaTime;
	}
}
