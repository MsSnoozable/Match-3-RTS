using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperScripts
{
	static void Swap<T>(T a, T b)
	{
		T temp = a;
		a = b;
		b = temp;
	}
}
