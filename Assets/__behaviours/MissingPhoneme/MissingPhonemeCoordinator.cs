using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using WingroveAudio;

public class MissingPhonemeCoordinator : MonoBehaviour 
{
	[SerializeField]
	private SimpleSpriteAnim[] m_flowers;
	[SerializeField]
	private int m_numAnswers = 3;
	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private GameObject m_draggablePrefab;
	[SerializeField]
	private Transform m_spawnedDraggableParent;
	[SerializeField]
	private Transform m_draggableSpawnHigh;
	[SerializeField]
	private Transform m_draggableSpawnLow;
	[SerializeField]
	private ProgressScoreBar m_scoreBar;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private GameObject m_letterButtonPrefab;
	[SerializeField]
	private Transform m_letterButtonStartPos;
	[SerializeField]
	private Transform m_letterButtonEndPos;
	
	int m_score;
	
	List<DataRow> m_wordPool = new List<DataRow>();
	List<DataRow> m_targetLetterPool = new List<DataRow>();
	List<DataRow> m_dummyLetterPool = new List<DataRow>();

	DataRow m_currentWordData;
	
	List<DraggableLabel> m_draggables = new List<DraggableLabel>();
	
	int m_wrongAnswers = 0;

	bool m_useLinkingIndices = false;

	// Use this for initialization
	IEnumerator Start () 
	{
		// always pip, always winner
		SessionInformation.Instance.SetPlayerIndex(0, 3);
		SessionInformation.Instance.SetWinner(0);
		
		m_scoreBar.SetStarsTarget(m_targetScore);
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_wordPool.AddRange(GameDataBridge.Instance.GetWords());
		
		/*
		int sectionId = 797;
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id = words.id WHERE section_id=" + sectionId);
		m_wordPool = dt.Rows;
		*/

		/*
		for(int i = m_wordPool.Count - 1; i > -1; --i)
		{
			if(m_wordPool[i]["is_target_word"] != null && m_wordPool[i]["is_target_word"].ToString() != "t")
			{
				m_wordPool.RemoveAt(i);
			}
		}
		*/

		List<DataRow> letterPool = new List<DataRow>();

		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Voyage && m_wordPool[0]["linking_index"] != null)
		{
			m_useLinkingIndices = true;
		}

		letterPool = GameDataBridge.Instance.GetLetters();

