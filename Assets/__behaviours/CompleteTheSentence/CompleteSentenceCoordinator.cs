using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class CompleteSentenceCoordinator : MonoBehaviour 
{
	[SerializeField]
	private GameObject m_textureParent;
	[SerializeField]
	private UITexture m_texture;
	[SerializeField]
	private GameObject m_sentenceLabelParent;
	[SerializeField]
	private UISprite m_sentenceBackground;
	[SerializeField]
	private UITexture m_targetWordBackground;
	[SerializeField]
	private UIFont m_font;
	[SerializeField]
	private GameObject m_draggableLabelPrefab;
	[SerializeField]
	private GameObject m_completeSentenceWordPrefab;
	[SerializeField]
    private GameObject m_audioPlayButton;
	[SerializeField]
    private ProgressScoreBar m_scoreBar;
	[SerializeField]
	private GameObject[] m_locators;
	[SerializeField]
	private Transform m_topBoundary;
	[SerializeField]
	private int m_numToSpawn;
	[SerializeField]
	private int m_targetScore;
	[SerializeField]
	private string m_imageFolderName;
	[SerializeField]
	private Transform m_textPosition;
	[SerializeField]
	private bool m_useDifficultySections;
	[SerializeField]
	private float m_imageScale = 0.4f;
	
	int m_score;
	
	string m_targetWord;
	string m_currentSentence;
	
	Dictionary<int, string> m_sentences = new Dictionary<int, string>();
	Dictionary<int, string> m_imageNames = new Dictionary<int, string>();
	Dictionary<int, List<DataRow>> m_words = new Dictionary<int, List<DataRow>>();
	
	List<DraggableLabel> m_spawnedDraggables = new List<DraggableLabel>();
	List<CompleteSentenceWord> m_spawnedSentenceWords = new List<CompleteSentenceWord>();

    bool m_askOneQuestion = false;
	
	// Use this for initialization
	IEnumerator Start () 
	{
		UserStoriesStats.Instance.ClearAnswers();

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        /*
		int sectionId = 1420;

		if(Game.session == Game.Session.Premade)
		{
			sectionId = SessionManager.Instance.GetCurrentSectionId();
		}
		else if (Game.session == Game.Session.Custom)
		{
			List<DataRow> storyData = LessonInfo.Instance.GetData("stories");

			if(storyData.Count > 0)
			{
				sectionId = Convert.ToInt32(storyData[0]["section_id"]);
				Debug.Log("Lesson Story sectionId: " + sectionId);
			}
			else
			{
				Debug.Log("No Lesson Story Data");
			}
		}
		else
		{
			Debug.Log("Stories");
			string bookId = SessionInformation.Instance.GetBookId().ToString();
			Debug.Log("bookId: " + bookId);
			DataTable bookTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE id=" + bookId);

			if(bookTable.Rows.Count > 0)
			{
				DataRow row = bookTable.Rows[0];
				sectionId = Convert.ToInt32(row["section_id"]);
				Debug.Log("Found book, sectionId= " + sectionId);
			}
		}
        */

        List<DataRow> stories = GameManager.Instance.GetData("stories");
        DataTable tempDt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from stories WHERE id=52");
        stories = tempDt.Rows;

        if (stories.Count > 0 && stories [0] ["section_id"] != null)
        {
            int sectionId = Convert.ToInt32(stories[0]["section_id"]);

            Debug.Log("sectionId: " + sectionId);
    		
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sentences WHERE section_id=" + sectionId);
            List<DataRow> rows = dt.Rows;

            Debug.Log("There are " + dt.Rows.Count + " sentences");

            m_askOneQuestion = rows.FindIndex(x => x["is_target_sentence"] == null) == -1;

            //if (Game.session == Game.Session.Premade)
            if (m_askOneQuestion)
            {
                foreach (DataRow row in rows)
                {
                    if (row ["is_target_sentence"].ToString() == "t")
                    {
                        m_sentences [0] = row ["text"].ToString();
                    } 
                    else
                    {
                        m_imageNames [0] = row ["text"].ToString();
                    }
                }
            } 
            else
            {
                foreach (DataRow row in rows)
                {
                    int linkingIndex = Convert.ToInt32(row ["linking_index"]);
    				
                    if (row ["is_target_sentence"].ToString() == "t")
                    {
                        m_sentences [linkingIndex] = row ["text"].ToString();
                    } else
                    {
                        m_imageNames [linkingIndex] = row ["text"].ToString();
                    }
                }
            }
            
            if (m_targetScore > m_sentences.Count)
            {
                m_targetScore = m_sentences.Count;
            }

            Debug.Log("m_targetScore: " + m_targetScore);

            if (m_targetScore > 1)
            {
                m_scoreBar.SetStarsTarget(m_targetScore);
            } else
            {
                m_scoreBar.gameObject.SetActive(false);
            }


            dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE section_id=" + sectionId);
            rows = dt.Rows;

            Debug.Log("There are " + dt.Rows.Count + " words");

            //if (Game.session == Game.Session.Premade)
            if(m_askOneQuestion)
            {
                m_words [0] = new List<DataRow>();
                foreach (DataRow row in rows)
                {
                    m_words [0].Add(row);
                }
            } 
            else
            {
                foreach (DataRow row in rows)
                {
                    int linkingIndex = Convert.ToInt32(row ["linking_index"]);
    				
                    if (!m_words.ContainsKey(linkingIndex))
                    {
                        m_words [linkingIndex] = new List<DataRow>();
                    }
    				
                    m_words [linkingIndex].Add(row);
                }
            }

    		
            m_textureParent.transform.localScale = Vector3.zero;
            m_sentenceLabelParent.transform.localScale = Vector3.zero;

            Debug.Log("m_sentences.Count: " + m_sentences.Count);
            Debug.Log("m_words.Count: " + m_sentences.Count);
    	
            if (m_sentences.Count > 0 && m_words.Count > 0)
            {
                StartCoroutine(AskQuestion());
            } else
            {
                Debug.Log("Could not find data, switching scenes");

                OnGameFinish();
            }
        }
	}

	IEnumerator AskQuestion()
	{
		ResetLocalScale();

		foreach(CompleteSentenceWord sentenceWord in m_spawnedSentenceWords)
		{
			Destroy(sentenceWord.gameObject);
		}
		m_spawnedSentenceWords.Clear();
		
		int linkingIndex;
		while(true)
		{
			linkingIndex = UnityEngine.Random.Range(0, m_targetScore + 1); // linkingIndex is one based

			if(m_sentences.ContainsKey(linkingIndex))
			{
				break;
			}
		}

		Debug.Log("linkingIndex: " + linkingIndex);
		Debug.Log("m_imageFolderName: " + m_imageFolderName);
		Debug.Log("imageName: " + m_imageNames[linkingIndex]);
		Debug.Log("Finding: " + m_imageFolderName + m_imageNames[linkingIndex]);
		m_texture.mainTexture = LoaderHelpers.LoadObject<Texture2D>(m_imageFolderName + m_imageNames[linkingIndex]);
		m_imageNames.Remove(linkingIndex);
		
		List<DataRow> rows = m_words[linkingIndex];
		m_words.Remove(linkingIndex);
		List<string> words = new List<string>();
		
		foreach(DataRow row in rows)
		{
			string word = row["word"].ToString();
			
			if(row["is_target_word"].ToString() == "t")
			{
				m_targetWord = word;
				Debug.Log("m_targetWord: " + m_targetWord);
			}
			
			if(!words.Contains(word))
			{
				words.Add(word);
			}
		}
		
		float totalWidth = 0;
		m_currentSentence = m_sentences[linkingIndex];
		m_sentences.Remove(linkingIndex);

		Debug.Log("PrintSize.x: " + m_font.CalculatePrintedSize(m_currentSentence, false, UIFont.SymbolStyle.None).x);

		if((m_font.CalculatePrintedSize(m_currentSentence, false, UIFont.SymbolStyle.None).x) > 2600)
		{
			float scalar = 0.6f;
			m_textPosition.transform.localScale *= scalar;
			m_targetWordBackground.transform.localScale *= scalar;
			Vector3 sentenceBackgroundScale = m_sentenceBackground.transform.localScale;
			sentenceBackgroundScale.x *= scalar;
			m_sentenceBackground.transform.localScale = sentenceBackgroundScale;
		}

		string[] sentenceWords = m_currentSentence.Split(' ');
		foreach(string word in sentenceWords)
		{
			GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_completeSentenceWordPrefab, m_textPosition);

			CompleteSentenceWord sentenceWord = newWord.GetComponent<CompleteSentenceWord>() as CompleteSentenceWord; 

			sentenceWord.SetUp(word, StringHelpers.Edit(word) == m_targetWord);

			totalWidth += sentenceWord.GetWidth();

			m_spawnedSentenceWords.Add(sentenceWord);
		}

		float width = -totalWidth / 2;
		foreach(CompleteSentenceWord sentenceWord in m_spawnedSentenceWords)
		{
			width += sentenceWord.GetWidth() / 2.0f;
			sentenceWord.transform.localPosition = new Vector3(width, 0, 0);
			width += sentenceWord.GetWidth() / 2.0f;

			if(sentenceWord.GetEditedWord() == m_targetWord)
			{
				m_targetWordBackground.GetComponent<MatchPosition>().SetTransformToMatch(sentenceWord.transform);

				m_targetWordBackground.width = sentenceWord.GetWidth();
			}
		}
		
		m_sentenceBackground.width = (int)totalWidth + 150;
		
		HashSet<string> reorderedWords = new HashSet<string>();

		reorderedWords.Add(m_targetWord);
		
		while(reorderedWords.Count < words.Count)
		{
			reorderedWords.Add(words[UnityEngine.Random.Range(0, words.Count)]);
		}
		
		int index = 0;
		foreach(string word in reorderedWords)
		{
			string editedWord = StringHelpers.Edit(word);
			DraggableLabel newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggableLabelPrefab, 
										m_locators[index].transform).GetComponent<DraggableLabel>();
		
			Debug.Log("editedWord: " + editedWord);

			newDraggable.SetUp(editedWord, null, true);
			m_spawnedDraggables.Add(newDraggable);
			newDraggable.OnRelease += OnDraggableRelease;
			
			++index;

			if(index >= m_locators.Length)
			{
				break;
			}
		}

		//m_texture.MakePixelPerfect();
		m_texture.width = 768;
		m_texture.height = 768;
		iTween.ScaleTo(m_textureParent, Vector3.one * m_imageScale, 0.5f);
		iTween.ScaleTo(m_sentenceLabelParent, Vector3.one / 2, 0.5f);

		Resources.UnloadUnusedAssets();

		yield break;
	}

	void ResetLocalScale()
	{
		m_textPosition.transform.localScale = Vector3.one;
		m_targetWordBackground.transform.localScale = Vector3.one;
		m_sentenceBackground.transform.localScale = Vector3.one;
	}
	
	void OnDraggableRelease(DraggableLabel draggable)
	{
		if(draggable.transform.position.y > m_topBoundary.position.y)
		{
			if(draggable.GetText() == m_targetWord)
			{
				UserStoriesStats.Instance.OnCorrectAnswer(m_score + 1, m_currentSentence);

				StartCoroutine(OnCorrect());
			}
			else
			{
				UserStoriesStats.Instance.OnIncorrectAnswer(m_score + 1, m_currentSentence);

				WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_INCORRECT");
				draggable.TweenToStartPos();
			}
		}
		else
		{
			draggable.TweenToStartPos();
		}
	}
	
	IEnumerator OnCorrect()
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
		iTween.ScaleTo(m_textureParent, Vector3.zero, 0.5f);
		iTween.ScaleTo(m_sentenceLabelParent, Vector3.zero, 0.5f);
		for(int index = 0; index < m_spawnedDraggables.Count; ++index)
		{
			m_spawnedDraggables[index].Off();
		}
		m_spawnedDraggables.Clear();
		
		++m_score;
		m_scoreBar.SetStarsCompleted(m_score);
	    m_scoreBar.SetScore(m_score);

		if(m_score < m_targetScore)
		{
			yield return new WaitForSeconds(0.5f);
			StartCoroutine(AskQuestion());
		}
		else
		{
			yield return new WaitForSeconds(0.5f);

			OnGameFinish();
		}
	}

	void OnGameFinish()
	{
		Debug.Log("CompleteSentenceCoordinator.OnGameFinish()");

		GameManager.Instance.CompleteGame (true, "NewCompleteSentenceEnd");

		/*
		if (GameDataBridge.Instance.GetContentType () == Game.Session.Single) 
		{
			TransitionScreen.Instance.ChangeLevel("NewCompleteSentenceEnd", false);	
		} 
		else 
		{
			GameManager.Instance.CompleteGame();
		}
		*/
	}
	
	string RemoveTargetFromSentence(string sentence, string target)
	{
		target = " " + target + " ";
		return sentence.Replace(target, new string(' ', target.Length));
	}
	
	string[] SeparateSentence(string sentence, string target)
	{
		int targetIndex = sentence.IndexOf(target);
		string preTarget = sentence.Substring(0, targetIndex);
		
		int endOfTargetIndex = targetIndex + target.Length;
		string postTarget = sentence.Substring(endOfTargetIndex, sentence.Length - endOfTargetIndex);
		
		return new string[2] {preTarget, postTarget};
	}
}
