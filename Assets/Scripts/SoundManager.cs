using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{

	public Sounds[] sounds;

	private void Start()
	{
		/*foreach (Sounds s in sounds)
		{

		}*/

		GameManager.i.OnAttackCreated += AttackCreatedSound;

		GameManager.i.OnShieldCreated += ShieldCreatedSound;
		GameManager.i.OnShieldFusion += ShieldFusionSound;
		GameManager.i.OnShieldHold += ShieldHoldSound;

		//GameManager.i.OnMoreButtonCursorSwap += SwapSound;
		//GameManager.i.OnDoubleCursorSwap += SwapSound;
	}

	void SwapSound ()
	{
		 
	}

	IEnumerator AttackCreatedSound (UnitAttackInfo info)
	{
		if (info.pg.tag == this.tag)
		{
			yield return null;
		}
	}

	IEnumerator ShieldCreatedSound (UnitShieldInfo info)
	{
		yield return null;
		if (info.pg.tag == this.tag)
		{
			switch (info.shielders.Count)
			{
				case 4:
					//play two sounds
					break;
				case 5:
					//play three sounds
					break;
				case 6:
					//play four sounds
					break;
				case 7:
					//play five sounds
					break;
				default:
					//play one sound
					break;
			}
		}
	}

	void ShieldFusionSound (UnitShieldInfo info)
	{

	}
	void ShieldHoldSound(UnitShieldInfo info)
	{

	}
}
