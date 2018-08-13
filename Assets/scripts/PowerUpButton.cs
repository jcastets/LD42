using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PowerUpButton : MonoBehaviour {

	[SerializeField] TMP_Text m_Price;
	
	[SerializeField] Button m_Button;

	public void SetListener(UnityAction _Action) {
		m_Button.onClick.RemoveAllListeners();
		m_Button.onClick.AddListener(_Action);
	}

	public void SetPrice(int _price) {
		m_Price.text = string.Format("{0:0000}", _price);
	}

}
