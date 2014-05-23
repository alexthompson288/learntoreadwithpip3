﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using WingroveAudio;
using System.Linq;

public class SensibleSpellingCoordinator : MonoBehaviour 
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
	private ProgressScoreBar m_scoreBar;
	[SerializeField]
	private AudioSource m_audioSource;
	
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
		
		yield return new WaitForSeconds(0.5f);

		if(m_wordsPool.Count > 0)
		{
			StartCoroutine(SpawnQuestion());
		}
		else
		{
			EndGame();
		}
	}
	
	public void SpeakCurrentWord()
	{
		AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWord);
		if(loadedAudio != null)
		{
			GetComponent<AudioSource>().clip = loadedAudio;
			GetComponent<AudioSource>().Play();
		}
	}
	
	IEnumerator SpawnQuestion ()
	{
		m_correctLetters = 0;
		m_wrongAnswers = 0;
		
		DataRow currentWordData = m_wordsPool[Random.Range(0, m_wordsPool.Count)];

		m_wordsPool.Remove(currentWordData);

		m_currentWord = currentWordData["word"].ToString();
		
		string[] phonemeIds = currentWordData["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
		string[] phonemes = new string[phonemeIds.Length];
		for(int i = 0; i < phonemeIds.Length; ++i)
		{
			phonemes[i] = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'").Rows[0]["phoneme"].ToString ();
		}
		
		m_targetCorrectLetters = phonemes.Length;
		
		SpellingPadBehaviour.Instance.DisplayNewWord(m_currentWord);
		
		List<Transform> locators = m_locators.ToList();
		
		for(int i = 0; i < phonemes.Length; ++i)
		{
			int locatorIndex = Random.Range(0, locators.Count);
			DraggableLabel newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, locators[locatorIndex]).GetComponent<DraggableLabel>() as DraggableLabel;

			locators.RemoveAt(locatorIndex);

			AudioClip letterAudio = null;

			DataTable dtPhonemes = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + phonemeIds[i] + "'");
			if(dtPhonemes.Rows.Count > 0)
			{
				string audioFilename =
				string.Format("{0}",
				dtPhonemes.Rows[0]["grapheme"]);
				
				if(((currentWordData["tricky"] != null && currentWordData["tricky"].ToString() == "t")
				    || (currentWordData["nondecodable"] != null && currentWordData["nondecodable"].ToString() == "t"))
				   && (currentWordData["nonsense"] == null || currentWordData["nonsense"].ToString() == "f"))
				{
					audioFilename = "lettername_" + audioFilename;
				}

				letterAudio = AudioBankManager.Instance.GetAudioClip(audioFilename);
			}

			newDraggable.SetUp(phonemes[i], letterAudio, true);
			m_draggables.Add(newDraggable);
			
			newDraggable.OnRelease += OnRelease;
		}
		
		SpellingPadBehaviour.Instance.SayWholeWord();
		
		yield return null;
	}
	
	void EndGame ()
	{
		SessionInformation.SetDefaultPlayerVar();
		GameManager.Instance.CompleteGame();
	}
	
	IEnumerator OnQuestionEnd()
	{
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
			EndGame();
		}
	}
	
	void OnRelease(DraggableLabel currentDraggable)
	{
		SpellingPadPhoneme spellingPadPhoneme = SpellingPadBehaviour.Instance.CheckLetters(currentDraggable.GetText(), currentDraggable.collider);
		
		if(spellingPadPhoneme != null)
		{
			Debug.Log("Correct");

            ///////////////////////////////////////////////////////
            // TODO: This should go in SpellingPadBehaviour
			spellingPadPhoneme.ChangeState(SpellingPadPhoneme.State.Answered);

			m_draggables.Remove(currentDraggable);
			currentDraggable.SetCanDrag(false);
			currentDraggable.Off();
			
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			currentDraggable.TweenToPos(spellingPadPhoneme.transform.position);
			currentDraggable.ChangeToOnTexture();
            /////////////////////////////////////////////////////
			
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
					SpellingPadBehaviour.Instance.SayShowSequential();
				}
			}
		}
		else
		{
			Debug.Log("Incorrect");

			SpeakCurrentWord();
			
			currentDraggable.TweenToStartPos();
			
			++m_wrongAnswers;
			
			switch(m_wrongAnswers)
			{
			case 2:
				SpellingPadBehaviour.Instance.SayShowAll(true);
				break;
			case 3:
				SpellingPadBehaviour.Instance.SayShowSequential();
				break;
			default:
				SpellingPadBehaviour.Instance.SayAll();
				break;
			}
		}
	}
}