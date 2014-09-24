﻿using UnityEngine;
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
        GameManager.Instance.SetProgramme("Plus");

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

        for(int i = 0; i < m_mathsGames.Length; ++i)
        {
            m_mathsGames[i].GetComponentInChildren<EventRelay>().SingleClicked += OnClickGameButton;

            bool mustLogin = true;
            string gameName = "NewCompleteEquationNumbers";
            switch(i)
            {
                case 0:
                    mustLogin = false;
                    gameName = "NewClockNumbers";
                    break;
                case 1:
                    gameName = "NewMultiplicationQuadNumbers";
                    break;
                case 2:
                    gameName = "NewToyShopNumbers";
                    break;
                default:
                    break;
            }

            m_mathsGames[i].SetMustLogin(mustLogin);
            m_mathsGames[i].SetUp(gameName);
        }

        for(int i = 0; i < m_readingGames.Length; ++i)
        {
            m_readingGames[i].GetComponentInChildren<EventRelay>().SingleClicked += OnClickGameButton;

            bool mustLogin = true;
            string gameName = "NewPlusQuiz";
            switch(i)
            {
                case 0:
                    mustLogin = false;
                    gameName = "NewPlusSpelling";
                    break;
                case 1:
                    gameName = "NewCorrectWord";
                    break;
                case 2:
                    gameName = "NewShoppingList";
                    break;
                default:
                    break;
            }

            m_readingGames[i].SetMustLogin(mustLogin);
            m_readingGames[i].SetUp(gameName);
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
        //iTween.MoveTo(m_camera, target.position, m_cameraTweenDuration);
        StartCoroutine(TweenCamera(target.position));
    }

    void OnClickSwitchButton(EventRelay relay)
    {
        Transform target = Mathf.Approximately(m_camera.transform.position.x, m_readingParent.transform.position.x) ? m_mathsParent : m_readingParent;
        //iTween.MoveTo(m_camera, target.position, m_cameraTweenDuration);
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

        ColorInfo.PipColor maxColor = plusGame.GetMaxColor();
        for (int i = 0; i < m_chooseColorButtons.Length && i < m_colorBands.Length; ++i)
        {
            bool isUnlocked = i == 0 || (int)m_colorBands[i] <= (int)maxColor + 1;

            m_chooseColorButtons[i].GetComponentInChildren<UISprite>().color = isUnlocked ? ColorInfo.GetColor(m_colorBands[i]) : Color.grey;

            if(isUnlocked)
            {
                m_chooseColorButtons[i].SingleClicked += OnChooseColor;
            }
        }

        if (plusGame.MustLogin() && !LoginInfo.Instance.IsValid())
        {
            LoginInfo.Instance.SpawnLogin();
        }
        else if(!m_hasClickedGameButton)
        {
            m_hasClickedGameButton = true;

            m_bookmark = System.Array.IndexOf(m_mathsGames, plusGame) != -1 ? Bookmark.Maths : Bookmark.Reading;

            m_gameName = plusGame.GetGameName();
            
            m_chooseNumPlayersMoveable.On();  
        }
    }

    void OnChooseColor(EventRelay relay)
    {
        foreach(EventRelay button in m_chooseColorButtons)
        {
            relay.SingleClicked -= OnChooseColor;
        }

        m_pipColor = m_colorBands [System.Array.IndexOf(m_chooseColorButtons, relay)];
        m_chooseColorMoveable.Off();
        StartGame();
    }

    void OnChooseNumPlayers(EventRelay relay)
    {
        SessionInformation.Instance.SetNumPlayers(System.Array.IndexOf(m_chooseNumPlayersButtons, relay) + 1);
        m_chooseNumPlayersMoveable.Off();
        m_chooseColorMoveable.On();
    }

    void OnClickBackCollider(EventRelay relay)
    {
        foreach(EventRelay button in m_chooseColorButtons)
        {
            relay.SingleClicked -= OnChooseColor;
        }

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

    public void MakeAllPipsJump()
    {
        foreach (SpriteAnim anim in m_pipAnims)
        {
            anim.PlayAnimation("JUMP");
        }
    }
}