		foreach(DataRow letter in letterPool)
		{
			if(letter["is_dummy_phoneme"] == null)
			{
				Debug.Log(letter["phoneme"].ToString() + " has no dummy property");
			}
			if(letter["is_dummy_phoneme"] != null && letter["is_dummy_phoneme"].ToString() == "t")
			{
				m_dummyLetterPool.Add(letter);
			}
			else
			{
				m_targetLetterPool.Add(letter);
			}
		}

		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Voyage)
		{
			DataRow section = JourneyInformation.Instance.GetCurrentSection();

			if(section["sectiontype"].ToString() == "Learn")
			{
				GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, m_letterButtonStartPos);
				
				LetterButton letterButton = newButton.GetComponent<LetterButton>() as LetterButton;
				letterButton.SetUp(m_targetLetterPool[0]);
				letterButton.SetMethods( letterButton.PlayPhonemeAudio, letterButton.PlayMnemonicAudio );
				
				newButton.transform.localScale = Vector3.one * 1.5f;
				iTween.ScaleFrom(newButton, Vector3.zero, 0.5f);
				
				yield return new WaitForSeconds(0.5f);
				
				letterButton.PlayMnemonicAudio();
				
				yield return new WaitForSeconds(letterButton.GetMnemonicAudioLength() + 0.2f);
				
				float smallTweenDuration = 0.5f;
				iTween.ScaleTo( newButton, Vector3.one * 0.2f, smallTweenDuration );
				iTween.MoveTo( newButton, m_letterButtonEndPos.position, smallTweenDuration );
				
				yield return new WaitForSeconds(smallTweenDuration);
			}
		}
		
		Debug.Log("target");
		foreach(DataRow targetLetter in m_targetLetterPool)
		{
			Debug.Log(targetLetter["phoneme"].ToString());
		}

		Debug.Log("dummy");
		foreach(DataRow dummyLetter in m_dummyLetterPool)
		{
			Debug.Log(dummyLetter["phoneme"].ToString());
		}

		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Voyage && !m_useLinkingIndices)
		{
			for(int i = m_wordPool.Count - 1; i > -1; --i)
			{
				List<DataRow> orderedPhonemes = GameDataBridge.Instance.GetOrderedPhonemes(m_wordPool[i]);
				if(!orderedPhonemes.Contains(m_targetLetterPool[0]))
				{
					m_wordPool.RemoveAt(i);
				}
			}
		}
		
		yield return new WaitForSeconds(0.5f);
		
		if(m_targetLetterPool.Count > 0)
		{
			SpawnQuestion();
		}
		else
		{
			EndGame();
		}
	}

	public void SpeakCurrentWord()
	{
		AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWordData["word"].ToString());
		GetComponent<AudioSource>().clip = loadedAudio;
		GetComponent<AudioSource>().Play();
	}

	void SpawnQuestion ()
	{
		m_currentWordData = m_wordPool[Random.Range(0, m_wordPool.Count)];
		string currentWord = m_currentWordData["word"].ToString();

		//string[] phonemeIds = m_currentWordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
		
		Debug.Log("currentWord: " + currentWord);

		HashSet<string> answerPhonemes = new HashSet<string>();
		string targetPhoneme = null;

		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Voyage)
		{
			if(m_useLinkingIndices)
			{
				Debug.Log("Linking Index: " + m_currentWordData["linking_index"].ToString());

				foreach(DataRow letter in m_targetLetterPool)
				{
					if(letter["linking_index"].ToString() == m_currentWordData["linking_index"].ToString())
					{
						targetPhoneme = letter["phoneme"].ToString();
						answerPhonemes.Add(targetPhoneme);
						break;
					}
				}
				
				List<DataRow> dummyLetters = new List<DataRow>();
				foreach(DataRow letter in m_dummyLetterPool)
				{
					if(letter["linking_index"].ToString() == m_currentWordData["linking_index"].ToString())
					{
						answerPhonemes.Add(letter["phoneme"].ToString());
					}
				}
			}
			else
			{
				targetPhoneme = m_targetLetterPool[0]["phoneme"].ToString();
				answerPhonemes.Add(m_targetLetterPool[0]["phoneme"].ToString());
				foreach(DataRow dummy in m_dummyLetterPool)
				{
					answerPhonemes.Add(dummy["phoneme"].ToString());
				}
			}
		}
		else
		{
			string[] phonemeIds = m_currentWordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
			List<string> phonemes = new List<string>();
			for(int i = 0; i < phonemeIds.Length; ++i)
			{
				phonemes.Add(GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'").Rows[0]["phoneme"].ToString());
			}
			
			int targetPhonemeIndex = Random.Range(0, phonemes.Count);
			targetPhoneme = phonemes[targetPhonemeIndex];

			answerPhonemes.Add(targetPhoneme);
			
			while(answerPhonemes.Count < m_numAnswers)
			{
				answerPhonemes.Add(m_targetLetterPool[Random.Range(0, m_targetLetterPool.Count)]["phoneme"].ToString());
			}
		}
		
		SpeakCurrentWord();
			
		SpellingPadBehaviour.Instance.DisplayNewWord(currentWord);
			
		foreach(string answerPhoneme in answerPhonemes)
		{
			DraggableLabel newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, m_spawnedDraggableParent).GetComponent<DraggableLabel>() as DraggableLabel;
			newDraggable.SetUp(answerPhoneme.ToString(), null, true);
			m_draggables.Add(newDraggable);
				
			newDraggable.OnRelease += OnRelease;
				
			newDraggable.transform.localPosition = new Vector3(Random.Range(m_draggableSpawnLow.localPosition.x, m_draggableSpawnHigh.localPosition.x),
				                                               Random.Range(m_draggableSpawnLow.localPosition.y, m_draggableSpawnHigh.localPosition.y),
				                                               newDraggable.transform.localPosition.z);
		}
			
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			
		Debug.Log("targetPhoneme: " + targetPhoneme);

		SpellingPadBehaviour.Instance.MakeAllVisibleExceptTarget(targetPhoneme);
		SpellingPadBehaviour.Instance.DisableAllCollidersExceptTarget(targetPhoneme);

		if(answerPhonemes.Count == 0 || targetPhoneme == null)
		{
			DestroyDraggables();
			SpawnQuestion();
		}
	}



	/*
	IEnumerator SpawnQuestion ()
	{
		m_currentWordData = m_wordPool[Random.Range(0, m_wordPool.Count)];
		string currentWord = m_currentWordData["word"].ToString();
		
		Debug.Log("currentWord: " + currentWord);

		if(GameDataBridge.Instance.GetContentType() == GameDataBridge.ContentType.Voyage)
		{
			DataRow targetLetter = null;

			foreach(DataRow letter in m_targetLetterPool)
			{
				if(letter["linking_index"].ToString() == currentWord["linking_index"].ToString())
				{
					targetLetter = letter;
					break;
				}
			}

			List<DataRow> dummyLetters = new List<DataRow>();
			foreach(DataRow letter in m_dummyLetterPool)
			{
				if(letter["linking_index"].ToString() == currentWord["linking_index"].ToString())
				{
					dummyLetters.AddRange(letter);
				}
			}

			string[] phonemeIds = m_currentWordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
			List<string> phonemes = new List<string>();
			for(int i = 0; i < phonemeIds.Length; ++i)
			{
				phonemes.Add(GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'").Rows[0]["phoneme"].ToString());
			}

			for(int i = phonemes.Count - 1; i > -1; --i)
			{

			}
		}
		else
		{
			string[] phonemeIds = m_currentWordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
			List<string> phonemes = new List<string>();
			for(int i = 0; i < phonemeIds.Length; ++i)
			{
				phonemes.Add(GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'").Rows[0]["phoneme"].ToString());
			}
			
			int targetPhonemeIndex = Random.Range(0, phonemes.Count);
			string targetPhoneme = phonemes[targetPhonemeIndex];
			phonemes.RemoveAt(targetPhonemeIndex);
			
			SpeakCurrentWord();
			
			SpellingPadBehaviour.Instance.DisplayNewWord(currentWord);
			
			HashSet<string> answerPhonemes = new HashSet<string>();
			answerPhonemes.Add(targetPhoneme);
			
			while(answerPhonemes.Count < m_numAnswers)
			{
				answerPhonemes.Add(m_targetLetterPool[Random.Range(0, m_targetLetterPool.Count)]["phoneme"].ToString());
			}
			
			foreach(string answerPhoneme in answerPhonemes)
			{
				DraggableLabel newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, m_spawnedDraggableParent).GetComponent<DraggableLabel>() as DraggableLabel;
				newDraggable.SetUp(answerPhoneme.ToString());
				m_draggables.Add(newDraggable);
				
				newDraggable.OnRelease += OnRelease;
				
				newDraggable.transform.localPosition = new Vector3(Random.Range(m_draggableSpawnLow.localPosition.x, m_draggableSpawnHigh.localPosition.x),
				                                                   Random.Range(m_draggableSpawnLow.localPosition.y, m_draggableSpawnHigh.localPosition.y),
				                                                   newDraggable.transform.localPosition.z);
			}
			
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			
			SpellingPadBehaviour.Instance.MakeAllVisibleExceptTarget(targetPhoneme);
			SpellingPadBehaviour.Instance.DisableAllCollidersExceptTarget(targetPhoneme);
		}

		
		yield return null;
	}
	*/
	
	void EndGame ()
	{
		PipHelpers.SetDefaultPlayerVar();
		PipHelpers.OnGameFinish();
	}

	void DestroyDraggables ()
	{
		foreach(DraggableLabel draggable in m_draggables)
		{
			draggable.Off();
		}
		m_draggables.Clear();
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEARS");
	}
	
	IEnumerator OnQuestionEnd(DraggableLabel currentDraggable)
	{
		SpellingPadBehaviour.Instance.MakeAllLabelsVisible();

		float alphaTweenDuration = SpellingPadBehaviour.Instance.TweenAllBackgroundsAlpha(0);
		TweenAlpha.Begin(currentDraggable.gameObject, alphaTweenDuration, 0);

		yield return new WaitForSeconds(alphaTweenDuration);

		SpeakCurrentWord();

		yield return new WaitForSeconds(2f);

		yield return new WaitForSeconds(1f);
		
		DestroyDraggables();
		
		yield return new WaitForSeconds(0.5f);
		
		if(m_score < m_targetScore)
		{
			//StartCoroutine(SpawnQuestion());
			SpawnQuestion();
		}
		else
		{
			EndGame();
		}
	}
	
	void OnRelease(DraggableLabel currentDraggable)
	{
		SpellingPadPhoneme spellingPadPhoneme = SpellingPadBehaviour.Instance.CheckLetters(currentDraggable.GetText(), currentDraggable.collider);
		
		if(spellingPadPhoneme != null)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			currentDraggable.TweenToPos(spellingPadPhoneme.transform.position);
			currentDraggable.ChangeToOnTexture();
			
			if(m_score < m_flowers.Length)
			{
				m_flowers[m_score].PlayAnimation("ON");
			}

			++m_score;
			m_scoreBar.SetStarsCompleted(m_score);
			m_scoreBar.SetScore(m_score);
				
			StartCoroutine(OnQuestionEnd(currentDraggable));
		}
		else
		{
			currentDraggable.TweenToStartPos();
			
			++m_wrongAnswers;
			
			SpellingPadBehaviour.Instance.SayAll();
		}
	}
}

