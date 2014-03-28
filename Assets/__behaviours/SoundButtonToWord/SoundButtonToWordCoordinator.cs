using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System.Linq;

public class SoundButtonToWordCoordinator : MonoBehaviour 
{
	[SerializeField]
	private ProgressScoreBar m_scoreBar;
	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private GameObject m_letterButtonPrefab;
	[SerializeField]
	private Transform m_letterButtonStartPosition;
	[SerializeField]
	private Transform m_letterButtonEndPosition;
	[SerializeField]
	private GameObject m_singlePrefab;
	[SerializeField]
	private GameObject m_doublePrefab;
	[SerializeField]
	private GameObject m_triplePrefab;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private int m_numDummies = 3; // m_numDummies is only used if we do not use linking_index, if we do use linking_index then we just use however many dummies are in the index
	
	private int m_score = 0;
	
	List<DataRow> m_wordPool = new List<DataRow>();

	List<DataRow> m_targetPhonemes = new List<DataRow>();
	List<DataRow> m_dummyPhonemes = new List<DataRow>();
	
	List<PipPadPhonemeSubButton> m_buttons = new List<PipPadPhonemeSubButton>();
	
	List<GameObject> m_draggableButtons = new List<GameObject>();



	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		m_wordPool = DataHelpers.GetWords();
		
		//SqliteDatabase db = GameDataBridge.Instance.GetDatabase();
		
		//int sectionId = 1392;
		//DataTable dt = db.ExecuteQuery("select * from data_words INNER JOIN words ON word_id = words.id WHERE section_id=" + sectionId);
		//m_wordPool = dt.Rows; 
		
		List<DataRow> phonemes = DataHelpers.GetLetters();
		
		foreach(DataRow phoneme in phonemes)
		{
			if(phoneme["is_target_phoneme"] != null && phoneme["is_target_phoneme"].ToString() == "t")
			{
				m_targetPhonemes.Add(phoneme);
			}
			else
			{
				m_dummyPhonemes.Add(phoneme);
			}
		}

		if(m_numDummies < m_dummyPhonemes.Count)
		{
			m_numDummies = m_dummyPhonemes.Count;
		}
		
		if(m_targetPhonemes.Count == 1) // If there is only 1 target phoneme then spawn the LetterButton 
		{
			yield return new WaitForSeconds(0.5f);
			
			Debug.Log("targetPhoneme: " + m_targetPhonemes[0]["phoneme"].ToString());
			
			for(int i = m_wordPool.Count - 1; i > -1; --i)
			{
				List<DataRow> orderedPhonemes = DataHelpers.GetOrderedPhonemes(m_wordPool[i]);
				if(!orderedPhonemes.Contains(m_targetPhonemes[0]))
				{
					m_wordPool.RemoveAt(i);
				}
			}
			
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, m_letterButtonStartPosition);
			
			LetterButton letterButton = newButton.GetComponent<LetterButton>() as LetterButton;
			letterButton.SetUp(m_targetPhonemes[0]);
			letterButton.SetMethods( letterButton.PlayPhonemeAudio, letterButton.PlayMnemonicAudio );
			
			newButton.transform.localScale = Vector3.one * 1.5f;
			iTween.ScaleFrom(newButton, Vector3.zero, 0.5f);
			
			yield return new WaitForSeconds(0.5f);
			
			letterButton.PlayMnemonicAudio();
			
			yield return new WaitForSeconds(letterButton.GetMnemonicAudioLength() + 0.2f);
			
			float smallTweenDuration = 0.5f;
			iTween.ScaleTo( newButton, Vector3.one * 0.2f, smallTweenDuration );
			iTween.MoveTo( newButton, m_letterButtonEndPosition.position, smallTweenDuration );
			
