using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour 
{

	[SerializeField] Image [] m_TentacleGauges;
	[SerializeField] Image m_SlimeGauge;
	[SerializeField] Image m_BurpGauge;

	static GameUI s_Instance;

	public static GameUI instance {
		get {
			if(null == s_Instance) {
				throw new System.Exception("Singleton instance null");
			}
			return s_Instance;
		}
	}

	GameUI() {
		s_Instance = this;
	}

	void Update() {
		Monster monster = Game.instance.monster;
		for(int i=0; i<m_TentacleGauges.Length; ++i) {
			m_TentacleGauges[i].gameObject.SetActive(i < monster.tentacleCount);
		}
	}
}
