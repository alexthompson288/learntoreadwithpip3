using UnityEngine;
using System.Collections;

public class CollectionRoomBoard : MonoBehaviour {

    [SerializeField]
    private ParticleSystem m_particles;

    int m_numberWordsUnlocked = 0;
    
	// Use this for initialization
	IEnumerator Start () 
    {
        m_numberWordsUnlocked = SessionInformation.Instance.GetNumberUnlockedWords();
        if (m_particles != null)
        {
            m_particles.enableEmission = false;
        }
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        while (true)
        {
            if (SessionInformation.Instance.GetNumberUnlockedWords() > 0)
            {
                GetComponent<TweenOnOffBehaviour>().On();
                break;
            }
            yield return null;
        }

        while (true)
        {
            if (SessionInformation.Instance.GetNumberUnlockedWords() > m_numberWordsUnlocked)
            {
                if (m_particles != null)
                {
                    m_particles.enableEmission = true;
                    GetComponent<TweenScale>().enabled = true;
                    m_numberWordsUnlocked = SessionInformation.Instance.GetNumberUnlockedWords();
                    yield return new WaitForSeconds(10.0f);
                    m_particles.enableEmission = false;
                    GetComponent<TweenScale>().enabled = false;
                }
            }
            yield return null;
        }
	
	}


}
