using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class JoinPairsPlayer : GamePlayer
{
    [SerializeField]
    private int m_playerIndex;
    [SerializeField]
    private ScoreKeeper m_scoreKeeper;
    [SerializeField]
    private Transform m_lowCorner;
    [SerializeField]
    private Transform m_highCorner;
    [SerializeField]
    private Transform m_spawnTransform;
    [SerializeField]
    private Transform m_leftOff;
    [SerializeField]
    private Transform m_rightOff;
    [SerializeField]
    private CharacterPopper m_characterPopper;
    [SerializeField]
    private Material m_lineRendererMaterial;
    [SerializeField]
    private Color m_lineRendererColor;
    [SerializeField]
    private Transform[] m_locators;

    GameObject m_firstPrefab;
    GameObject m_secondPrefab;

    JoinableLineDraw m_currentJoinable = null;

    int m_panelDepthIncrement = 1;

    int m_score;
    public int score
    {
        get
        {
            return m_score;
        }
    }

    List<GameObject> m_spawnedJoinables = new List<GameObject>();

    List<CharacterSelection> m_characterSelections = new List<CharacterSelection>();
    [SerializeField] private int m_selectedCharacter = -1;

    public override void RegisterCharacterSelection(CharacterSelection characterSelection)
    {
        m_characterSelections.Add(characterSelection);
    }
    
    public override void SelectCharacter(int characterIndex)
    {
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        JoinPairsCoordinator.Instance.CharacterSelected(characterIndex);
    }

    public void HideCharacter(int index)
    {
        foreach (CharacterSelection cs in m_characterSelections)
        {
            if (cs.GetCharacterIndex() == index)
            {
                cs.DeactivatePress(false);
            }
        }
    }
    
    public void HideAll()
    {
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(true);
        }
    }
    
    public bool HasSelectedCharacter()
    {
        return (m_selectedCharacter != -1);
    }

    public void SetUp(int targetScore, string dataType)
    {
        Debug.Log("JoinPairsPlayer.SetUp(): " + dataType);

        m_scoreKeeper.SetTargetScore(targetScore);

        if (dataType == "numbers")
        {
            m_firstPrefab = JoinPairsCoordinator.Instance.numberPrefab;
            m_secondPrefab = JoinPairsCoordinator.Instance.numberPrefab;
        } 
        else if (dataType == "shapes")
        {
            m_firstPrefab = JoinPairsCoordinator.Instance.picturePrefab;
            m_secondPrefab = JoinPairsCoordinator.Instance.picturePrefab;
        } 
        else
        {
            m_firstPrefab = JoinPairsCoordinator.Instance.picturePrefab;
            m_secondPrefab = JoinPairsCoordinator.Instance.textPrefab;
            Debug.Log("m_firstPrefab: " + m_firstPrefab.name);
            Debug.Log("m_secondPrefab: " + m_secondPrefab.name);

        }
    }

    public IEnumerator SetUpNext()
    {
        m_panelDepthIncrement = 1;
        
        Dictionary<string, DataRow> letters = new Dictionary<string, DataRow>();
        HashSet<DataRow> dataPool = new HashSet<DataRow>();
        
        int pairsToShowAtOnce = JoinPairsCoordinator.Instance.GetPairsToShowAtOnce();

        if (pairsToShowAtOnce > (m_locators.Length / 2))
        {
            pairsToShowAtOnce = m_locators.Length / 2;
        }
        
        while (dataPool.Count < pairsToShowAtOnce)
        {
            DataRow data = JoinPairsCoordinator.Instance.dataPool[Random.Range(0, JoinPairsCoordinator.Instance.dataPool.Count)];
            if(data != null)
            {
                dataPool.Add(data);
            }
            yield return null;
        }

        CollectionHelpers.Shuffle(m_locators);

        int locatorIndex = 0;
        foreach(DataRow data in dataPool)
        {
            JoinableLineDraw firstLineDraw = SpawnLineDraw(m_firstPrefab, data, locatorIndex);
            ++locatorIndex;

            if(JoinPairsCoordinator.Instance.dataType == "words" && SessionInformation.Instance.GetNumPlayers() == 1)
            {
                firstLineDraw.JoinableClickEventHandler += OnJoinableClicked;
            }


            SpawnLineDraw(m_secondPrefab, data, locatorIndex);
            ++locatorIndex;
        }
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        
        yield break;
    }

    JoinableLineDraw SpawnLineDraw(GameObject lineDrawPrefab, DataRow data, int locatorIndex)
    {
        GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(lineDrawPrefab, m_locators[locatorIndex]);

        m_spawnedJoinables.Add(newGo);

        JoinableLineDraw lineDraw = newGo.GetComponent<JoinableLineDraw>() as JoinableLineDraw;
        lineDraw.SetUp(JoinPairsCoordinator.Instance.dataType, data);
        lineDraw.SetMaterial(m_lineRendererMaterial);
        lineDraw.SetColors(m_lineRendererColor, m_lineRendererColor);
        lineDraw.JoinableJoinEventHandler += OnJoin;
        lineDraw.JoinablePressEventHandler += OnJoinablePressed;

        return lineDraw;
    }

    void OnJoinableClicked(JoinableLineDraw joinable)
    {
        JoinPairsCoordinator.Instance.PlayShortAudio(joinable.data);
    }

    void OnJoinablePressed(JoinableLineDraw joinable, bool pressed)
    {
        if (pressed)
        {
            m_currentJoinable = joinable;
            m_currentJoinable.Tint(Color.gray);
        } 
        else
        {
            m_currentJoinable.Tint(Color.white);
            m_currentJoinable = null;
        }
    }

    void OnJoin(JoinableLineDraw a, JoinableLineDraw b)
    {
        if (a != b)
        {
            if (a.joinableType != b.joinableType || JoinPairsCoordinator.Instance.AreJoinablesSameType()) // shapes are both pictures
            {
                if (a.data == b.data)
                {
                    //StartCoroutine(SpeakWellDone(a.word));
                    if(SessionInformation.Instance.GetNumPlayers() == 1)
                    {
                        DataRow audioData = a.joinableType == JoinableLineDraw.JoinableType.Picture ? b.data : a.data;
                        JoinPairsCoordinator.Instance.PlayShortAudio(audioData);
                    }
                    
                    a.TransitionOff(a.transform.position.x < b.transform.position.x ? m_leftOff : m_rightOff);
                    b.TransitionOff(a.transform.position.x < b.transform.position.x ? m_rightOff : m_leftOff);
                    m_spawnedJoinables.Remove(a.gameObject);
                    m_spawnedJoinables.Remove(b.gameObject);

                    WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
                    
                    if (m_spawnedJoinables.Count == 0)
                    {                        
                        StartCoroutine(AddPoint());
                    }
                }
                else if(SessionInformation.Instance.GetNumPlayers() == 1 && JoinPairsCoordinator.Instance.dataType == "words")
                {
                    WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
                    PipPadBehaviour.Instance.Show(a.joinableType == JoinableLineDraw.JoinableType.Picture ? b.data["word"].ToString() : a.data["word"].ToString());
                    PipPadBehaviour.Instance.SayAll(1.5f);
                }
            }
        }
    }

    IEnumerator AddPoint()
    {
        m_spawnedJoinables.Clear();
        
        yield return new WaitForSeconds(2.0f);
        
        m_characterPopper.PopCharacter();
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");
        
        m_score++;
        m_scoreKeeper.UpdateScore();
        
        if (m_score == JoinPairsCoordinator.Instance.targetScore)
        {
            JoinPairsCoordinator.Instance.IncrementNumFinishedPlayers();
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(SetUpNext());
        }
    }

    public void DestroyJoinables()
    {
        foreach(GameObject joinable in m_spawnedJoinables)
        {
            if(joinable != null)
            {
                joinable.GetComponent<JoinableLineDraw>().DestroyJoinable();
            }
        }
    }

    public IEnumerator OnWin()
    {
        yield return StartCoroutine(m_scoreKeeper.On());
    }

    void PlayJoinableShortAudio(JoinableLineDraw joinable)
    {
        JoinPairsCoordinator.Instance.PlayShortAudio(joinable.data);
    }

    void PlayJoinableLongAudio(JoinableLineDraw joinable)
    {
        JoinPairsCoordinator.Instance.PlayLongAudio(joinable.data);
    }
}
