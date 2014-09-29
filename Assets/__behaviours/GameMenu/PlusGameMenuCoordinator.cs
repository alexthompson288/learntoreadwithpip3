using UnityEngine;
using System.Collections;

public class PlusGameMenuCoordinator : Singleton<PlusGameMenuCoordinator> 
{
    [SerializeField]
    private GameObject m_camera;
    [SerializeField]
    private EventRelay m_readingButton;
    [SerializeField]
    private EventRelay m_mathsButton;
    [SerializeField]
    private EventRelay[] m_switchButtons;
    [SerializeField]
    private Transform m_readingParent;
    [SerializeField]
    private Transform m_mathsParent;
    [SerializeField]
    private float m_cameraTweenDuration = 0.3f;
    [SerializeField]
    private PlusGame[] m_mathsGames;
    [SerializeField]
    private PlusGame[] m_readingGames;
    [SerializeField]
    private EventRelay[] m_backColliders;
    [SerializeField]
    private TweenBehaviour m_chooseNumPlayersMoveable;
    [SerializeField]
    private EventRelay[] m_chooseNumPlayersButtons;
    [SerializeField]
    private TweenBehaviour m_chooseColorMoveable;
    [SerializeField]
    private EventRelay[] m_chooseColorButtons;
    [SerializeField]
    private SpriteAnim[] m_pipAnims;
    [SerializeField]
    private TweenBehaviour m_allUnlockedMoveable;
    [SerializeField]
    private GameObject[] m_titleLabels;

    ColorInfo.PipColor[] m_colorBands = new ColorInfo.PipColor[] { ColorInfo.PipColor.Turquoise, ColorInfo.PipColor.Purple, ColorInfo.PipColor.Gold, ColorInfo.PipColor.White};

    string m_gameName;
    ColorInfo.PipColor m_pipColor;
    int m_numPlayers;

    bool m_hasClickedGameButton = false;

    string m_scoreType = "plus";
    public string GetScoreType()
    {
        return m_scoreType;
    }

    static Bookmark m_bookmark;
    enum Bookmark
    {
        None,
        Maths,
        Reading
    }

    IEnumerator Start()
    {
        GameManager.Instance.SetProgramme(ProgrammeInfo.plusReading);

        if (m_bookmark == Bookmark.Maths)
        {
            m_camera.transform.position = m_mathsParent.position;
        }
        else if (m_bookmark == Bookmark.Reading)
        {
            m_camera.transform.position = m_readingParent.position;
        }

        m_bookmark = Bookmark.None;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_readingButton.SingleClicked += OnClickMenuButton;
        m_mathsButton.SingleClicked += OnClickMenuButton;

        foreach (EventRelay relay in m_backColliders)
        {
            relay.SingleClicked += OnClickBackCollider;
        }

        foreach (EventRelay relay in m_switchButtons)
        {
            relay.SingleClicked += OnClickSwitchButton;
        }

        foreach (EventRelay relay in m_chooseNumPlayersButtons)
        {
            relay.SingleClicked += OnChooseNumPlayers;
        }

        System.Array.Sort(m_chooseColorButtons, CollectionHelpers.LocalLeftToRight_TopToBottom);

        string[] mathGameNames = ProgrammeInfo.GetPlusMathsGames();
        for(int i = 0; i < m_mathsGames.Length && i < mathGameNames.Length; ++i)
        {
            m_mathsGames[i].GetComponentInChildren<EventRelay>().SingleClicked += OnClickGameButton;
            m_mathsGames[i].SetUp(mathGameNames[i], i);
        }

        string[] readingGameNames = ProgrammeInfo.GetPlusReadingGames();
        for(int i = 0; i < m_readingGames.Length && i < readingGameNames.Length; ++i)
        {
            m_readingGames[i].GetComponentInChildren<EventRelay>().SingleClicked += OnClickGameButton;
            m_readingGames[i].SetUp(readingGameNames[i], i);
        }

        RefreshPurchases();
    }

    void OnClickMenuButton(EventRelay relay)
    {
        bool goToMaths = relay == m_mathsButton;

        Vector3 localPos = m_mathsParent.localPosition;

        float mathsParentLocalPosX = goToMaths ? 0 : -1024;
        localPos.x = mathsParentLocalPosX;
        m_mathsParent.localPosition = localPos;

        localPos.x += 1024;
        m_readingParent.localPosition = localPos;

        Transform target = goToMaths ? m_mathsParent : m_readingParent;

        string programmeName = goToMaths ? ProgrammeInfo.plusMaths : ProgrammeInfo.plusReading;
        GameManager.Instance.SetProgramme(programmeName);

        StartCoroutine(TweenCamera(target.position));
    }

    void OnClickSwitchButton(EventRelay relay)
    {
        bool goToMaths = Mathf.Approximately(m_camera.transform.position.x, m_readingParent.transform.position.x);

        Transform target = goToMaths ? m_mathsParent : m_readingParent;

        string programmeName = goToMaths ? ProgrammeInfo.plusMaths : ProgrammeInfo.plusReading;
        GameManager.Instance.SetProgramme(programmeName);

        StartCoroutine(TweenCamera(target.position));
    }

    IEnumerator TweenCamera(Vector3 targetPos)
    {
        yield return new WaitForSeconds(0.1f);
        iTween.MoveTo(m_camera, targetPos, m_cameraTweenDuration);
    }

