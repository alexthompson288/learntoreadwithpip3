using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class PlusSpellingPlayer : GamePlayer
{
    [SerializeField]
    private PlusScoreKeeper m_scoreKeeper;
    [SerializeField]
    private TrafficLights m_trafficLights;
    [SerializeField]
    private SpellingPadBehaviour m_spellingPad;
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private DataDisplay m_dataDisplay;

    DataRow m_currentData;

    Dictionary<int, string> m_currentLetters = new Dictionary<int, string>();
    
    List<GameWidget> m_spawnedDraggables = new List<GameWidget>();
    
    int m_targetCorrectLetters = 0;
    int m_correctLetters = 0;
    int m_wrongAnswers = 0;
    
    public void SetCurrentData(DataRow myCurrentData)
    {
        m_currentData = myCurrentData;
    }
    
    public IEnumerator PlayTrafficLights()
    {
        yield return StartCoroutine(m_trafficLights.On());
    }
    
    public override void SelectCharacter(int characterIndex)
    {
        //////D.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        //////D.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        
        m_scoreKeeper.SetCharacterIcon(characterIndex);
        PlusSpellingCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void StartGame()
    {
        m_dataDisplay.SetShowPicture(true);

        m_scoreKeeper.SetHealthLostPerSecond(0.75f);

        m_scoreKeeper.LevelledUp += OnLevelUp;
        m_scoreKeeper.Completed += OnScoreKeeperComplete;
        m_scoreKeeper.StartTimer();
        
        AskQuestion();
    }
    
    void AskQuestion()
    {
        m_dataDisplay.On("words", m_currentData);
        
        string word = m_currentData ["word"].ToString().ToLower();
        //string word = StringHelpers.Edit(m_currentData ["word"].ToString()).ToLower();
        
        for (int i = 0; i < word.Length; ++i)
        {
            m_currentLetters.Add(i, word[i].ToString());
        }
        
        m_targetCorrectLetters = word.Length;
        
        m_spellingPad.DisplayNewWord(word);
        
        CollectionHelpers.Shuffle(m_locators);

        int numToSpawn = Mathf.Min(m_locators.Length, PlusSpellingCoordinator.Instance.GetNumLettersToSpawn());

        GameObject draggablePrefab = PlusSpellingCoordinator.Instance.GetDraggablePrefab();
        
        for(int i = 0; i < numToSpawn; ++i)
        {
            GameWidget newDraggable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(draggablePrefab, m_locators[i]).GetComponent<GameWidget>() as GameWidget;

            string letter = i < m_currentLetters.Count ? m_currentLetters[i] : StringHelpers.GetRandomLetter().ToString();

            newDraggable.SetUp(letter, false);
            newDraggable.EnableDrag(false);
            newDraggable.Unpressing += OnDraggableRelease;
            m_spawnedDraggables.Add(newDraggable);
        }

        if (m_playerIndex == 0 && PlusSpellingCoordinator.Instance.CanPlayAudio())
        {
            m_spellingPad.SayWholeWord();
        }
    }

    void OnDraggableRelease(GameWidget currentDraggable)
    {
        currentDraggable.ChangeBackgroundState(true);
        WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_MUSHROOM");
        
        PadLetter padLetter = m_spellingPad.CheckLetters(currentDraggable.labelText, currentDraggable.collider);

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
            m_spawnedDraggables.Remove(currentDraggable);
            
            foreach(GameWidget draggable in m_spawnedDraggables)
            {
                draggable.TintWhite();
                draggable.ChangeBackgroundState(false);
            }
            
            if(padLetter == null)
            {
                padLetter = m_spellingPad.GetFirstUnansweredPhoneme();
            }
            
            if(padLetter != null)
            {
                m_currentLetters.Remove(padLetter.GetPositionIndex()); 
                currentDraggable.TweenToPos(padLetter.transform.position);
                padLetter.ChangeState(PadLetter.State.Answered);
            }
            else // Defensive: This should never execute
            {
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
                m_scoreKeeper.UpdateScore(1);
                PlusSpellingCoordinator.Instance.OnCorrectAnswer(this);
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
            m_scoreKeeper.UpdateScore(-1);

            DataAudio.Instance.PlayShort(m_currentData);
            
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

    public IEnumerator ClearQuestion()
    {
        m_wrongAnswers = 0;
        m_correctLetters = 0;
        m_currentLetters.Clear();
        CollectionHelpers.DestroyObjects(m_spawnedDraggables, true);

        yield return new WaitForSeconds(0.5f);
        
        AskQuestion();
    }

    void OnLevelUp(ScoreKeeper scoreKeeper)
    {
        PlusSpellingCoordinator.Instance.OnLevelUp();
    }

    void OnScoreKeeperComplete(ScoreKeeper scoreKeeper)
    {
        PlusSpellingCoordinator.Instance.CompleteGame();
    }

    public void ClearGame()
    {
        m_spellingPad.Off();
        m_dataDisplay.Off();
        CollectionHelpers.DestroyObjects(m_spawnedDraggables, true);
    }

    public IEnumerator CelebrateVictory()
    {
        if (SessionInformation.Instance.GetNumPlayers() == 2)
        {
            yield return new WaitForSeconds(0.8f);
            WingroveAudio.WingroveRoot.Instance.PostEvent(string.Format("PLAYER_{0}_WIN", m_selectedCharacter));
            CelebrationCoordinator.Instance.DisplayVictoryLabels(m_playerIndex);
            CelebrationCoordinator.Instance.PopCharacter(m_selectedCharacter, true);
            yield return new WaitForSeconds(2f);
        }

        yield return StartCoroutine(m_scoreKeeper.Celebrate());
    }

    public int GetScore()
    {
        return m_scoreKeeper.GetScore();
    }
}
