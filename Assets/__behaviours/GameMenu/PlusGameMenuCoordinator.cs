using UnityEngine;
using System.Collections;

public class PlusGameMenuCoordinator : Singleton<PlusGameMenuCoordinator> 
{
    [SerializeField]
    private GameObject m_camera;
    [SerializeField]
    private Transform m_readingParent;
    [SerializeField]
    private PipButton m_readingButton;
    [SerializeField]
    private Transform m_mathsParent;
    [SerializeField]
    private PipButton m_mathsButton;
    [SerializeField]
    private TweenOnOffBehaviour m_colorChoiceTweenBehaviour;
    [SerializeField]
    private PipButton[] m_chooseColorButtons;

    string m_scoreType = "plus";
    public string GetScoreType()
    {
        return m_scoreType;
    }

    bool m_hasClickedGameButton = false;

    void Start()
    {
        if (GameManager.Instance.programme == "Reading2")
        {
            m_camera.transform.position = m_readingParent.position;
        }

        m_readingButton.Unpressing += OnUnpressReadingButton;
        m_mathsButton.Unpressing += OnUnpressMathsButton;

        PlusGameButton[] gameButtons = Object.FindObjectsOfType(typeof(PlusGameButton)) as PlusGameButton[];
        foreach (PlusGameButton button in gameButtons)
        {
            button.Clicked += OnClickGameButton;
        }
    }

    void OnUnpressReadingButton(PipButton button)
    {
        TweenCamera(m_readingParent);
    }

    void OnUnpressMathsButton(PipButton button)
    {
        TweenCamera(m_mathsParent);
    }

    void TweenCamera(Transform target)
    {
        iTween.MoveTo(m_camera, target.position, 0.3f);
    }

    int m_numPlayers = 1;
    string m_gameName;
    ColorInfo.PipColor m_pipColor;

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
    
    void OnClickGameButton(PlusGameButton button)
    {
        if(!m_hasClickedGameButton)
        {
            m_hasClickedGameButton = true;

            m_gameName = button.GetGameName();

            m_numPlayers = button.GetNumPlayers();
            SessionInformation.Instance.SetNumPlayers(m_numPlayers);

            if(m_numPlayers == 1)
            {
                m_pipColor = ColorInfo.PipColor.Turquoise;
                StartGame();
            }
            else
            {
                m_colorChoiceTweenBehaviour.On();
            }    
        }
    }

    void OnUnpressChooseColorButton(PipButton button)
    {
        m_pipColor = button.pipColor;
        StartGame();
    }
}
