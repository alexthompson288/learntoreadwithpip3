using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlashcardPlayCoordinator : Singleton<FlashcardPlayCoordinator>
{	
	[SerializeField]
	private int m_targetScore = 5;
	[SerializeField]
	private CharacterPopper m_characterToPop;
	[SerializeField]
	private CharacterPopper m_characterToPopTroll;
	[SerializeField]
	private int m_maxSpawn = 2;
	[SerializeField]
	private UILabel m_scoreLabel;
	
	private List<string> m_remainingWords = new List<string>();
	private List<string> m_allWords = new List<string>();
	string m_correctWord;

	int m_score = 0;
	bool m_gotIncorrect = false;

	
	// Use this for initialization
	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		PipPadBehaviour.Instance.OnBlackboardClick += OnBlackboardClick;
		
		List<DataRow> wordSelection = GameDataBridge.Instance.GetWords();
		

		for(int i = 0; i < wordSelection.Count; ++i)
		{
			Debug.Log(wordSelection[i]["word"].ToString());
			Texture2D tex = null;
			if(wordSelection[i]["image"] != null)
			{
				tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + wordSelection[i]["image"].ToString());
			}
			if(tex == null)
			{
				tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + wordSelection[i]["word"].ToString());
			}
			Debug.Log("tex: " + tex);
			if(tex != null)
			{
				m_remainingWords.Add(wordSelection[i]["word"].ToString());
				m_allWords.Add(wordSelection[i]["word"].ToString());
			}
		}

		Resources.UnloadUnusedAssets();


		if(m_targetScore > m_remainingWords.Count)
		{
			m_targetScore = m_remainingWords.Count;
		}

		UpdateScoreLabel();

		if(m_remainingWords.Count > m_targetScore)
		{
			CollectionHelpers.Shuffle(m_remainingWords); // Shuffle before removing any words.

			for(int i = m_remainingWords.Count - 1; i > m_targetScore - 1; --i)
			{
				m_remainingWords.RemoveAt(i);
			}
		}

		Debug.Log("m_remainingWords.Count: " + m_remainingWords.Count);


		if(m_remainingWords.Count >= m_maxSpawn)
		{
			StartCoroutine(ShowNextQuestion());
		}
		else
		{
			StartCoroutine(FinishGame());
		}
	}
	
	IEnumerator ShowNextQuestion()
	{
		m_gotIncorrect = false;

		m_correctWord = null;
		Texture2D targetTex = null;

		while(targetTex == null)
		{
			m_correctWord = m_remainingWords[Random.Range(0, m_remainingWords.Count)];
			Debug.Log("m_correctWord: " + m_correctWord);
			targetTex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + m_correctWord);
		}

		m_remainingWords.Remove(m_correctWord);
		
		List<string> wordList = new List<string>();

		wordList.Add(m_correctWord);

		while (wordList.Count < m_maxSpawn)
		{
			Debug.Log("Adding to otherWordList: " + wordList.Count);
			string otherWord = m_allWords[Random.Range(0, m_allWords.Count)];
			Texture2D tex = null;
			while (otherWord == m_correctWord || tex == null)
			{
				otherWord = m_allWords[Random.Range(0, m_allWords.Count)];
				Debug.Log("otherWord: " + otherWord);
				tex = (Texture2D)Resources.Load("Images/word_images_png_350/_" + otherWord);
			}
			if ( !wordList.Contains(otherWord) )
			{
				wordList.Add(otherWord);
			}
		}

		PipPadBehaviour.Instance.Show(m_correctWord);

		CollectionHelpers.Shuffle(wordList);
		PipPadBehaviour.Instance.ShowMultipleBlackboards(wordList);
		
		yield break;
	}
	
	IEnumerator OnCorrectClick()
	{
		if (!m_gotIncorrect)
		{
			PipPadBehaviour.Instance.SayWholeWord();
			WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");

			m_characterToPop.PopCharacter();

			m_score++;
			UpdateScoreLabel();
			iTween.PunchScale(m_scoreLabel.transform.parent.gameObject, Vector3.one * 1.5f, 0.8f);

			yield return new WaitForSeconds(1.25f);
		}
		else
		{
			PipPadBehaviour.Instance.SayAll(0.5f);
			yield return new WaitForSeconds(4.0f);
		}
		PipPadBehaviour.Instance.Hide();

		if (m_remainingWords.Count > 0)
		{
			yield return new WaitForSeconds(1.5f);
			StartCoroutine(ShowNextQuestion());
		}
		else
		{
			StartCoroutine(FinishGame());
		}
	}
	
	IEnumerator FinishGame()
	{
		yield return new WaitForSeconds(1.5f);

		if(m_score >= m_targetScore)
		{
			Debug.Log("Current Level: " + SkillProgressInformation.Instance.GetCurrentLevel());
			Debug.Log("Current Progress: " + SkillProgressInformation.Instance.GetCurrentSkillProgress());

			if(SkillProgressInformation.Instance.GetCurrentLevel() > SkillProgressInformation.Instance.GetCurrentSkillProgress())
			{
				SkillProgressInformation.Instance.IncrementCurrentSkillProgress();
			}

			if(Random.Range(0, 2) == 0 || SkillProgressInformation.Instance.GetCurrentLevel() == 1) // We always want to play troll explode if they have just beaten level 1
			{
				yield return StartCoroutine(CelebrationCoordinator.Instance.ExplodeLetters());
			}
			else
			{
				yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
			}
		}

		TransitionScreen.Instance.ChangeLevel("NewFlashcardMap", false);
	}
	
	public void OnBlackboardClick(ImageBlackboard clickedBlackboard)
	{
		if (StringHelpers.Edit(clickedBlackboard.GetImageName()) == m_correctWord)
		{
			StopAllCoroutines();
			StartCoroutine(OnCorrectClick());
		}
		else
		{
			m_gotIncorrect = true;
			m_characterToPopTroll.PopCharacter();
			clickedBlackboard.ShakeFade();
			StopAllCoroutines();
			StartCoroutine(StartIncorrect());
		}
	}
	
	IEnumerator StartIncorrect()
	{
		//iTween.PunchScale(m_scoreLabel.transform.parent.gameObject, Vector3.one * 0.5f, 0.8f);
		iTween.ShakePosition(m_scoreLabel.transform.parent.gameObject, Vector3.one * 0.1f, 0.8f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_MINOR");
		yield return new WaitForSeconds(0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("GOOD_TRY");
		
		yield return new WaitForSeconds(1.5f);
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("PRESS_BLUE_BUTTONS");
		
		yield return new WaitForSeconds(1.0f);
	}

	void UpdateScoreLabel()
	{
		m_scoreLabel.text = System.String.Format("{0}/{1}", m_score, m_targetScore);
	}
}
