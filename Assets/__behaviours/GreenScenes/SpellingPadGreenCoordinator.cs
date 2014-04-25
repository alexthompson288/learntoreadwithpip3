using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wingrove;

public class SpellingPadGreenCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_draggablePrefab;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private string[] m_words;

	List<DataRow> m_wordData = new List<DataRow>();

	int m_index = 0;

	List<DraggableLabel> m_draggables = new List<DraggableLabel>();

	int m_targetCorrectLetters = 0;
	int m_correctLetters = 0;
	int m_wrongAnswers = 0;

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		foreach(string word in m_words)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words where word='" + word + "'");

			if(dt.Rows.Count > 0)
			{
				m_wordData.Add(dt.Rows[0]);
			}
		}

		StartCoroutine(DisplayNewWord());
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			--m_index;
			StartCoroutine(DisplayNewWord());
		}
		else if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			++m_index;
			StartCoroutine(DisplayNewWord());
		}
	}

	IEnumerator DisplayNewWord ()
	{
		Debug.Log("DisplayNewWord()");

		if(m_draggables.Count > 0)
		{
			foreach(DraggableLabel draggable in m_draggables)
			{
				draggable.Off();
			}

			m_draggables.Clear();

			yield return new WaitForSeconds(0.6f);
		}

		m_correctLetters = 0;
		m_wrongAnswers = 0;

		if(m_index >= m_wordData.Count)
		{
			m_index = 0;
		}
		else if(m_index < 0)
		{
			m_index = m_wordData.Count - 1;
		}

		SpellingPadBehaviour.Instance.DisplayNewWord(m_wordData[m_index]["word"].ToString());

		string[] phonemeIds = m_wordData[m_index]["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');
		string[] phonemes = new string[phonemeIds.Length];
		for(int i = 0; i < phonemeIds.Length; ++i)
		{
			phonemes[i] = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'").Rows[0]["phoneme"].ToString ();
		}

		m_targetCorrectLetters = phonemes.Length;

		List<Transform> locators = m_locators.ToList();
		
		foreach(string phoneme in phonemes)
		{
			int i = Random.Range(0, locators.Count);
			DraggableLabel newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, locators[i]).GetComponent<DraggableLabel>() as DraggableLabel;
			Debug.Log(locators[i].name);
			locators.RemoveAt(i);
			newDraggable.SetUp(phoneme, null, true);
			m_draggables.Add(newDraggable);
			
			newDraggable.OnRelease += OnRelease;
		}

		SpeakCurrentWord();

		yield return null;
	}

	void OnRelease(DraggableLabel currentDraggable)
	{
		SpellingPadPhoneme spellingPadPhoneme = SpellingPadBehaviour.Instance.CheckLetters(currentDraggable.GetText(), currentDraggable.collider);
		
		if(spellingPadPhoneme != null)
		{
			spellingPadPhoneme.ChangeState(SpellingPadPhoneme.State.Answered);
			
			currentDraggable.SetCanDrag(false);
			
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			currentDraggable.TweenToPos(spellingPadPhoneme.transform.position);
			currentDraggable.ChangeToOnTexture();
			
			++m_correctLetters;

			if(m_correctLetters < m_targetCorrectLetters && m_wrongAnswers >= 3)
			{
				SpellingPadBehaviour.Instance.SayShowSequential();
			}

			/*
			if(m_correctLetters >= m_targetCorrectLetters)
			{
				StartCoroutine(OnQuestionEnd());
			}
			else
			{
				if(m_wrongAnswers >= 3)
				{
					SpellingPadBehaviour.Instance.SayShowSequential();
				}
			}
			*/
		}
		else
		{
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

	public void SpeakCurrentWord()
	{
		AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_wordData[m_index]["word"].ToString());
		GetComponent<AudioSource>().clip = loadedAudio;
		GetComponent<AudioSource>().Play();
	}
}
