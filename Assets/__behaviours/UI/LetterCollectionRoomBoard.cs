using UnityEngine;
using System.Collections;

public class LetterCollectionRoomBoard : MonoBehaviour {

    [SerializeField]
    private ParticleSystem m_particles;

    int m_numberLettersUnlocked = 0;
    
	// Use this for initialization
	IEnumerator Start () 
    {
        m_numberLettersUnlocked = SessionInformation.Instance.GetNumberUnlockedLetters();
        if (m_particles != null)
        {
            m_particles.enableEmission = false;
        }
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        while (true)
        {
            if (SessionInformation.Instance.GetNumberUnlockedLetters() > 0)
            {
                GetComponent<TweenOnOffBehaviour>().On();
                break;
            }
            yield return null;
        }

        while (true)
        {
            if (SessionInformation.Instance.GetNumberUnlockedLetters() > m_numberLettersUnlocked)
            {
                if (m_particles != null)
                {
                    m_particles.enableEmission = true;
                    GetComponent<TweenScale>().enabled = true;
                    m_numberLettersUnlocked = SessionInformation.Instance.GetNumberUnlockedLetters();
                    yield return new WaitForSeconds(10.0f);
                    m_particles.enableEmission = false;
                    GetComponent<TweenScale>().enabled = false;
                }
            }
            yield return null;
        }
	
	}


}
