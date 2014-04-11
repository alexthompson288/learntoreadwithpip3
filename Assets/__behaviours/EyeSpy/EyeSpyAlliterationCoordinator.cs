using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class EyeSpyAlliterationCoordinator : Singleton<EyeSpyAlliterationCoordinator>
{
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private GameObject m_answerPrefab;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	CharacterPopper m_characterPopper;
	[SerializeField]
	UICamera m_camera;

	List<DataRow> m_wordPool = new List<DataRow>();

	DataRow m_targetPhonemeData;

	Dictionary<string, AudioClip> m_phonemeAudio = new Dictionary<string, AudioClip>();

	int m_score;
	int m_targetScore;

	bool m_canClick = false;

	IEnumerator Start () 
	{
		m_camera.allowMultiTouch = false;

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		//int sectionId = SessionManager.Instance.GetCurrentSectionId();
		int sectionId = 414;
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
		m_wordPool.AddRange(dt.Rows);

		//m_wordPool.AddRange(DataHelpers.GetWords());

		//Debug.Log("pre m_wordPool.Count: " + m_wordPool.Count);

		m_targetScore = m_locators.Length;

		while(m_wordPool.Count > m_targetScore)
		{
			m_wordPool.RemoveAt(Random.Range(0, m_wordPool.Count));
		}

		//Debug.Log("post m_wordPool.Count: " + m_wordPool.Count);

		for(int i = 0; i < m_wordPool.Count; ++i)
		{
			//Debug.Log("word: " + m_wordPool[i]["word"].ToString());

			GameObject newAnswer = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_answerPrefab, m_locators[i]);

			Vector3 fromPos = new Vector3(newAnswer.transform.position.x + Random.Range(-5f, 5f), newAnswer.transform.position.x + Random.Range(-3f, 3f), newAnswer.transform.position.z);
			iTween.MoveFrom(newAnswer, fromPos, Random.Range(0.5f, 2f));

			string[] phonemeIds = m_wordPool[i]["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');

			AudioClip phonemeAudio = null;

			dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + phonemeIds[0] + "'");
			if(dt.Rows.Count > 0)
			{
				phonemeAudio = AudioBankManager.Instance.GetAudioClip(dt.Rows[0]["grapheme"].ToString());
				m_phonemeAudio[dt.Rows[0]["phoneme"].ToString()] = phonemeAudio;

				//Debug.Log("phoneme: " + dt.Rows[0]["phoneme"].ToString());
				//Debug.Log("phonemeAudio: " + phonemeAudio);
			}

			newAnswer.GetComponent<EyeSpyWord>().SetUp(m_wordPool[i], phonemeAudio);
			newAnswer.GetComponent<EyeSpyWord>().OnWordClick += OnWordClick;
		}

		//yield return new WaitForSeconds(0.5f);

		yield return new WaitForSeconds(2);

		WingroveAudio.WingroveRoot.Instance.PostEvent("EYE_SPY_SOMETHING");

		yield return new WaitForSeconds(3f);

		StartCoroutine(AskQuestion());
	}

	IEnumerator AskQuestion()
	{
		int wordIndex = Random.Range(0, m_wordPool.Count);
		DataRow wordData = m_wordPool[wordIndex];
		m_wordPool.RemoveAt(wordIndex);

		string[] phonemeIds = wordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[0] + "'");
		if(dt.Rows.Count > 0)
		{
			m_targetPhonemeData = dt.Rows[0];

			PlayPhonemeAudio();
		}

		ChangeableBennyAudio.Instance.SetChangeableInstruction(LoaderHelpers.LoadMnemonic(m_targetPhonemeData));

		m_canClick = true;

		yield break;
	}

	public void OnWordClick(EyeSpyWord wordBehaviour)
	{
		if(m_canClick)
		{
			//Debug.Log("Coord.OnWordClick()");
			DataRow wordData = wordBehaviour.GetWordData();
			//Debug.Log("Clicked: " + wordData["word"].ToString());
			string[] phonemeIds = wordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[0] + "'");
			if(dt.Rows.Count > 0)
			{
				DataRow phonemeData = dt.Rows[0];

				if(phonemeData["id"].ToString() == m_targetPhonemeData["id"].ToString())
				{
					m_canClick = false; // Defensive prevents bug caused by rapidly pressing many options
					//Debug.Log("Correct");
					StartCoroutine(OnCorrect(wordBehaviour));
				}
				else
				{
					//Debug.Log("Incorrect");
					//Debug.Log("phoneme: " + phonemeData["phoneme"].ToString());
					//Debug.Log("phonemeId: " + phonemeData["id"].ToString());
					//Debug.Log("target: " + m_targetPhonemeData["phoneme"].ToString());
					//Debug.Log("targetId: " + m_targetPhonemeData["id"].ToString());
					wordBehaviour.PlayWordAudio();
				}
			}
			else
			{
				//Debug.Log("No phonemes");
			}
		}
	}

	IEnumerator OnCorrect(EyeSpyWord wordBehaviour)
	{
		++m_score;

		wordBehaviour.IncreaseDepth();

		iTween.ScaleTo(wordBehaviour.gameObject, Vector3.one * 1.5f, 0.5f);

		yield return new WaitForSeconds(PlayPhonemeAudio() + 0.25f);

		yield return new WaitForSeconds(wordBehaviour.PlayWordAudio() + 0.2f);

		m_characterPopper.PopCharacter();

		StartCoroutine(wordBehaviour.Off());

		yield return new WaitForSeconds(1.5f);

		if(m_score < m_targetScore && m_wordPool.Count > 0)
		{
			StartCoroutine(AskQuestion());
		}
		else
		{
			yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
			//GameManager.Instance.CompleteGame();
            GameManager.Instance.CompleteGame();
		}
	}

	float PlayPhonemeAudio()
	{
		if(m_phonemeAudio.ContainsKey(m_targetPhonemeData["phoneme"].ToString()))
		{
			m_audioSource.clip = m_phonemeAudio[m_targetPhonemeData["phoneme"].ToString()];
			if(m_audioSource.clip != null)
			{
				m_audioSource.Play();
				return m_audioSource.clip.length;
			}
		}
		else
		{
			Debug.Log("Couldn't find audio for " + m_targetPhonemeData["phoneme"].ToString());
		}

		return 0;
	}
}
