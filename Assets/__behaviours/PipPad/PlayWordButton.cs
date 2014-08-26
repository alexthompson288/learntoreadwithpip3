using UnityEngine;
using System.Collections;

public class PlayWordButton : MonoBehaviour {

    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private SimpleSpriteAnim m_spriteAnim;
    [SerializeField]
    private GameObject m_hideObject;

	AudioClip m_loadedAudio;

    public void SetWordAudio(string audioFilename)
    {
        AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(audioFilename);
        if (loadedAudio == null)
        {
            m_hideObject.SetActive(false);
        }
        else
        {
            m_hideObject.SetActive(true);
            //m_spriteAnim.PlayAnimation("ON");
			m_loadedAudio = loadedAudio;
            collider.enabled = true;
        }
    }

    public void OnDisable()
    {
        m_audioSource.clip = null;
        Resources.UnloadUnusedAssets();
    }

    void OnClick()
    {
        ////D.Log("PlayWordButton.OnClick()");
        PipPadBehaviour.Instance.SayAll(0.0f);
    }


    public void Speak()
    {
        StartCoroutine(PlayWord());
    }
    

    public IEnumerator PlayWord()
    {
        PipPadBehaviour.Instance.HighlightWholeWord();
        //PipPadBehaviour.Instance.ReShowWordImage();
        collider.enabled = false;
       // m_spriteAnim.PlayAnimation("OFF");
		m_audioSource.clip = m_loadedAudio;
        m_audioSource.Play();
        while (m_audioSource.isPlaying)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        collider.enabled = true;
        //m_spriteAnim.PlayAnimation("ON"); 
    }

	public float GetClipLength()
	{
		if(m_audioSource.clip != null)
		{
			return m_audioSource.clip.length;
		}
		else
		{
			return 0;
		}
	}
}
