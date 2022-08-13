using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActions : MonoBehaviour
{

    public virtual void Shield ()
	{
        print(this.name);

    }

    public virtual void Attack ()
	{
        print(this.name);
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
