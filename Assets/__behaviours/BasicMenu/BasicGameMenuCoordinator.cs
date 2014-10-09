using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicGameMenuCoordinator : Singleton<BasicGameMenuCoordinator> 
{
    [SerializeField]
    private UILabel m_titleLabel;
    [SerializeField]
    private PipColorWidgets m_colorBehaviour;
    [SerializeField]
    private Transform[] m_voyageRows;
    [SerializeField]
    private UIAtlas m_sessionUnlockedAtlas;
    [SerializeField]
    private UIAtlas m_sessionLockedAtlas;
    [SerializeField]
    private ProgressGameButton[] m_progressGames;
    [SerializeField]
    private TweenBehaviour m_numPlayersMoveable;
    [SerializeField]
    private EventRelay[] m_numPlayersButtons;
    [SerializeField]
    private PipColorWidgets m_2PlayerButtonColorBehaviour;
    [SerializeField]
    private GameObject m_onlyiOSLabel;

    List<VoyageButton> m_voyageButtons = new List<VoyageButton>();

    ColorInfo.PipColor m_pipColor;

    ProgressGameButton m_currentProgressGame;

    void Awake()
    {
        m_numPlayersMoveable.gameObject.SetActive(true);

        System.Array.Sort(m_progressGames, CollectionHelpers.LocalLeftToRight_TopToBottom);
        System.Array.Sort(m_voyageRows, CollectionHelpers.LocalTopToBottom);

#if UNITY_STANDALONE
        System.Array.Sort(m_numPlayersButtons, CollectionHelpers.LocalTopToBottom);
        
        for(int i = 0; i < m_numPlayersButtons.Length; ++i)
        {
            if(i == 0)
            {
                m_numPlayersButtons[i].SingleClicked += OnClickChooseNumPlayers;
            }
            else
            {
                UISprite[] buttonSprites = m_numPlayersButtons[i].GetComponentsInChildren<UISprite>() as UISprite[];
                foreach(UISprite sprite in buttonSprites)
                {
                    sprite.color = Color.grey;
                }
            }

#if UNITY_EDITOR
            m_numPlayersButtons[i].SingleClicked += OnClickChooseNumPlayers;
#endif
        }
#else
        foreach (EventRelay relay in m_numPlayersButtons)
        {
            relay.SingleClicked += OnClickChooseNumPlayers;
        }
#endif

        foreach (Transform row in m_voyageRows)
        {
            VoyageButton[] buttons = row.GetComponentsInChildren<VoyageButton>() as VoyageButton[];
            System.Array.Sort(buttons, CollectionHelpers.LocalLeftToRight);
            m_voyageButtons.AddRange(buttons);
        }
    }

    public void On(ColorInfo.PipColor myPipColor, bool isMaths)
    {
        m_pipColor = myPipColor;
        m_colorBehaviour.SetPipColor(m_pipColor);

        string programmeName = isMaths ? ProgrammeInfo.basicMaths : ProgrammeInfo.basicReading;
        GameManager.Instance.SetProgramme(programmeName);

        if (m_titleLabel != null)
        {
            string programme = isMaths ? "Maths" : "Reading";
            m_titleLabel.text = string.Format("{0} - {1}", programme, m_pipColor.ToString());
        }

        Color col = ColorInfo.GetColor(m_pipColor);

        // Set up VoyageButtons
        List<DataRow> sessions = GameDataBridge.Instance.GetDatabase().ExecuteQuery(
            "select * from programsessions WHERE programmodule_id=" + DataHelpers.GetModule(m_pipColor).GetId() + " ORDER BY number").Rows;
        
        for (int i = 0; i < m_voyageButtons.Count; ++i)
        {
            DataRow session = i < sessions.Count ? sessions[i] : null;
            m_voyageButtons[i].SetUp(session, col);
        }

        // Set up ProgressButtons
        List<string> gameNames = GameColorLinker.Instance.GetGameNames(m_pipColor);
        for(int i = 0; i < m_progressGames.Length; ++i)
        {
            bool hasGame = i < gameNames.Count;
            
            m_progressGames[i].gameObject.SetActive(hasGame);
            
            if(hasGame)
            {
                m_progressGames[i].SetUp(gameNames[i], m_pipColor);
            }
        }
    }

    int m_minimumDataCount = 6;
    
    void AddExtraData(List<DataRow> dataPool, List<DataRow> extraDataPool)
    {
        if (extraDataPool.Count > 0)
        {
            int safetyCounter = 0;
            while (dataPool.Count < m_minimumDataCount && safetyCounter < 100)
            {
                DataRow data = extraDataPool [Random.Range(0, extraDataPool.Count)];
                if (!dataPool.Contains(data))
                {
                    dataPool.Add(data);
                }
                
                ++safetyCounter;
            }
        }
    }

    public void OnClickProgressGame(ProgressGameButton myCurrentProgressGame)
    {
        m_currentProgressGame = myCurrentProgressGame;
        m_numPlayersMoveable.On();
    }

    void OnClickChooseNumPlayers(EventRelay relay)
    {
        GameManager.Instance.Reset();

        GameManager.Instance.SetActivity(ProgrammeInfo.progress);

        PlusScoreInfo.Instance.SetScoreType(m_pipColor.ToString());
        ScoreInfo.Instance.SetScoreType(m_pipColor.ToString());

        int numPlayers = System.Array.IndexOf(m_numPlayersButtons, relay) + 1;
        numPlayers = Mathf.Max(numPlayers, 1);
        SessionInformation.Instance.SetNumPlayers(numPlayers);

        GameManager.Instance.AddGame(m_currentProgressGame.GetGameName());
        
        if(GameManager.Instance.programme == ProgrammeInfo.basicMaths)
        {
            DataSetters.AddModuleNumbers(m_pipColor);
        }
        else
        {
            int moduleId = DataHelpers.GetModuleId(m_pipColor);
            
            GameManager.Instance.AddData("phonemes", DataHelpers.GetModulePhonemes(moduleId));
            GameManager.Instance.AddData("words", DataHelpers.GetModuleWords(moduleId));
            GameManager.Instance.AddData("keywords", DataHelpers.GetModuleKeywords(moduleId));
            GameManager.Instance.AddData("sillywords", DataHelpers.GetModuleSillywords(moduleId));
            
            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
            GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
            GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));
        }
        
        StartGames();
    }

    public void OnClickVoyageButton(DataRow session)
    {
        GameManager.Instance.Reset();
        ScoreInfo.Instance.SetScoreType(session.GetId().ToString());

        VoyageInfo.Instance.SetUp(session.GetId());

        List<DataRow> sections = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sections WHERE programsession_id=" + session.GetId()).Rows;
        
        if (sections.Count > 0)
        {
            GameManager.Instance.SetActivity(ProgrammeInfo.voyage);

            foreach (DataRow section in sections)
            {
                DataRow game = DataHelpers.GetGameForSection(section);
                
                if (game != null)
                {
                    GameManager.Instance.AddGame(game);
                }
            }
            
            GameManager.Instance.AddGame("NewSessionComplete");

            // Set data
            SqliteDatabase db = GameDataBridge.Instance.GetDatabase(); // Locally store the database because we're going to call it a lot
            
            if (GameManager.Instance.programme == ProgrammeInfo.basicReading)
            {
                int previousModuleId = DataHelpers.GetPreviousModuleId(m_pipColor);
                
                // Phonemes
                DataTable dt = db.ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE programsession_id=" + session.GetId());
                if (dt.Rows.Count > 0)
                {
                    List<DataRow> phonemePool = dt.Rows;
                    
                    if (phonemePool.Count < m_minimumDataCount)
                    {
                        
                        int extraModuleId = previousModuleId;
                        
                        while (phonemePool.Count < m_minimumDataCount && extraModuleId > 0)
                        {
                            AddExtraData(phonemePool, DataHelpers.GetModulePhonemes(extraModuleId));
                            --extraModuleId;
                        }
                    }
                    
                    
                    GameManager.Instance.AddData("phonemes", phonemePool);
                    GameManager.Instance.AddTargetData("phonemes", phonemePool.FindAll(x => x ["is_target_phoneme"] != null && x ["is_target_phoneme"].ToString() == "t"));
                }
                
                // Words/Keywords
                dt = db.ExecuteQuery("select * from data_words INNER JOIN words ON word_id=words.id WHERE programsession_id=" + session.GetId());
                if (dt.Rows.Count > 0)
                {
                    // Words
                    List<DataRow> words = dt.Rows.FindAll(word => (word ["tricky"] == null || word ["tricky"].ToString() == "f") 
                                                          && (word ["nondecodeable"] == null || word ["nondecodeable"].ToString() == "f"));
                    
                    if (words.Count < m_minimumDataCount)
                    {
                        int extraModuleId = previousModuleId;
                        
                        while (words.Count < m_minimumDataCount && extraModuleId > 0)
                        {
                            AddExtraData(words, DataHelpers.GetModuleWords(extraModuleId));
                            --extraModuleId;
                        }
                    }
                    
                    GameManager.Instance.AddData("words", words);
                    GameManager.Instance.AddTargetData("words", words.FindAll(x => x ["is_target_word"] != null && x ["is_target_word"].ToString() == "t"));
                    
                    // Keywords
                    List<DataRow> keywords = dt.Rows.FindAll(word => (word ["tricky"] != null && word ["tricky"].ToString() == "t") 
                                                             || (word ["nondecodeable"] != null && word ["nondecodeable"].ToString() == "t"));
                    
                    if (keywords.Count < m_minimumDataCount)
                    {
                        int extraModuleId = previousModuleId;
                        
                        while (keywords.Count < m_minimumDataCount && extraModuleId > 0)
                        {
                            AddExtraData(keywords, DataHelpers.GetModuleKeywords(extraModuleId));
                            --extraModuleId;
                        }
                    }
                    
                    GameManager.Instance.AddData("keywords", keywords);
                    GameManager.Instance.AddTargetData("keywords", keywords.FindAll(x => x ["is_target_word"] != null && x ["is_target_word"].ToString() == "t"));
                }
                
                // Quiz Questions and Captions
                dt = db.ExecuteQuery("select * from datasentences WHERE programsession_id=" + session.GetId());
                
                GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
                GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));
            } 
            else
            {
                DataTable sessionsTable = db.ExecuteQuery("select * from programsessions WHERE id=" + session.GetId());
                
                if (sessionsTable.Rows.Count > 0)
                {
                    int highestNumber = sessionsTable.Rows [0] ["highest_number"] != null ? sessionsTable.Rows [0].GetInt("highest_number") : 10;
                    GameManager.Instance.AddData("numbers", DataHelpers.CreateNumbers(1, highestNumber));
                }
            }
            
            StartGames();
        }
    }

    public void StartGames()
    {
        GameManager.Instance.SetPipColor(m_pipColor);
        GameManager.Instance.SetReturnScene(Application.loadedLevelName);

        WingroveAudio.WingroveRoot.Instance.PostEvent("MUSIC_STOP");
        
        GameManager.Instance.StartGames();
    }
    
    public UIAtlas sessionUnlockedAtlas
    {
        get
        {
            return m_sessionUnlockedAtlas;
        }
    }
    
    public UIAtlas sessionLockedAtlas
    {
        get
        {
            return m_sessionLockedAtlas;
        }
    }
}