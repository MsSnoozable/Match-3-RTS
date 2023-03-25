using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboUI : MonoBehaviour
{
	public TextMeshProUGUI comboCounter;
	public TextMeshProUGUI comboFinalizerText;


	// Start is called before the first frame update
	void Start()
    {
		GameManager._.OnUpdateComboCount += DisplayCombo;
		GameManager._.OnFinalizeComboCount += FinalizeCombo;
    }

	void OnDestroy()
	{
        GameManager._.OnUpdateComboCount -= DisplayCombo;
		GameManager._.OnFinalizeComboCount -= FinalizeCombo;
	}

	void FinalizeCombo(int comboCount, string playerTag)
	{
		if (playerTag == this.tag)
		{
			switch (comboCount)
			{
				case 1:
					comboFinalizerText.text = "Weak!";
					break;
				case 2:
					comboFinalizerText.text = "double!";
					break;
				case 3:
					comboFinalizerText.text = "triple!";
					break;
				case 4:
					comboFinalizerText.text = "Impressive!";
					break;
				default:
					comboFinalizerText.text = "Ultraaaaaa!";
					break;
			}
			StartCoroutine(HideCombo());
		}
	}

	void DisplayCombo(int comboCount, string playerTag)
	{
		if (playerTag == this.tag)
		{
			StopAllCoroutines();
			comboCounter.gameObject.SetActive(true);
			comboCounter.text = "Combo " + comboCount;
		}
	}

	IEnumerator HideCombo ()
	{
		//Hide combo
		yield return new WaitForSeconds(3f);
		comboCounter.gameObject.SetActive(false);
		comboFinalizerText.gameObject.SetActive(false);
	}
}
