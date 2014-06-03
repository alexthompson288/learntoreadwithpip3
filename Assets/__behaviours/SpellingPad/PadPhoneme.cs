using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PadPhoneme : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_singleButton;
    [SerializeField]
    private GameObject m_doubleButton;
    [SerializeField]
    private GameObject m_tripleButton;
    [SerializeField]
    private AtoBPressButton[] m_pressButtons;
    [SerializeField]
    private AudioSource m_audioSource;

    float m_width;
    private GameObject m_linkedObject = null;

    bool m_isActive = false;
    private AudioClip m_audioClip;
    
    private string m_phoneme;

    public void Activate()
    {
        if (!m_isActive)
        {
            if (m_linkedObject != null)
            {
                m_linkedObject.GetComponent<SpellingPadPhoneme>().ActivateFinal();
            }
            ActivateFinal();
            m_audioSource.Play();
        }
    }

    public void ActivateFinal()
    {
        m_isActive = true;
        StartCoroutine(HighlightAndPlay());
    }

    // TODO: Highlight the PadLetters
    IEnumerator HighlightAndPlay()
    {
        foreach (AtoBPressButton abs in m_pressButtons)
        {
            abs.ManualActivate(true);
        }

        
        float t = 0;
        while(t < 0.5f)
        {
            t += Time.deltaTime;
  
            m_isActive = true;
            foreach (AtoBPressButton abs in m_pressButtons)
            {
                abs.ManualActivate(true);
            }
            yield return null;
        }
        
        foreach (AtoBPressButton abs in m_pressButtons)
        {
            abs.ManualActivate(false);
        }

        m_isActive = false;
    }

    public void PlayAudio()
    {
        if(m_audioSource.clip != null)
        {
            m_audioSource.Play();
        }
    }

    public string GetPhoneme()
    {
        return m_phoneme;
    }
}
