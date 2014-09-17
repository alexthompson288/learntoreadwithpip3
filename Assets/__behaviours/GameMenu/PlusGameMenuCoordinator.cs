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
    private PlusGame[] m_plusGames;
    [SerializeField]
    private EventRelay m_backCollider;
    [SerializeField]
    private TweenBehaviour m_chooseNumPlayersMoveable;
    [SerializeField]
    private EventRelay[] m_chooseNumPlayersButtons;
    [SerializeField]
    private TweenBehaviour m_chooseColorMoveable;
    [SerializeField]
    private EventRelay[] m_chooseColorButtons;

    string m_gameName;
    ColorInfo.PipColor m_pipColor;
    int m_numPlayers;

    bool m_hasClickedGameButton = false;

    string m_scoreType = "plus";
    public string GetScoreType()
    {
        return m_scoreType;
    }

    void Awake()
    {
        m_readingButton.SingleClicked += OnClickMenuButton;
        m_mathsButton.SingleClicked += OnClickMenuButton;

        m_backCollider.SingleClicked += OnClickBackCollider;

        foreach (EventRelay relay in m_switchButtons)
        {
            relay.SingleClicked += OnClickSwitchButton;
        }

        foreach (EventRelay relay in m_chooseNumPlayersButtons)
        {
            relay.SingleClicked += OnChooseNumPlayers;
        }

        foreach (EventRelay relay in m_chooseColorButtons)
        {
            relay.SingleClicked += OnChooseColor;
        }

        foreach(PlusGame game in m_plusGames)
        {
            game.GetComponentInChildren<EventRelay>().SingleClicked += OnClickGameButton;
        }
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
        iTween.MoveTo(m_camera, target.position, m_cameraTweenDuration);
    }

    void OnClickSwitchButton(EventRelay relay)
    {
        Transform target = Mathf.Approximately(m_camera.transform.position.x, m_readingParent.transform.position.x) ? m_mathsParent : m_readingParent;
        iTween.MoveTo(m_camera, target.position, m_cameraTweenDuration);
    }

    void OnClickGameButton(EventRelay relay)
    {
        if(!m_hasClickedGameButton)
        {
            m_hasClickedGameButton = true;
            
            m_gameName = relay.GetComponentInParent<PlusGame>().GetGameName();
            
            if(m_numPlayers == 1)
            {
                m_pipColor = ColorInfo.PipColor.Turquoise;
                StartGame();
            }
            else
            {
                m_chooseNumPlayersMoveable.On();
            }    
        }
    }

    void OnChooseColor(EventRelay relay)
    {
        int index = System.Array.IndexOf(m_chooseColorButtons, relay);

        switch (index)
        {
            case 0:
                m_pipColor = ColorInfo.PipColor.Turquoise;
                break;
            case 1:
                m_pipColor = ColorInfo.PipColor.Purple;
                break;
            case 2:
                m_pipColor = ColorInfo.PipColor.Gold;
                break;
            case 3:
                m_pipColor = ColorInfo.PipColor.White;
                break;
            default:
                m_pipColor = ColorInfo.PipColor.Turquoise;
                break;
        }

        m_chooseColorMoveable.Off();
        m_chooseNumPlayersMoveable.On();
    }

    void OnChooseNumPlayers(EventRelay relay)
    {
        int numPlayers = System.Array.IndexOf(m_chooseNumPlayersButtons, relay);
        StartGame();
    }

    void OnClickBackCollider(EventRelay relay)
    {
        m_chooseColorMoveable.Off();
        m_chooseNumPlayersMoveable.Off();
    }

    void StartGame()
    {
        SessionInformation.Instance.SetNumPlayers(m_numPlayers);
        PlusScoreInfo.Instance.SetScoreType(m_scoreType);
        
        GameManager.Instance.SetCurrentColor(m_pipColor);
        GameManager.Instance.AddGame(m_gameName);
        GameManager.Instance.SetReturnScene(Application.loadedLevelName);
        
        bool isMaths = Mathf.Approximately(m_camera.transform.position.x, m_mathsParent.position.x);
        
        string programmeName = isMaths ? "Maths2" : "Reading2";
        GameManager.Instance.SetProgramme(programmeName);
        
        if(isMaths)
        {
            DataSetters.AddModuleNumbers(m_pipColor);
            DataSetters.AddModuleTimes(m_pipColor);
        }
        else if(GameManager.Instance.programme.Contains("Maths"))
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
        
        GameManager.Instance.StartGames();
    }
}
