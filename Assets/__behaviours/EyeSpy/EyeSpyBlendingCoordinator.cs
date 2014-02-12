using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class EyeSpyBlendingCoordinator : Singleton<EyeSpyBlendingCoordinator>
{
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private GameObject m_answerPrefab;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private ClickEvent m_bennyAudio;
	[SerializeField]
	private CharacterPopper m_characterPopper;
	[SerializeField]
	private UICamera m_camera;
	
	List<DataRow> m_wordPool = new List<DataRow>();
	
	DataRow m_targetWordData;

	Dictionary<DataRow, AudioClip[]> m_phonemeAudio = new Dictionary<DataRow, AudioClip[]>();

	int m_score;
	int m_targetScore;

	bool m_canClick = false;

	void Awake()
	{
		m_bennyAudio.OnSingleClick += OnBennyClick;

		m_camera.allowMultiTouch = false;
	}

	void OnBennyClick(ClickEvent behaviour)
	{
		StartCoroutine(PlayPhonemeAudio());
	}
	
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		int sectionId = JourneyInformation.Instance.GetCurrentSectionId();
		//int sectionId = 1392;
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
		m_wordPool.AddRange(dt.Rows);
		
		m_targetScore = m_locators.Length;
		
		while(m_wordPool.Count > m_targetScore)
		{
			m_wordPool.RemoveAt(Random.Range(0, m_wordPool.Count));
		}
		
		//Debug.Log("m_wordPool.Count: " + m_wordPool.Count);
		
		for(int i = 0; i < m_wordPool.Count; ++i)
		{
			//Debug.Log("word: " + m_wordPool[i]["word"].ToString());
			
			GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_answerPrefab, m_locators[i]);

			Vector3 fromPos = new Vector3(newAnswer.transform.position.x + Random.Range(-5f, 5f), newAnswer.transform.position.x + Random.Range(-3f, 3f), newAnswer.transform.position.z);
			iTween.MoveFrom(newAnswer, fromPos, Random.Range(0.5f, 2f));
			
			string[] phonemeIds = m_wordPool[i]["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
			
			List<AudioClip> phonemeAudioList = new List<AudioClip>();

			foreach(string id in phonemeIds)
			{
				dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + id + "'");
				if(dt.Rows.Count > 0)
				{
					phonemeAudioList.Add(AudioBankManager.Instance.GetAudioClip(dt.Rows[0]["grapheme"].ToString()));
					
					//Debug.Log("phoneme: " + dt.Rows[0]["phoneme"].ToString());
				}
			}

			m_phonemeAudio[m_wordPool[i]] = phonemeAudioList.ToArray();

			newAnswer.GetComponent<EyeSpyWord>().SetUp(m_wordPool[i], null);
			newAnswer.GetComponent<EyeSpyWord>().OnWordClick += OnWordClick;
		}
		
		yield return new WaitForSeconds(1f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("EYE_SPY_A");
		
		yield return new WaitForSeconds(2f);
		

		
		StartCoroutine(AskQuestion());
	}
	
	IEnumerator AskQuestion()
	{
		int wordIndex = Random.Range(0, m_wordPool.Count);
		m_targetWordData = m_wordPool[wordIndex];
		Debug.Log("targetWord: " + m_targetWordData["word"].ToString());
		StartCoroutine(PlayPhonemeAudio());
		m_wordPool.RemoveAt(wordIndex);

		m_canClick = true;

		yield break;
	}
	
	public void OnWordClick(EyeSpyWord wordBehaviour)
	{
		if(m_canClick)
		{
			Debug.Log("Coord.OnWordClick()");
			DataRow wordData = wordBehaviour.GetWordData();
			Debug.Log("Clicked: " + wordData["word"].ToString());

			if(wordData["id"].ToString() == m_targetWordData["id"].ToString())
			{
				m_canClick = false;
				Debug.Log("Correct");
				StopAllCoroutines();
				StartCoroutine(OnCorrect(wordBehaviour));
			}
			else
			{
				Debug.Log("Incorrect");
				Debug.Log("phoneme: " + wordData["word"].ToString());
				Debug.Log("phonemeId: " + wordData["id"].ToString());
				Debug.Log("target: " + m_targetWordData["word"].ToString());
				Debug.Log("targetId: " + m_targetWordData["id"].ToString());
				wordBehaviour.PlayWordAudio();
			}
		}
	}
	
	IEnumerator OnCorrect(EyeSpyWord wordBehaviour)
	{
		++m_score;
		
		wordBehaviour.IncreaseDepth();
		
		iTween.ScaleTo(wordBehaviour.gameObject, Vector3.one * 1.5f, 0.5f);

		StartCoroutine(PlayPhonemeAudio());

		yield return new WaitForSeconds(FindPhonemeAudioLength() + 0.25f);
		
		yield return new WaitForSeconds(wordBehaviour.PlayWordAudio() + 0.2f);
		
		StartCoroutine(wordBehaviour.Off());
		m_characterPopper.PopCharacter();
		
		yield return new WaitForSeconds(1.5f);
		
		if(m_score < m_targetScore)
		{
			StartCoroutine(AskQuestion());
		}
		else
		{
			yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
			JourneyInformation.Instance.OnGameFinish();
		}
	}

	float FindPhonemeAudioLength()
	{
		float length = 0;

		AudioClip[] phonemeAudio = m_phonemeAudio[m_targetWordData];

		foreach(AudioClip clip in phonemeAudio)
		{
			if(clip != null)
			{
				length += clip.length;
				length += 0.2f;
			}
		}

		return length;
	}

	IEnumerator PlayPhonemeAudio()
	{
		if(m_targetWordData != null && m_phonemeAudio.ContainsKey(m_targetWordData))
		{
			AudioClip[] phonemeAudio = m_phonemeAudio[m_targetWordData];

			foreach(AudioClip clip in phonemeAudio)
			{
				m_audioSource.clip = clip;
				if(m_audioSource.clip != null)
				{
					m_audioSource.Play();
					yield return new WaitForSeconds(m_audioSource.clip.length + 0.2f);
				}
			}
		}
		
		yield break;
	}

	float PlayWordAudio()
	{
		Debug.Log("m_targetWordData: " + m_targetWordData);

		m_audioSource.clip = LoaderHelpers.LoadAudioForWord(m_targetWordData["word"].ToString());

		if(m_audioSource.clip != null)
		{
			m_audioSource.Play();
			return m_audioSource.clip.length;
		}
		else
		{
			return 0;
		}
	}
}