    void OnClickGameButton(EventRelay relay)
    {
        D.Log("OnClickGameButton()");

        PlusGame plusGame = relay.GetComponentInParent<PlusGame>() as PlusGame;

        if (!m_hasClickedGameButton)
        {
            ResourcesAudio.Instance.PlayFromResources(plusGame.GetGame()["labeltext"].ToString());

            m_hasClickedGameButton = true;

            if (ContentLock.Instance.IsPlusGameUnlocked(plusGame.GetGame().GetId()))
            {
                ColorInfo.PipColor maxColor = plusGame.GetMaxColor();
                for (int i = 0; i < m_chooseColorButtons.Length && i < m_colorBands.Length; ++i)
                {
                    bool isUnlocked = i == 0 || (int)m_colorBands [i] <= (int)maxColor + 1;
                    
                    m_chooseColorButtons [i].GetComponentInChildren<UISprite>().color = isUnlocked ? ColorInfo.GetColor(m_colorBands [i]) : Color.grey;

#if UNITY_EDITOR
                    m_chooseColorButtons [i].SingleClicked += OnChooseColor;
#else
                    if (isUnlocked)
                    {
                        m_chooseColorButtons [i].SingleClicked += OnChooseColor;
                    }
#endif
                }
                
                m_bookmark = System.Array.IndexOf(m_mathsGames, plusGame) != -1 ? Bookmark.Maths : Bookmark.Reading;
                
                m_gameName = plusGame.GetGame()["name"].ToString();
                
                m_chooseNumPlayersMoveable.On();
            } 
            else if (ContentLock.Instance.lockType == ContentLock.Lock.Login)
            {
                LoginInfo.Instance.SpawnLogin();
            } 
            else
            {
                PurchasePlusGames.Instance.On(plusGame.GetGame());
            }
        }

    }

    void OnChooseColor(EventRelay relay)
    {
        foreach(EventRelay button in m_chooseColorButtons)
        {
            relay.SingleClicked -= OnChooseColor;
        }

        m_pipColor = m_colorBands [System.Array.IndexOf(m_chooseColorButtons, relay)];

        WingroveAudio.WingroveRoot.Instance.PostEvent(string.Format("COLOR_{0}", m_pipColor.ToString().ToUpper()));

        m_chooseColorMoveable.Off();
        StartGame();
    }

    void OnChooseNumPlayers(EventRelay relay)
    {
        int numPlayers = System.Array.IndexOf(m_chooseNumPlayersButtons, relay) + 1;
        SessionInformation.Instance.SetNumPlayers(numPlayers);
        WingroveAudio.WingroveRoot.Instance.PostEvent(string.Format("{0}_PLAYER", numPlayers));
        //m_chooseNumPlayersMoveable.Off();
        m_chooseColorMoveable.On();
    }

    void OnClickBackCollider(EventRelay relay)
    {
        foreach(EventRelay button in m_chooseColorButtons)
        {
            relay.SingleClicked -= OnChooseColor;
        }

        PurchasePlusGames.Instance.Off();
        m_chooseColorMoveable.Off();
        m_chooseNumPlayersMoveable.Off();
        m_hasClickedGameButton = false;
    }

    void StartGame()
    {
        PlusScoreInfo.Instance.SetScoreType(m_scoreType);
        
        GameManager.Instance.SetCurrentColor(m_pipColor);
        D.Log("m_gameName: " + m_gameName);
        GameManager.Instance.AddGame(m_gameName);
        GameManager.Instance.SetReturnScene(Application.loadedLevelName);
        
        bool isMaths = Mathf.Approximately(m_camera.transform.position.x, m_mathsParent.position.x);
        
       
        if(isMaths)
        {
            DataSetters.AddModuleNumbers(m_pipColor);
            DataSetters.AddModuleTimes(m_pipColor);
        }
        else
        {
            int moduleId = DataHelpers.GetModuleId(m_pipColor);

            D.Log("moduleId: " + moduleId);
            
            GameManager.Instance.AddData("phonemes", DataHelpers.GetModulePhonemes(moduleId));
            GameManager.Instance.AddData("words", DataHelpers.GetModuleWords(moduleId));
            GameManager.Instance.AddData("keywords", DataHelpers.GetModuleKeywords(moduleId));
            GameManager.Instance.AddData("sillywords", DataHelpers.GetModuleSillywords(moduleId));
            
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
            //GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
            GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));
        }
        
        GameManager.Instance.StartGames(false);
    }

    public void MakeAllPipsJump()
    {
        foreach (SpriteAnim anim in m_pipAnims)
        {
            anim.PlayAnimation("JUMP");
        }
    }

    public void RefreshPurchases()
    {
        bool allUnlocked = true;

        for (int i = 0; i < m_mathsGames.Length; ++i)
        {
            m_mathsGames[i].RefreshPadlock();

            if(!m_mathsGames[i].IsGameUnlocked())
            {
                allUnlocked = false;
            }
        }

        for (int i = 0; i < m_readingGames.Length; ++i)
        {
            m_readingGames[i].RefreshPadlock();

            if(!m_readingGames[i].IsGameUnlocked())
            {
                allUnlocked = false;
            }
        }



        Hashtable tweenArgs = new Hashtable();

        float posY = allUnlocked ? 262 : 310;
        tweenArgs.Add("position", new Vector3(0, posY, 0));

        tweenArgs.Add("islocal", true);
        tweenArgs.Add("time", 0.2f);

        for (int i = 0; i < m_titleLabels.Length; ++i)
        {
            iTween.MoveTo(m_titleLabels[i], tweenArgs);
        }

        if (allUnlocked)
        {
            m_allUnlockedMoveable.On();
        }
        else
        {
            m_allUnlockedMoveable.Off();
        }
    }
}
