using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    [HideInInspector] public static MatchManager _;

    /*
     P1 win counter
     P2 win counter

     
     
     */

    public int count = 0;
    
    void Awake()
    {

        if (_ != null && _ != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            _ = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Battle")
		{
        //next round
        print(++count);

		}
        else
		{
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(this.gameObject);
		}
	}

}
