using UnityEngine;
using System.Collections;

public class UnlockLettersBoard : MonoBehaviour {

    [SerializeField]
    private ParticleSystem m_particles;
    
	// Use this for initialization
	IEnumerator Start () 
    {	
        if (m_particles != null)
        {
            m_particles.enableEmission = false;
        }
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        while (true)
        {
            if (SessionInformation.Instance.GetHasEverWonCoin())
            {
                GetComponent<TweenOnOffBehaviour>().On();
                break;
            }
            yield return null;
        }


        if (m_particles != null)
        {
            m_particles.enableEmission = true;
            GetComponent<TweenScale>().enabled = true;
            yield return new WaitForSeconds(10.0f);
            m_particles.enableEmission = false;
            GetComponent<TweenScale>().enabled = false;
        }
        yield return null;
    }
}
