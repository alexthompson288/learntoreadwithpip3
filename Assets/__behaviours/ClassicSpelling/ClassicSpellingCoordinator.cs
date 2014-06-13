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
    private ProgressScoreBar m_scoreBar;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private DataDisplay m_dataDisplay;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_instructions;

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
        // always pip, always winner
        SessionInformation.Instance.SetPlayerIndex(0, 3);
        SessionInformation.Instance.SetWinner(0);
        
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_wordsPool = m_useNonsenseWords ? DataHelpers.GetSillywords() : DataHelpers.GetWords();

        // TODO: Delete. Only used for debugging tricky words
#if UNITY_EDITOR
        //m_wordsPool = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from words WHERE tricky='t'").Rows;
#endif

        if(m_targetScore > m_wordsPool.Count)
        {
            m_targetScore = m_wordsPool.Count;
        }
        
        m_scoreKeeper.SetTargetScore(m_targetScore);

        
        yield return new WaitForSeconds(0.5f);
        
        if(m_wordsPool.Count > 0)
        {
            m_audioSource.clip = m_instructions;
            m_audioSource.Play();

            while(m_audioSource.isPlaying)
            {
                yield return null;
            }

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
  
        string word = m_currentWord ["word"].ToString();

        for (int i = 0; i < word.Length; ++i)
        {
            m_currentLetters.Add(i, word[i].ToString());
        }
        
        m_targetCorrectLetters = word.Length;
        
        SpellingPadBehaviour.Instance.DisplayNewWord(word);
        
        List<Transform> locators = m_locators.ToList();
        
        for(int i = 0; i < m_currentLetters.Count; ++i)
        {
            int locatorIndex = Random.Range(0, locators.Count);
            GameWidget newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, locators[locatorIndex]).GetComponent<GameWidget>() as GameWidget;
            
            locators.RemoveAt(locatorIndex);

            newDraggable.SetUp(m_currentLetters[i], false);

            m_draggables.Add(newDraggable);
            
            newDraggable.onAll += OnDraggableRelease;
        }
        
        SpellingPadBehaviour.Instance.SayWholeWord();

        yield return null;
    }
    
    IEnumerator CompleteGame ()
    {
        float timeTaken = Time.time - m_startTime;
        
        float twoStarPerQuestion = 7;
        float threeStarPerQuestion = 4.5f;
        
        Debug.Log("timeTaken: " + timeTaken);
        Debug.Log("targetScore: " + m_targetScore);
        Debug.Log("twoStar: " + twoStarPerQuestion * (float)m_targetScore);
        Debug.Log("threeStar: " + threeStarPerQuestion * (float)m_targetScore);
        
        ScoreInfo.Instance.NewScore(m_score, m_targetScore, timeTaken, twoStarPerQuestion * (float)m_targetScore, threeStarPerQuestion * (float)m_targetScore);

        yield return StartCoroutine(m_scoreKeeper.On());

        SessionInformation.SetDefaultPlayerVar();
        GameManager.Instance.CompleteGame();
    }
    
    IEnumerator OnQuestionEnd()
    {
        SpellingPadBehaviour.Instance.SayWholeWord();

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
            StartCoroutine(CompleteGame());
        }
    }
    
    void OnDraggableRelease(GameWidget currentDraggable)
    {
        PadLetter padLetter = SpellingPadBehaviour.Instance.CheckLetters(currentDraggable.labelText, currentDraggable.collider);

        //Debug.Log("first attempt: " + padLetter);

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
            //Debug.Log("Correct");
            m_draggables.Remove(currentDraggable);

            foreach(GameWidget draggable in m_draggables)
            {
                draggable.TintDefault();
                draggable.ChangeBackgroundState(false);
            }

            if(padLetter == null)
            {
                padLetter = SpellingPadBehaviour.Instance.GetFirstUnansweredPhoneme();
                //Debug.Log("second attempt: " + padLetter);
            }

            if(padLetter != null)
            {
                m_currentLetters.Remove(padLetter.GetPositionIndex()); 
                currentDraggable.TweenToPos(padLetter.transform.position);
                padLetter.ChangeState(PadLetter.State.Answered);
            }
            else // Defensive: This should never execute
            {
                Debug.LogError("No spelling pad phoneme!");
                m_currentLetters.Remove(firstLetterIndex);
            }

            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
            currentDraggable.TweenToPos(padLetter.transform.position);
            currentDraggable.ChangeBackgroundState();

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
                    SpellingPadBehaviour.Instance.SayShowSequential();
                }
            }
        }
        else
        {
            //Debug.Log("Incorrect");
            
            SpeakCurrentWord();
            
            currentDraggable.TweenToStartPos();
            currentDraggable.TintIncorrect();
            
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
