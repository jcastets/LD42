using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour {

	[SerializeField] Image m_Foreground;
	[SerializeField] Image m_Fill;


	public void SetCompletion(float _Completion) {
		m_Fill.fillAmount = Mathf.Clamp01(_Completion);
	}

}
