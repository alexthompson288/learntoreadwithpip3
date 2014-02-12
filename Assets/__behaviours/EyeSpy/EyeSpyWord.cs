using UnityEngine;
using System.Collections;
using Wingrove;

public class EyeSpyWord : MonoBehaviour 
{
	public delegate void WordClick(EyeSpyWord wordBehaviour);
	public event WordClick OnWordClick;

	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private UITexture m_picture;
	[SerializeField]
	private UITexture m_background;
	[SerializeField]
	private SimpleSpriteAnim m_smokeAnim;
	[SerializeField]
	private Texture2D[] m_bgTextures;
	
	AudioClip m_wordAudio;
	AudioClip m_phonemeAudio;

	DataRow m_wordData;
	
	void Start()
	{
		m_smokeAnim.PlayAnimation("OFF");
	}
	
	public void SetUp (DataRow wordData, AudioClip phonemeAudio) 
	{
		m_wordData = wordData;

		Texture2D wordImage = null;
		if ((m_wordData["image"] != null) && (!string.IsNullOrEmpty(m_wordData["image"].ToString())))
		{
			wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + m_wordData["image"].ToString());
		}
		else
		{
			wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + m_wordData["word"].ToString());
		}
		m_picture.mainTexture = wordImage;
		
		m_phonemeAudio = phonemeAudio;
		
		m_wordAudio = LoaderHelpers.LoadAudioForWord(m_wordData["word"].ToString());
		
		collider.enabled = true;

		m_background.mainTexture = m_bgTextures[Random.Range(0, m_bgTextures.Length)];

		//Debug.Log("SetUp: " + m_wordData["word"].ToString());
	}
	
	void OnClick()
	{
		Debug.Log("OnClick: " + m_wordData["word"].ToString());
		if(OnWordClick != null)
		{
			OnWordClick(this);
		}
	}
	
	public IEnumerator Off()
	{
		//WingroveAudio.WingroveRoot.Instance.PostEvent("SMOKE_RANDOM");
		WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
		m_smokeAnim.PlayAnimation("ON");
		yield return new WaitForSeconds(0.2f);
		collider.enabled = false;
		m_picture.enabled = false;
		m_background.enabled = false;
		/*
		yield return new WaitForSeconds(0.3f);
		PlayPhonemeAudio();
		yield return new WaitForSeconds(0.5f);
		PlayWordAudio();
		*/
	}
	
	public float PlayWordAudio()
	{
		if(m_wordAudio != null)
		{
			m_audioSource.clip = m_wordAudio;
			m_audioSource.Play();
			return m_audioSource.clip.length;
		}
		else
		{
			return 0;
		}
	}
	
	public void PlayPhonemeAudio()
	{
		if(m_phonemeAudio != null)
		{
			m_audioSource.clip = m_phonemeAudio;
			m_audioSource.Play();
		}
	}

	public DataRow GetWordData()
	{
		return m_wordData;
	}

	public void IncreaseDepth()
	{
		m_picture.depth += 3;
		m_background.depth += 3;
	}
}
