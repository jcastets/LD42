using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour 
{

	[SerializeField] Gauge [] m_TentacleGauges;
	[SerializeField] Gauge m_SlimeGauge;
	[SerializeField] Gauge m_BurpGauge;

	[SerializeField] TMP_Text m_Victims;
	[SerializeField] TMP_Text m_DNA;

	[SerializeField] PowerUpButton [] m_BuyTentacles;
	[SerializeField] PowerUpButton m_BuySlime;
	[SerializeField] PowerUpButton m_BuyBurp;

	[SerializeField] GameObject m_GameOver;

	[SerializeField] TextIntegerClimb m_ScoreVictims;
	[SerializeField] TextIntegerClimb m_ScoreBonus;
	[SerializeField] TextIntegerClimb m_ScoreTotal;

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

	void Start() {
		for(int i=0; i<m_BuyTentacles.Length; ++i) {
			m_BuyTentacles[i].SetListener(BuyTentacle);
		}
		m_BuySlime.SetListener(BuySlime);
		m_BuyBurp.SetListener(BuyBurp);
		m_GameOver.SetActive(false);
	}

	void Update() {
		Monster monster = Game.instance.monster;
		for(int i=0; i<m_TentacleGauges.Length; ++i) {
			Gauge g = m_TentacleGauges[i];
			bool hasTentacle = i < monster.tentacleCount;
			g.gameObject.SetActive(hasTentacle);
			g.SetCompletion(monster.GetTentacleCompletion(i));
		}

		for(int i=0; i<m_BuyTentacles.Length; ++i) {
			m_BuyTentacles[i].gameObject.SetActive(monster.tentacleCount == (i+1));
			m_BuyTentacles[i].SetPrice(Game.powerUps[(int)Game.PowerUpKind.Tentacle].price);
		}
		
		m_SlimeGauge.gameObject.SetActive(monster.hasSlime);
		m_SlimeGauge.SetCompletion(monster.slimeCompletion);
		m_BuySlime.gameObject.SetActive(!monster.hasSlime);
		m_BuySlime.SetPrice(Game.powerUps[(int)Game.PowerUpKind.Slime].price);

		m_BurpGauge.gameObject.SetActive(monster.hasBurp);
		m_BurpGauge.SetCompletion(monster.burpCompletion);
		m_BuyBurp.gameObject.SetActive(!monster.hasBurp);
		m_BuyBurp.SetPrice(Game.powerUps[(int)Game.PowerUpKind.Burp].price);

		m_Victims.text = string.Format("{0:0000}", monster.victims);
		m_DNA.text = string.Format("{0:0000}", monster.dna);
	}

	void BuyTentacle() {
		Game.instance.monster.BuyTentacle(Game.powerUps[(int)Game.PowerUpKind.Tentacle].price);
	}

	void BuySlime() {
		Game.instance.monster.BuySlime(Game.powerUps[(int)Game.PowerUpKind.Slime].price);
	}

	void BuyBurp() {
		Game.instance.monster.BuyBurp(Game.powerUps[(int)Game.PowerUpKind.Burp].price);
	}

	public void GameOver(bool _victory, int _score) {
		m_GameOver.SetActive(true);
		TMP_Text t = m_GameOver.GetComponentInChildren<TMP_Text>();
		if(_victory) {
			t.text = "VICTORY!";
			t.colorGradientPreset = Game.instance.goldPreset;
		} else {
			t.text = "GAME OVER";
			t.colorGradientPreset = Game.instance.redPreset;
		}

		m_ScoreVictims.target = _score;
		m_ScoreBonus.target = Game.instance.freeSlots.Count;
		if(Game.instance.freeSlots.Count > 0) {
			m_ScoreTotal.target = _score * Game.instance.freeSlots.Count;
		} else {
			m_ScoreTotal.target = _score;
		}

	}
}
