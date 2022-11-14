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

		GameManager._.OnAttackCreated += AttackCreatedSound;

		GameManager._.OnShieldCreated += ShieldCreatedSound;
		GameManager._.OnShieldFusion += ShieldFusionSound;

		//GameManager._.OnMoreButtonCursorSwap += SwapSound;
		//GameManager._.OnDoubleCursorSwap += SwapSound;
	}

	void SwapSound ()
	{
		 
	}

	void AttackCreatedSound (UnitAttackInfo info)
	{
		if (info.pg.tag == this.tag)
		{

		}
	}

	void ShieldCreatedSound (UnitShieldInfo info)
	{
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
}
