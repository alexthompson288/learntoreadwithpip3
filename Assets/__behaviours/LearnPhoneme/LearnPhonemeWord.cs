using UnityEngine;
using System.Collections;
using Wingrove;

public class LearnPhonemeWord : MonoBehaviour 
{
	[SerializeField]
	private SimpleSpriteAnim m_smokeAnim;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private UITexture m_picture;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private string[] m_spriteNames;
	
	bool m_showingPicture = false;

	AudioClip m_wordAudio;
	AudioClip m_phonemeAudio;

	bool m_pressed = false;

	public bool GetPressed()
	{
		return m_pressed;
	}
	

	void Start()
	{
		m_smokeAnim.PlayAnimation("OFF");
		
		if(m_spriteNames.Length > 0)
		{
			string spriteName = m_spriteNames[Random.Range(0, m_spriteNames.Length)];
			m_background.spriteName = spriteName;
		}
	}
	
	public void SetUp (DataRow wordData, AudioClip phonemeAudio) 
	{
		Texture2D wordImage = null;
		if ((wordData["image"] != null) && (!string.IsNullOrEmpty(wordData["image"].ToString())))
		{
			wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + wordData["image"].ToString());
		}
		else
		{
			wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + wordData["word"].ToString());
		}
		m_picture.mainTexture = wordImage;
		m_picture.enabled = false;

		m_phonemeAudio = phonemeAudio;

		m_wordAudio = LoaderHelpers.LoadAudioForWord(wordData["word"].ToString());

		collider.enabled = true;
	}

	void OnClick()
	{
		m_pressed = true;

		if(m_showingPicture)
		{
			PlayWordAudio();
		}
		else
		{
			LearnPhonemeWord[] allWords = Object.FindObjectsOfType<LearnPhonemeWord>();
			foreach(LearnPhonemeWord behaviour in allWords)
			{
				behaviour.StopAllCoroutines();
			}
			StartCoroutine(OnClickCo());
		}
	}

	IEnumerator OnClickCo()
	{
		GetComponent<ThrobGUIElement>().Off();
		//WingroveAudio.WingroveRoot.Instance.PostEvent("SMOKE_1");
		WingroveAudio.WingroveRoot.Instance.PostEvent("SMOKE_RANDOM");
		m_smokeAnim.PlayAnimation("ON");
		yield return new WaitForSeconds(0.4f);
		m_picture.enabled = true;
		m_showingPicture = true;
		yield return new WaitForSeconds(0.3f);
		PlayPhonemeAudio();
		yield return new WaitForSeconds(0.5f);
		PlayPhonemeAudio();
		yield return new WaitForSeconds(0.5f);
		PlayWordAudio();

		float delay = 0;
		if(m_wordAudio != null)
		{
			delay = m_wordAudio.length;
		}
		yield return new WaitForSeconds(delay + 0.2f);

		LearnPhonemeCoordinator.Instance.CheckForEnd();
	}

	void PlayWordAudio()
	{
		if(m_wordAudio != null)
		{
			m_audioSource.clip = m_wordAudio;
			m_audioSource.Play();
		}
	}

	void PlayPhonemeAudio()
	{
		if(m_phonemeAudio != null)
		{
			m_audioSource.clip = m_phonemeAudio;
			m_audioSource.Play();
		}
	}
}
