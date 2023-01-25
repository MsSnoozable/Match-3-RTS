using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameJumperTest : MonoBehaviour
{
    public Animator anim;

    // Start is called before the first frame update
    int myAnimCount = 0;
    void Start()
    {
        anim.SetBool("isShield", true);
        StartCoroutine(Repeat());
    }

    int[] framePositions = new int[] { 0, 10, 20, 100, 14 };

    // Update is called once per frame
    IEnumerator Repeat()
    {
        yield return null;

        //setup animation…
        AnimatorClipInfo[] animInfo = anim.GetNextAnimatorClipInfo(0);
        if (animInfo.Length > 0)
        {
            AnimatorClipInfo mainInfo = animInfo[0];
            myAnimCount = Mathf.FloorToInt(mainInfo.clip.frameRate * mainInfo.clip.length);
        }

        for (int i = 0; true; i++)
		{
            int frame = framePositions[i % framePositions.Length];
            float percentageOfAnimation = frame / (float)myAnimCount + 0.001f;

            anim.Play("Shield", 0, percentageOfAnimation);
            
            yield return new WaitForSeconds(0.3f);
		}
    }
}
