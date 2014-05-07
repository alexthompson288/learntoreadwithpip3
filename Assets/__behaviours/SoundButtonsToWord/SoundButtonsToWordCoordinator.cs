using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System.Linq;

public class SoundButtonsToWordCoordinator : MonoBehaviour 
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
	
	private int m_score = 0;
	
	List<DataRow> m_wordPool = new List<DataRow>();
	
	List<PipPadPhonemeSubButton> m_buttons = new List<PipPadPhonemeSubButton>();

	int m_totalPhonemes;
	int m_correctPhonemes;

	List<GameObject> m_draggableButtons = new List<GameObject>();
	
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		m_wordPool = DataHelpers.GetWords();

		//SqliteDatabase db = GameDataBridge.Instance.GetDatabase();
		//int sectionId = 1392;
		//DataTable dt = db.ExecuteQuery("select * from data_words INNER JOIN words ON word_id = words.id WHERE section_id=" + sectionId);
		//m_wordPool = dt.Rows; 

		List<DataRow> phonemes = DataHelpers.GetPhonemes();
		DataRow targetPhoneme = null;

		foreach(DataRow phoneme in phonemes)
		{
			if(phoneme["is_target_phoneme"] != null && phoneme["is_target_phoneme"].ToString() == "t")
			{
				targetPhoneme = phoneme;
				break;
			}
		}

		if(targetPhoneme != null)
		{
			yield return new WaitForSeconds(0.5f);

			Debug.Log("targetPhoneme: " + targetPhoneme["phoneme"].ToString());

			for(int i = m_wordPool.Count - 1; i > -1; --i)
			{
				List<DataRow> orderedPhonemes = DataHelpers.GetOrderedPhonemes(m_wordPool[i]);
				if(!orderedPhonemes.Contains(targetPhoneme))
				{
					m_wordPool.RemoveAt(i);
				}
			}

			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, m_letterButtonStartPosition);

			LetterButton letterButton = newButton.GetComponent<LetterButton>() as LetterButton;
			letterButton.SetUp(targetPhoneme);
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
			GameManager.Instance.CompleteGame();
		}
	}
	
	void ShowWord ()
	{
		Debug.Log("ShowWord()");
		DataRow targetWord = m_wordPool[Random.Range(0, m_wordPool.Count)];
		
		PipPadBehaviour.Instance.Show(targetWord["word"].ToString());

		string[] phonemes = targetWord["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');

		m_correctPhonemes = 0;
		m_totalPhonemes = phonemes.Length;

		DataRow[] phonemeData = new DataRow[phonemes.Length];

		for(int i = 0; i < phonemes.Length; ++i)
		{
			DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes WHERE id='" + phonemes[i] + "'");
			if(dt.Rows.Count > 0)
			{
				phonemeData[i] = dt.Rows[0];
			}
		}

		List<Transform> locators = m_locators.ToList();
		
		foreach(DataRow data in phonemeData)
		{
			GameObject buttonPrefab = null;

			switch(data["phoneme"].ToString().Length)
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

			AudioClip graphemeAudio = AudioBankManager.Instance.GetAudioClip(data["grapheme"].ToString());

			newButton.GetComponent<Draggable>().SetUp(data, graphemeAudio);
			newButton.GetComponent<Draggable>().OnRelease += OnDraggableRelease;
		}

		PipPadBehaviour.Instance.EnableButtons(false);
		PipPadBehaviour.Instance.EnableDragColliders(true);
		
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
			++m_correctPhonemes;

			if(m_correctPhonemes >= m_totalPhonemes)
			{
				StartCoroutine(OnFinishWord());
			}
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
			GameManager.Instance.CompleteGame();
		}
	}

}
