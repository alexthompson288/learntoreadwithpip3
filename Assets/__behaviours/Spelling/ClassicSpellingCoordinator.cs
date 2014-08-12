using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using WingroveAudio;
using System.Linq;

public class ClassicSpellingCoordinator : MonoBehaviour
{
    [SerializeField]
    private bool m_useNonsenseWords;
    [SerializeField]
    private int m_numDummyPhonemes;
    [SerializeField]
    private int m_targetScore;
    [SerializeField]
    private GameObject m_draggablePrefab;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_instructions;
	[SerializeField]
	private SpellingPadBehaviour m_spellingPad;

    float m_startTime;
    
    int m_score;
    
    List<DataRow> m_wordsPool = new List<DataRow>();

    DataRow m_currentWord;

    Dictionary<int, string> m_currentLetters = new Dictionary<int, string>();
    
    List<GameWidget> m_draggables = new List<GameWidget>();
    
    int m_targetCorrectLetters = 0;
    int m_correctLetters = 0;
    int m_wrongAnswers = 0;
    
    // Use this for initialization
    IEnumerator Start () 
    {
        m_dataDisplay.SetShowPicture(true);

        // always pip, always winner
        SessionInformation.Instance.SetPlayerIndex(0, 3);
        SessionInformation.Instance.SetWinner(0);
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_wordsPool = m_useNonsenseWords ? DataHelpers.GetSillywords() : DataHelpers.GetWords();

        /*
        #if UNITY_EDITOR
        DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE word='ping-pong'");
        if(dt.Rows.Count > 0)
        {
            m_wordsPool = dt.Rows;
        }
        #endif
        */

        m_wordsPool = DataHelpers.OnlyAlphaChars(m_wordsPool);


        if(m_targetScore > m_wordsPool.Count)
        {
            m_targetScore = m_wordsPool.Count;
        }
        
        m_scoreKeeper.SetTargetScore(m_targetScore);

        
        yield return new WaitForSeconds(0.5f);

        //D.Log("wordPool.Count: " + m_wordsPool.Count);
        
        if(m_wordsPool.Count > 0)
        {
            m_audioSource.clip = m_instructions;
            m_audioSource.Play();

            //D.Log("Playing");

            while(m_audioSource.isPlaying)
            {
                yield return null;
            }

            //D.Log("Played");

            m_startTime = Time.time;
            StartCoroutine(SpawnQuestion());
        }
        else
        {
            StartCoroutine(CompleteGame());
        }
    }
    
    public void SpeakCurrentWord()
    {
        AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(m_currentWord["word"].ToString());
        if(loadedAudio != null)
        {
            GetComponent<AudioSource>().clip = loadedAudio;
            GetComponent<AudioSource>().Play();
        }
    }
    
    IEnumerator SpawnQuestion ()
    {
        m_currentWord = m_wordsPool[Random.Range(0, m_wordsPool.Count)];

        m_wordsPool.Remove(m_currentWord);

        m_dataDisplay.On("words", m_currentWord);
  
        string word = m_currentWord ["word"].ToString().ToLower();
        //string word = StringHelpers.Edit(m_currentWord ["word"].ToString()).ToLower();

        for (int i = 0; i < word.Length; ++i)
        {
            m_currentLetters.Add(i, word[i].ToString());
        }
        
        m_targetCorrectLetters = word.Length;
        
        m_spellingPad.DisplayNewWord(word);
        
        List<Transform> locators = m_locators.ToList();
        
        for(int i = 0; i < m_currentLetters.Count; ++i)
        {
            int locatorIndex = Random.Range(0, locators.Count);
            GameWidget newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, locators[locatorIndex]).GetComponent<GameWidget>() as GameWidget;
            
            locators.RemoveAt(locatorIndex);

            newDraggable.SetUp(m_currentLetters[i], false);
            newDraggable.EnableDrag(false);
            newDraggable.Unpressing += OnDraggableRelease;
            m_draggables.Add(newDraggable);
        }
        
        m_spellingPad.SayWholeWord();

        yield return null;
    }
    
    IEnumerator CompleteGame ()
    {
        //D.Log("CompleteGame()");

        float timeTaken = Time.time - m_startTime;
        
        float twoStarPerQuestion = 7;
        float threeStarPerQuestion = 4.5f;
        
        int stars = ScoreInfo.CalculateTimeStars(timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);
        
        ScoreInfo.Instance.NewScore(timeTaken, m_score, m_targetScore, stars);

        //D.Log("WAIT FOR SCOREKEEPER");

        yield return StartCoroutine(m_scoreKeeper.Celebrate());

        //D.Log("SET DEFAULT");

        SessionInformation.SetDefaultPlayerVar();

        //D.Log("COMPLETE");

        GameManager.Instance.CompleteGame();
    }
    
    IEnumerator OnQuestionEnd()
    {
        m_spellingPad.SayWholeWord();

        yield return new WaitForSeconds(0.8f);

        m_dataDisplay.Off();
        
        foreach(GameWidget draggable in m_draggables)
        {
            draggable.Off();
        }
        m_draggables.Clear();

        m_currentLetters.Clear();
        
        m_correctLetters = 0;
        m_wrongAnswers = 0;
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");
        
        yield return new WaitForSeconds(0.5f);
        
        if(m_score < m_targetScore)
        {
            StartCoroutine(SpawnQuestion());
        }
        else
        {
            //D.Log("CALL COMPLETEGAME");
            StartCoroutine(CompleteGame());
        }
    }
    
    void OnDraggableRelease(GameWidget currentDraggable)
    {
        currentDraggable.ChangeBackgroundState(true);
        WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_MUSHROOM");

        PadLetter padLetter = m_spellingPad.CheckLetters(currentDraggable.labelText, currentDraggable.collider);

        ////D.Log("first attempt: " + padLetter);

        int firstLetterIndex = int.MaxValue;
        foreach(KeyValuePair<int, string> kvp in m_currentLetters)
        {
            if(kvp.Key < firstLetterIndex)
            {
                firstLetterIndex = kvp.Key;
            }
        }

        bool foundPhoneme = currentDraggable.labelText == m_currentLetters[firstLetterIndex] || padLetter != null;

        if(foundPhoneme)
        {
            ////D.Log("Correct");
            m_draggables.Remove(currentDraggable);

            foreach(GameWidget draggable in m_draggables)
            {
                draggable.TintWhite();
                draggable.ChangeBackgroundState(false);
            }

            if(padLetter == null)
            {
                padLetter = m_spellingPad.GetFirstUnansweredPhoneme();
                ////D.Log("second attempt: " + padLetter);
            }

            if(padLetter != null)
            {
                m_currentLetters.Remove(padLetter.GetPositionIndex()); 
                currentDraggable.TweenToPos(padLetter.transform.position);
                padLetter.ChangeState(PadLetter.State.Answered);
            }
            else // Defensive: This should never execute
            {
                //D.LogError("No spelling pad phoneme!");
                m_currentLetters.Remove(firstLetterIndex);
            }

            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
            currentDraggable.TweenToPos(padLetter.transform.position);
            currentDraggable.ChangeBackgroundState(true);

            currentDraggable.EnableDrag(false);
            currentDraggable.Off();

            
            ++m_correctLetters;
            
            if(m_correctLetters >= m_targetCorrectLetters)
            {
                ++m_score;
                m_scoreKeeper.UpdateScore();

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
            ////D.Log("Incorrect");
            
            SpeakCurrentWord();
            
            currentDraggable.TweenToStartPos();
            currentDraggable.TintGray();

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
    }
}
