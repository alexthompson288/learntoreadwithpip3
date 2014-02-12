using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using WingroveAudio;

public class PhonemeSwapCoordinator : MonoBehaviour {

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
	
	int m_score;
	
	List<DataRow> m_wordsPool = new List<DataRow>();
	DataRow m_currentWordData = null;

	string m_currentWord;
	
	List<DraggableLabel> m_draggables = new List<DraggableLabel>();

	int m_wrongAnswers = 0;

	// Use this for initialization
	IEnumerator Start () 
	{
		// always pip, always winner
		SessionInformation.Instance.SetPlayerIndex(0, 3);
		SessionInformation.Instance.SetWinner(0);
		
		m_scoreBar.SetStarsTarget(m_targetScore);

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		int[] sectionIds =((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
		int difficulty = SessionInformation.Instance.GetDifficulty();
		m_wordsPool = GameDataBridge.Instance.GetSectionWords(sectionIds[difficulty]).Rows;

		yield return new WaitForSeconds(0.5f);
		
		//StartCoroutine(SpawnQuestion());
		//SpawnFirstQuestion();
	}

	public void SpeakCurrentWord()
	{
		AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWordData["word"].ToString());
		GetComponent<AudioSource>().clip = loadedAudio;
		GetComponent<AudioSource>().Play();
	}

	/*
	void SpawnFirstQuestion()
	{
		m_currentWordData = m_wordsPool[Random.Range(0, m_wordsPool.Count)];
		m_currentWord = m_currentWordData["word"].ToString();
		
		string[] phonemeIds = m_currentWordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
		string[] phonemes = new string[phonemeIds.Length];
		for(int i = 0; i < phonemeIds.Length; ++i)
		{
			phonemes[i] = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'").Rows[0]["phoneme"].ToString ();
		}
		
		m_targetCorrectLetters = phonemes.Length;
		
		Debug.Log("m_currentWord: " + m_currentWord);
		
		SpeakCurrentWord();
		
		SpellingPadBehaviour.Instance.DisplayNewWord(m_currentWord);
		
		foreach(string phoneme in phonemes)
		{
			DraggableLabel newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, m_spawnedDraggableParent).GetComponent<DraggableLabel>() as DraggableLabel;
			newDraggable.SetUp(phoneme);
			m_draggables.Add(newDraggable);
			
			newDraggable.OnRelease += OnRelease;
			
			newDraggable.transform.localPosition = new Vector3(Random.Range(m_draggableSpawnLow.localPosition.x, m_draggableSpawnHigh.localPosition.x),
			                                                   Random.Range(m_draggableSpawnLow.localPosition.y, m_draggableSpawnHigh.localPosition.y),
			                                                   newDraggable.transform.localPosition.z);
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
	}
	*/

	IEnumerator SpawnQuestion()
	{
		m_wrongAnswers = 0;
		
		string[] currentPhonemeIds = m_currentWordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');

		int numSharedPhonemes = currentPhonemeIds.Length - 1;

		for(int i = numSharedPhonemes; i > -1; --numSharedPhonemes)
		{
			List<DataRow> legalWords = new List<DataRow>();

			for(int j = 0; j < m_wordsPool.Count; ++j)
			{
				// Make sure that you exclude the current target
			}
		}

		yield return null;
	}

	IEnumerator EndGame ()
	{
		yield return new WaitForSeconds(1.5f);
		TransitionScreen.Instance.ChangeLevel("NewScoreDanceScene", false);
	}

	IEnumerator OnQuestionEnd()
	{
		yield return new WaitForSeconds(1f);
		
		foreach(DraggableLabel draggable in m_draggables)
		{
			draggable.Off();
		}
		m_draggables.Clear();
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEARS");
		
		yield return new WaitForSeconds(0.5f);
		
		if(m_score < m_targetScore)
		{
			StartCoroutine(SpawnQuestion());
		}
		else
		{
			StartCoroutine(EndGame());
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
			
			
			++m_score;
			m_scoreBar.SetStarsCompleted(m_score);
			m_scoreBar.SetScore(m_score);
			
			StartCoroutine(OnQuestionEnd());
		}
		else
		{
			SpeakCurrentWord();
			
			currentDraggable.TweenToStartPos();
			
			++m_wrongAnswers;
			
			if(m_wrongAnswers == 2)
			{
				SpellingPadBehaviour.Instance.SayShowSequential();
			}
		}
	}
}