			yield return new WaitForSeconds(smallTweenDuration);
		}
		
		m_scoreBar.SetStarsTarget(m_targetScore);
		
		if(m_wordPool.Count > 0)
		{
			ShowWord();
		}
		else
		{
			SessionManager.Instance.OnGameFinish();
		}
	}
	
	void ShowWord ()
	{
		Debug.Log("ShowWord()");
		DataRow targetWord = m_wordPool[Random.Range(0, m_wordPool.Count)];

		DataRow targetPhoneme = null;
		HashSet<DataRow> answerPhonemes = new HashSet<DataRow>();

		if(m_targetPhonemes.Count == 1) // If there is only 1 target phoneme then don't use linking indices because target/dummies never change
		{
			targetPhoneme = m_targetPhonemes[0];
			answerPhonemes.Add(targetPhoneme);

			while(answerPhonemes.Count < m_numDummies)
			{
				answerPhonemes.Add(m_dummyPhonemes[Random.Range(0, m_dummyPhonemes.Count)]);
			}
		}
		else
		{
			int linkingIndex = System.Convert.ToInt32(targetWord["linking_index"]);

			foreach(DataRow phoneme in m_targetPhonemes)
			{
				if(System.Convert.ToInt32(phoneme["linking_index"]) == linkingIndex)
				{
					targetPhoneme = phoneme;
					answerPhonemes.Add(targetPhoneme);
					break;
				}
			}

			foreach(DataRow phoneme in m_dummyPhonemes)
			{
				if(System.Convert.ToInt32(phoneme["linking_index"]) == linkingIndex)
				{
					answerPhonemes.Add(phoneme);
				}
			}
		}

		PipPadBehaviour.Instance.Show(targetWord["word"].ToString());
		
		List<Transform> locators = m_locators.ToList();
		
		foreach(DataRow phoneme in answerPhonemes)
		{
			GameObject buttonPrefab = null;
			
			switch(phoneme["phoneme"].ToString().Length)
			{
			case 1:
				buttonPrefab = m_singlePrefab; 
				break;
			case 2:
				buttonPrefab = m_doublePrefab;
				break;
			default:
				buttonPrefab = m_triplePrefab;
				break;
			}
			
			int locatorIndex = Random.Range(0, locators.Count);
			
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(buttonPrefab, locators[locatorIndex]);
			
			m_draggableButtons.Add(newButton);
			
			locators.RemoveAt(locatorIndex);
			
			AudioClip graphemeAudio = AudioBankManager.Instance.GetAudioClip(phoneme["grapheme"].ToString());
			
			newButton.GetComponent<Draggable>().SetUp(phoneme, graphemeAudio);
			newButton.GetComponent<Draggable>().OnRelease += OnDraggableRelease;

			if(locators.Count == 0) // Break if we run out of locators. targetPhoneme is the first to be added to answerPhonemes, so targetPhoneme always spawns
			{
				break;
			}
		}
		
		PipPadBehaviour.Instance.EnableButton(false, targetPhoneme["phoneme"].ToString());
		PipPadBehaviour.Instance.EnableDragCollider(true, targetPhoneme["phoneme"].ToString());
		
		PipPadBehaviour.Instance.SayWholeWord();
	}
	
	void OnDraggableRelease(Draggable draggable)
	{
		List<GameObject> trackers = PipPadBehaviour.Instance.GetDraggablesTracking(draggable.gameObject);
		Debug.Log("trackers.Count: " + trackers.Count);
		
		PipPadPhoneme correctPhoneme = null;
		
		foreach(GameObject tracker in trackers)
		{
			if(tracker.GetComponent<PipPadPhoneme>().GetText() == draggable.GetData()["phoneme"].ToString())
			{
				correctPhoneme = tracker.GetComponent<PipPadPhoneme>() as PipPadPhoneme;
			}
		}
		
		if(correctPhoneme != null)
		{
			draggable.TweenToPos(correctPhoneme.GetButtonPos());
			WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
			correctPhoneme.EnableSubButtons(true);
			correctPhoneme.EnableDragCollider(false);
			draggable.enabled = false;

			StartCoroutine(OnFinishWord());
		}
		else
		{
			draggable.TweenToStartPos();
		}
	}
	
	
	IEnumerator OnFinishWord()
	{
		yield return new WaitForSeconds(0.5f);
		
		//PipPadBehaviour.Instance.SayWholeWord();
		PipPadBehaviour.Instance.SayAll(0);
		
		yield return new WaitForSeconds(PipPadBehaviour.Instance.GetTotalLength() + 0.3f);
		
		for(int i = m_draggableButtons.Count - 1; i > -1; --i)
		{
			Destroy(m_draggableButtons[i]);
		}
		
		m_draggableButtons.Clear();
		
		PipPadBehaviour.Instance.Hide();
		
		++m_score;
		
		m_scoreBar.SetStarsCompleted(m_score);
		m_scoreBar.SetScore(m_score);
		
		yield return new WaitForSeconds(1f);
		
		if(m_score < m_targetScore)
		{
			ShowWord();
		}
		else
		{
			SessionManager.Instance.OnGameFinish();
		}
	}
	
}
