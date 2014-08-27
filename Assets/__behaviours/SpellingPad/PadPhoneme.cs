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

    PadPhoneme m_linkedObject = null;

    float m_width;

    bool m_isActive = false;
    AudioClip m_audioClip;
    
    string m_phoneme;

    int m_positionIndex;

    List<PadLetter> m_padLetters = new List<PadLetter>();

    public void SetUp(SpellingPadBehaviour.PhonemeBuildInfo pbi)
    {
        Resources.UnloadUnusedAssets();

        if (pbi.m_audioFilename != null)
        {
            m_audioClip = AudioBankManager.Instance.GetAudioClip(pbi.m_audioFilename);
            m_audioSource.clip = m_audioClip;
        }
        else
        {
            m_audioClip = null;
            m_audioSource.clip = null;
        }
        
        if (pbi.m_displayString.Length == 1)
        {
            m_singleButton.SetActive(true);
            m_width = 100;
        }
        else if (pbi.m_displayString.Length == 2)
        {
            m_doubleButton.SetActive(true);
            m_width = 128;
        }
        else
        {
            m_tripleButton.SetActive(true);
            m_width = 170;
        }
        
        m_phoneme = pbi.m_fullPhoneme;
        m_positionIndex = pbi.m_positionIndex;
        
        ////////D.Log(System.String.Format("Phoneme: {0} - {1}", m_phoneme, m_positionIndex));
    }

    public void AddPadLetter(PadLetter padLetter)
    {
        m_padLetters.Add(padLetter);
    }

    public void Activate()
    {
        if (!m_isActive)
        {
            if (m_linkedObject != null)
            {
                m_linkedObject.ActivateFinal();
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

    public float GetWidth()
    {
        return m_width;
    }

    public void Link(PadPhoneme other)
    {
        m_linkedObject = other;
    }
}
