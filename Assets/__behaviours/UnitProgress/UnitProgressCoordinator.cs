using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class UnitProgressCoordinator : MonoBehaviour 
{
	[SerializeField]
	private CharacterPopper m_characterPopper;
	[SerializeField]
	private UITexture m_background;
	[SerializeField]
	private AudioSource m_sparkleAudioSource;
	[SerializeField]
	private AudioSource m_ambientAudioSource;
	[SerializeField]
	private AudioClip m_forestAudio;
	[SerializeField]
	private AudioClip m_underwaterAudio;
	[SerializeField]
	private AudioClip m_alienAudio;
	[SerializeField]
	private AudioClip m_farmAudio;
	[SerializeField]
	private AudioClip m_castleAudio;
	[SerializeField]
	private AudioClip m_schoolAudio;
	[SerializeField]
	private GameObject m_letterPrefab;
	[SerializeField]
	private Transform m_botLeft;
	[SerializeField]
	private Transform m_topRight;


	// Use this for initialization
	IEnumerator Start () 
	{
		m_sparkleAudioSource.loop = false;
		m_ambientAudioSource.loop = true;

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        if(VoyageInfo.Instance.hasBookmark)
        {
            int sectionId = VoyageInfo.Instance.currentSectionId;

    		List<DataRow> sentences = DataHelpers.GetSectionSentences(sectionId);

    		if(sentences.Count > 0)
    		{
    			foreach(DataRow sentence in sentences)
    			{
    				if(sentence["is_target_sentence"] != null && sentence["is_target_sentence"].ToString() == "t")
    				{
    					////D.Log("target: " + sentence["text"].ToString());
    					Texture2D tex = Resources.Load<Texture2D>("unitProgress_backgrounds/" + sentence["text"].ToString());

    					////D.Log("tex: " + tex);

    					if(tex != null)
    					{
    						m_background.mainTexture = tex;
    					}
    				}
    				else if(sentence["is_dummy_sentence"] != null && sentence["is_dummy_sentence"].ToString() == "t")
    				{
    					////D.Log("dummy: " + sentence["text"].ToString());
    					AudioClip clip = Resources.Load<AudioClip>("unitProgress_audio/" + sentence["text"].ToString());

    					////D.Log("clip: " + clip);

    					if(clip != null)
    					{
    						m_sparkleAudioSource.clip = clip;
    						m_sparkleAudioSource.Play();
    					}
    				}
    			}

    			string findAudioSentence = sentences[0]["text"].ToString();
    			AudioClip ambientAudio = null;

    			if(findAudioSentence.Contains("forest"))
    			{
    				ambientAudio = m_forestAudio;
    			}
    			else if(findAudioSentence.Contains("underwater"))
    			{
    				ambientAudio = m_underwaterAudio;
    			}
    			else if(findAudioSentence.Contains("alien"))
    			{
    				ambientAudio = m_alienAudio;
    			}
    			else if(findAudioSentence.Contains("farm"))
    			{
    				ambientAudio = m_farmAudio;
    			}
    			else if(findAudioSentence.Contains("castle"))
    			{
    				ambientAudio = m_castleAudio;
    			}
    			else if(findAudioSentence.Contains("school"))
    			{
    				ambientAudio = m_schoolAudio;
    			}

    			if(ambientAudio != null)
    			{
    				m_ambientAudioSource.clip = ambientAudio;
    				m_ambientAudioSource.Play();
    			}
            }

            List<DataRow> phonemes = DataHelpers.GetSectionLetters(sectionId);
            
            foreach(DataRow phoneme in phonemes)
            {
                GameObject newMnemonic = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterPrefab, m_botLeft);
                newMnemonic.transform.position = new Vector3(Random.Range(m_botLeft.position.x, m_topRight.position.x), Random.Range(m_botLeft.position.y, m_topRight.position.y), 0);
                newMnemonic.name = phoneme["phoneme"].ToString();
                newMnemonic.GetComponent<SplineLetter>().SetUp(phoneme, true);
                
                GameObject newLetter = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterPrefab, m_botLeft);
                newLetter.transform.position = new Vector3(Random.Range(m_botLeft.position.x, m_topRight.position.x), Random.Range(m_botLeft.position.y, m_topRight.position.y), 0);
                newLetter.name = phoneme["phoneme"].ToString();
                newLetter.GetComponent<SplineLetter>().SetUp(phoneme, false);
            }
            
            m_characterPopper.PopCharacter();
            WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
            
            while(true)
            {
                yield return new WaitForSeconds(Random.Range(2.5f, 4f));
                m_characterPopper.PopCharacter();
                WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
            }
		}
	}
}
