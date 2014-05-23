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
    private AudioSource m_audioSource;
    [SerializeField]
    private UIPanel m_picturePanel;
    [SerializeField]
    private UITexture m_pictureTexture;
    
    int m_score;
    
    List<DataRow> m_wordsPool = new List<DataRow>();

    DataRow m_currentWord;
    Dictionary<int, DataRow> m_currentPhonemes = new Dictionary<int, DataRow>();
    
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

        if(m_targetScore > m_wordsPool.Count)
        {
            m_targetScore = m_wordsPool.Count;
        }
        
        //m_scoreBar.SetStarsTarget(m_targetScore);
        m_scoreKeeper.SetTargetScore(m_targetScore);

        
        yield return new WaitForSeconds(0.5f);
        
        if(m_wordsPool.Count > 0)
        {
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

        SetPicture();
  
        string[] phonemeIds = m_currentWord["ordered_phonemes"].ToString().Replace("[", "").Replace("]", "").Split(',');

        for(int i = 0; i < phonemeIds.Length; ++i)
        {
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where id='" + phonemeIds[i] + "'");

            if(dt.Rows.Count > 0)
            {
                m_currentPhonemes.Add(i, dt.Rows[0]);
            }
        }
        
        m_targetCorrectLetters = m_currentPhonemes.Count;
        
        SpellingPadBehaviour.Instance.DisplayNewWord(m_currentWord["word"].ToString());
        
        List<Transform> locators = m_locators.ToList();
        
        for(int i = 0; i < m_currentPhonemes.Count; ++i)
        {
            int locatorIndex = Random.Range(0, locators.Count);
            GameWidget newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_draggablePrefab, locators[locatorIndex]).GetComponent<GameWidget>() as GameWidget;
            
            locators.RemoveAt(locatorIndex);

            newDraggable.SetUp("phonemes", m_currentPhonemes[i], true);

            m_draggables.Add(newDraggable);
            
            newDraggable.onAll += OnDraggableRelease;
        }
        
        SpellingPadBehaviour.Instance.SayWholeWord();
        
        yield return null;
    }

    void SetPicture()
    {
        Texture2D tex = null;
        if(m_currentWord["image"] != null)
        {
            tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + m_currentWord["image"].ToString());
        }
        if(tex == null)
        {
            tex =(Texture2D)Resources.Load("Images/word_images_png_350/_" + m_currentWord["word"].ToString());
        }
        Debug.Log("tex: " + tex);

        m_pictureTexture.mainTexture = tex;
        m_pictureTexture.gameObject.SetActive(tex != null);
        float picturePanelAlpha = tex != null ? 1 : 0;
        TweenAlpha.Begin(m_picturePanel.gameObject, 0.25f, picturePanelAlpha);
    }
    
    IEnumerator CompleteGame ()
    {
        StartCoroutine(m_scoreKeeper.On());

        yield return new WaitForSeconds(2.5f);

        SessionInformation.SetDefaultPlayerVar();
        GameManager.Instance.CompleteGame();
    }
    
    IEnumerator OnQuestionEnd()
    {
        yield return new WaitForSeconds(1f);
        
        foreach(GameWidget draggable in m_draggables)
        {
            draggable.Off();
        }
        m_draggables.Clear();

        m_currentPhonemes.Clear();
        
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
        SpellingPadPhoneme spellingPadPhoneme = SpellingPadBehaviour.Instance.CheckLetters(currentDraggable.labelText, currentDraggable.collider);

        Debug.Log("first attempt: " + spellingPadPhoneme);

        bool foundPhoneme = currentDraggable.data == m_currentPhonemes.First().Value || spellingPadPhoneme != null;

        if(foundPhoneme)
        {
            Debug.Log("Correct");
            m_draggables.Remove(currentDraggable);

            if(spellingPadPhoneme == null)
            {
                spellingPadPhoneme = SpellingPadBehaviour.Instance.GetFirstNonAnsweredPhoneme();

                Debug.Log("second attempt: " + spellingPadPhoneme);
            }

            if(spellingPadPhoneme != null)
            {
                m_currentPhonemes.Remove(spellingPadPhoneme.positionIndex);
                currentDraggable.TweenToPos(spellingPadPhoneme.transform.position);
                spellingPadPhoneme.ChangeState(SpellingPadPhoneme.State.Answered);
            }
            else // Defensive: This should never execute
            {
                Debug.LogError("No spelling pad phoneme!");
                m_currentPhonemes.Remove(m_currentPhonemes.First().Key);
            }

            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
            currentDraggable.TweenToPos(spellingPadPhoneme.transform.position);
            currentDraggable.ChangeBackgroundState();

            currentDraggable.EnableDrag(false);
            currentDraggable.Off();

            
            ++m_correctLetters;
            
            if(m_correctLetters >= m_targetCorrectLetters)
            {
                ++m_score;
                //m_scoreBar.SetStarsCompleted(m_score);
                //m_scoreBar.SetScore(m_score);
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
