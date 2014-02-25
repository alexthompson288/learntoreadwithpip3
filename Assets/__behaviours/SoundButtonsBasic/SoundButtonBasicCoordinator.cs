using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundButtonBasicCoordinator : MonoBehaviour 
{
	[SerializeField]
	private ProgressScoreBar m_scoreBar;
	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private SplattableBug m_splattable;
	[SerializeField]
	private Transform m_splattableOff;

	private int m_score = 0;

	List<DataRow> m_wordPool = new List<DataRow>();

	List<GameObject> m_orderedPhonemes = new List<GameObject>();

	List<PipPadPhonemeSubButton> m_buttons = new List<PipPadPhonemeSubButton>();

	IEnumerator Start () 
	{
		yield return new WaitForSeconds(1f);
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_wordPool = GameDataBridge.Instance.GetWords();

		/*
		int sectionId = 1392;
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id = words.id WHERE section_id=" + sectionId);
		m_wordPool = dt.Rows;
		*/
		
		m_scoreBar.SetStarsTarget(m_targetScore);

		//StartCoroutine(ShowWord());

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

		PipPadBehaviour.Instance.Show(targetWord["word"].ToString());

		List<GameObject> orderedPhonemes = PipPadBehaviour.Instance.GetCreatedPhonemes();

		foreach(GameObject phoneme in orderedPhonemes)
		{
			m_orderedPhonemes.Add(phoneme);
		}

		PipPadBehaviour.Instance.SayWholeWord();

		foreach(GameObject phoneme in m_orderedPhonemes)
		{
			PipPadPhonemeSubButton[] button = phoneme.GetComponentsInChildren<PipPadPhonemeSubButton>() as PipPadPhonemeSubButton[];
			button[0].OnButtonClick += OnButtonClick;
			button[1].OnButtonClick += OnButtonClick;
		}

		//m_splattable.transform.position = m_orderedPhonemes[0].transform.position;

		PipPadBehaviour.Instance.EnableLabels(false);

		StartCoroutine(MoveSplattable());
	}

	public void OnButtonClick(PipPadPhoneme button)
	{
		Debug.Log("OnButtonClick()");

		if(button.gameObject == m_orderedPhonemes[0])
		{
			m_orderedPhonemes.RemoveAt(0);

			button.EnableLabel(true);

			m_splattable.Splat();
			StopCoroutine("MoveSplattable");
			StartCoroutine(MoveSplattable());

			if(m_orderedPhonemes.Count == 0)
			{
				StartCoroutine(OnFinishWord());
			}
		}
		else
		{
			PipPadBehaviour.Instance.SayWholeWord();
		}
	}

	IEnumerator MoveSplattable()
	{
		yield return new WaitForSeconds(0.8f);
		m_splattable.Unsplat();
		try
		{
			Vector3 splattablePosition = m_orderedPhonemes[0].transform.position;
			splattablePosition.y -= 0.35f;
			m_splattable.transform.position = splattablePosition;
		}
		catch
		{
			m_splattable.transform.position = m_splattableOff.position;
		}
	}

	IEnumerator OnFinishWord()
	{
		yield return new WaitForSeconds(0.8f);

		PipPadBehaviour.Instance.SayWholeWord();

		yield return new WaitForSeconds(1f);

		PipPadBehaviour.Instance.Hide();

		++m_score;

		m_scoreBar.SetStarsCompleted(m_score);
		m_scoreBar.SetScore(m_score);
		
		yield return new WaitForSeconds(1f);
		
		if(m_score < m_targetScore)
		{
			//StartCoroutine(ShowWord());
			ShowWord();
		}
		else
		{
			SessionManager.Instance.OnGameFinish();
		}
	}
}
