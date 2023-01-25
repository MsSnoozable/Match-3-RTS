using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComboUI : MonoBehaviour
{
	public TextMeshProUGUI comboCounter;

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
				case 0:
					break;
				default:
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
	}
}
