using UnityEngine;
using System.Collections;

public class PlayBlinkRandomly : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {

        while (true)
        {
            GetComponent<SimpleSpriteAnim>().PlayAnimation("OFF");
            yield return new WaitForSeconds(Random.Range(2.0f, 6.0f));
            GetComponent<SimpleSpriteAnim>().PlayAnimation("ON");
            yield return new WaitForSeconds(0.9f);
        }
	}
	
}
