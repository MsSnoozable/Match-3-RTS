using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTravel : MonoBehaviour
{
	public float travelSpeed;
	public int attackStrength = 22;

	private void Awake()
	{
		StartCoroutine(DelayedDelete());
	}

	IEnumerator DelayedDelete()
	{
		yield return new WaitForSeconds(0.1f);
		GetComponent<Rigidbody2D>().WakeUp(); //makes the rigidbody active after some time
		
		yield return new WaitForSeconds(2f);

		Destroy(this.gameObject);
	}
	// Update is called once per frame
	void Update()
    {
        transform.Translate(Vector2.up * travelSpeed);
    }
}
