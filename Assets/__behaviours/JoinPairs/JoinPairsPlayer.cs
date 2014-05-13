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

    ThrobGUIElement m_currentThrobBehaviour = null;
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

    public void SetUp(int targetScore)
    {
        m_scoreKeeper.SetTargetScore(targetScore);
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

        /*
        List<Vector3> positions = new List<Vector3>();
        Vector3 delta = m_highCorner.transform.localPosition - m_lowCorner.transform.localPosition;
        
        for (int index = 0; index < pairsToShowAtOnce * 2; ++index)
        {
            int x = index % pairsToShowAtOnce;
            int y = index / pairsToShowAtOnce;
            positions.Add(
                m_lowCorner.transform.localPosition +
                new Vector3((delta.x / pairsToShowAtOnce) * (x + 0.5f),
                        (delta.y / 2) * (y + 0.5f), 0)
                + new Vector3(Random.Range(-delta.x / (pairsToShowAtOnce*20.0f), delta.x / (pairsToShowAtOnce*20.0f)),
                          Random.Range(-delta.y / 5, delta.y / 5),
                          0)
                );
        }
        */

        CollectionHelpers.Shuffle(m_locators);

        int locatorIndex = 0;
        foreach(DataRow data in dataPool)
        {
            GameObject newText = SpawningHelpers.InstantiateUnderWithIdentityTransforms(JoinPairsCoordinator.Instance.textPrefab, m_locators[locatorIndex]);
            ++locatorIndex;

            GameObject newImage = SpawningHelpers.InstantiateUnderWithIdentityTransforms(JoinPairsCoordinator.Instance.picturePrefab, m_locators[locatorIndex]);
            ++locatorIndex;

            //GameObject newText = SpawningHelpers.InstantiateUnderWithIdentityTransforms(JoinPairsCoordinator.Instance.textPrefab, m_spawnTransform);
            //GameObject newImage = SpawningHelpers.InstantiateUnderWithIdentityTransforms(JoinPairsCoordinator.Instance.picturePrefab, m_spawnTransform);
            
            //int posIndex = Random.Range(0, positions.Count);
            //Vector3 posA = positions[posIndex];
            //positions.RemoveAt(posIndex);
            
            //newText.transform.localPosition = posA;
            
            //posIndex = Random.Range(0, positions.Count);
            //Vector3 posB = positions[posIndex];
            //positions.RemoveAt(posIndex);
            
            //newImage.transform.localPosition = posB;
            
            Texture2D texture = JoinPairsCoordinator.Instance.GetPicture(data);

            JoinableLineDraw textLineDraw = newText.GetComponent<JoinableLineDraw>() as JoinableLineDraw;
            textLineDraw.SetUp(JoinPairsCoordinator.Instance.dataType, data);
            textLineDraw.SetMaterial(m_lineRendererMaterial);
            textLineDraw.SetColors(m_lineRendererColor, m_lineRendererColor);
            textLineDraw.JoinableJoinEventHandler += OnJoin;
            textLineDraw.JoinablePressEventHandler += OnJoinablePressed;

            JoinableLineDraw imageLineDraw = newImage.GetComponent<JoinableLineDraw>() as JoinableLineDraw;
            imageLineDraw.SetUp(JoinPairsCoordinator.Instance.dataType, data);
            imageLineDraw.SetMaterial(m_lineRendererMaterial);
            imageLineDraw.SetColors(m_lineRendererColor, m_lineRendererColor);
            imageLineDraw.JoinableJoinEventHandler += OnJoin;
            imageLineDraw.JoinablePressEventHandler += OnJoinablePressed;
            
            m_spawnedJoinables.Add(newText);
            m_spawnedJoinables.Add(newImage);
        }
        
        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        
        yield break;
    }

    void OnJoinablePressed(JoinableLineDraw joinable, bool pressed)
    {
        if (pressed)
        {
            if(m_currentThrobBehaviour != null)
            {
                m_currentThrobBehaviour.Off();
            }

            m_currentThrobBehaviour = joinable.GetComponent<ThrobGUIElement>() as ThrobGUIElement;
            m_currentThrobBehaviour.On();

            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEARS");
        } 
        else
        {
            if(m_currentThrobBehaviour != null)
            {
                m_currentThrobBehaviour.Off();
            }

            m_currentThrobBehaviour = null;

            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEARS");
        }
    }

    void OnJoin(JoinableLineDraw a, JoinableLineDraw b)
    {
        if (a != b)
        {
            if (a.isPicture != b.isPicture)
            {
                if (a.data == b.data)
                {
                    //StartCoroutine(SpeakWellDone(a.word));
                    
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
                else
                {
                    //WingroveAudio.WingroveRoot.Instance.PostEvent("NEGATIVE_HIT");
                    //PipPadBehaviour.Instance.Show(a.isPicture ? b.word : a.word);
                    //PipPadBehaviour.Instance.SayAll(1.5f);
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
