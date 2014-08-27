using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using WingroveAudio;
using System.Linq;

public class NonsenseSpellingCoordinator : Singleton<NonsenseSpellingCoordinator> 
{
	[SerializeField]
	private bool m_useSillyWords;
	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private GameObject m_draggablePrefab;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField] 
	private Transform m_spawnedDraggableParent; // TODO: Delete
	[SerializeField]
	private Transform m_draggableSpawnHigh; // TODO: Delete
	[SerializeField]
	private Transform m_draggableSpawnLow; // TODO: Delete
	[SerializeField]
	private ProgressScoreBar m_scoreBar;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private Troll m_troll;
    [SerializeField]
    private SpellingPadBehaviour m_spellingPad;
	
	int m_score;
	
	List<DataRow> m_wordsPool = new List<DataRow>();

	string m_currentWord;
	
	List<DraggableLabel> m_draggables = new List<DraggableLabel>();

	int m_targetCorrectLetters = 0;
	int m_correctLetters = 0;
	int m_wrongAnswers = 0;
	
	// Use this for initialization
	IEnumerator Start () 
	{
		// always pip, always winner
		SessionInformation.Instance.SetPlayerIndex(0, 3);
		SessionInformation.Instance.SetWinner(0);
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_wordsPool = m_useSillyWords ? DataHelpers.GetSillywords() : DataHelpers.GetWords();

		if(m_targetScore > m_wordsPool.Count)
		{
			m_targetScore = m_wordsPool.Count;
		}

		m_scoreBar.SetStarsTarget(m_targetScore);
		
		if(m_wordsPool.Count > 0)
		{
			yield return new WaitForSeconds(0.5f);
			StartCoroutine(SpawnQuestion());
		}
		else
		{
			StartCoroutine(EndGame());
		}
	}
	
	public void SpeakCurrentWord()
	{
		AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWord);
		GetComponent<AudioSource>().clip = loadedAudio;
		GetComponent<AudioSource>().Play();
	}

	IEnumerator SpawnQuestion ()
	{
		m_correctLetters = 0;
		m_wrongAnswers = 0;
		
		DataRow currentWordData = m_wordsPool[Random.Range(0, m_wordsPool.Count)];

		m_wordsPool.Remove(currentWordData);

		m_currentWord = currentWordData["word"].ToString();
		
        string[] phonemeIds = DataHelpers.GetOrderedPhonemeIdStrings(currentWordData);
		string[] phonemes = new string[phonemeIds.Length];
		for(int i = 0; i < phonemeIds.Length; ++i)
		{
			phonemes[i] = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'").Rows[0]["phoneme"].ToString ();
		}

		m_targetCorrectLetters = phonemes.Length;
		
		//////D.Log("m_currentWord: " + m_currentWord);
		
		//SpeakCurrentWord();
		
		m_spellingPad.DisplayNewWord(m_currentWord);

		List<Transform> locators = m_locators.ToList();

		foreach(string phoneme in phonemes)
		{
			int i = Random.Range(0, locators.Count);
			DraggableLabel newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, locators[i]).GetComponent<DraggableLabel>() as DraggableLabel;
			//////D.Log(locators[i].name);
			locators.RemoveAt(i);
			newDraggable.SetUp(phoneme, null, true);
			m_draggables.Add(newDraggable);
			
			newDraggable.OnRelease += OnRelease;
		}

		/*
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
		*/

		m_spellingPad.SayWholeWord();
		
		yield return null;
	}

	IEnumerator EndGame ()
	{
		yield return new WaitForSeconds(1.5f);

        GameManager.Instance.CompleteGame();
	}

	IEnumerator OnQuestionEnd()
	{
		GameObject printedWord = m_spellingPad.PrintWord();
		//iTween.MoveTo(printedWord, Vector3.zero, 0.3f);
		//yield return new WaitForSeconds(0.31f);
		float burpDelay = m_troll.EatFood(printedWord);

		yield return new WaitForSeconds(burpDelay);


		yield return new WaitForSeconds(1f);

		foreach(DraggableLabel draggable in m_draggables)
		{
			draggable.Off();
		}
		m_draggables.Clear();

		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");

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
        /*
		SpellingPadPhoneme spellingPadPhoneme = m_spellingPad.CheckLetters(currentDraggable.GetText(), currentDraggable.collider);

		if(spellingPadPhoneme != null)
		{
			spellingPadPhoneme.ChangeState(SpellingPadPhoneme.State.Answered);

			m_draggables.Remove(currentDraggable);
			currentDraggable.SetCanDrag(false);
			currentDraggable.Off();


			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			currentDraggable.TweenToPos(spellingPadPhoneme.transform.position);
			currentDraggable.ChangeToOnTexture();

			++m_correctLetters;

			if(m_correctLetters >= m_targetCorrectLetters)
			{
				++m_score;
				m_scoreBar.SetStarsCompleted(m_score);
				m_scoreBar.SetScore(m_score);

				StartCoroutine(OnQuestionEnd());
			}
			else
			{
				if(m_wrongAnswers >= 3)
				{
					m_spellingPad.SayShowSequential();
				}
			}
		}
		else
		{
			SpeakCurrentWord();
			
			currentDraggable.TweenToStartPos();

			++m_wrongAnswers;

			switch(m_wrongAnswers)
			{
			case 2:
				m_spellingPad.SayShowAll(true);
				break;
			case 3:
				m_spellingPad.SayShowSequential();
				break;
			default:
				m_spellingPad.SayAll();
				break;
			}
		}
        */
	}
}