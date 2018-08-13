using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour {

	[SerializeField] Image m_Foreground;
	[SerializeField] Image m_Fill;


	public void SetCompletion(float _Completion) {
		m_Fill.fillAmount = Mathf.Clamp01(_Completion);
	}

	void Update() {
		if(m_Fill.fillAmount >= 1f-10e3) {
			float sin = Mathf.Sin(Time.time * 50);
			//m_Fill.enabled = sin > 0f;
		}
	}

}
