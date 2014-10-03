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

    List<VoyageButton> m_voyageButtons = new List<VoyageButton>();

    ColorInfo.PipColor m_pipColor;

    void Awake()
    {
        System.Array.Sort(m_voyageRows, CollectionHelpers.LocalTopToBottom);
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
            m_titleLabel.text = isMaths ? "Maths" : "Reading";
        }

        // Set up VoyageButtons
        Color col = ColorInfo.GetColor(m_pipColor);

        List<DataRow> sessions = GameDataBridge.Instance.GetDatabase().ExecuteQuery(
            "select * from programsessions WHERE programmodule_id=" + DataHelpers.GetModule(m_pipColor).GetId() + " ORDER BY number").Rows;
        
        for (int i = 0; i < m_voyageButtons.Count; ++i)
        {
            DataRow session = i < sessions.Count ? sessions[i] : null;
            m_voyageButtons[i].SetUp(session, col);
        }


        SetUpGameButtons();
    }

    void SetUpGameButtons()
    {
        
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

    public void OnClickVoyageButton(DataRow session)
    {
        VoyageInfo.Instance.SetCurrentSessionId(session.GetId());

        List<DataRow> sections = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from sections WHERE programsession_id=" + session.GetId()).Rows;
        
        if (sections.Count > 0)
        {
            foreach (DataRow section in sections)
            {
                DataRow game = DataHelpers.GetGameForSection(section);
                
                if (game != null)
                {
                    GameManager.Instance.AddGame(game);
                }
            }
            
            GameManager.Instance.AddGame("NewSessionComplete");
            
            // Set return scene
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            
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
            
            ScoreInfo.Instance.SetScoreType(session.GetId().ToString());
            
            WingroveAudio.WingroveRoot.Instance.PostEvent("MUSIC_STOP");
            
            GameManager.Instance.StartGames();
        }
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