﻿using UnityEngine;
using System.Collections;
using System;

public class GameMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private PipButton[] m_gameButtons;
    [SerializeField]
    private PipButton[] m_playButtons;
    [SerializeField]
    private GameObject m_onePlayerButtonParent;
    [SerializeField]
    private GameObject m_twoPlayerButtonParent;
    [SerializeField]
    private UILabel m_gameLabel;
    [SerializeField]
    private UISprite m_gameIcon;
    [SerializeField]
    private UITexture m_temporaryGameIcon;
    [SerializeField]
    private UISprite[] m_starSprites;
    [SerializeField]
    private AnimManager m_pipAnimManager;
    [SerializeField]
    private TweenOnOffBehaviour m_gameButtonParent;
    [SerializeField]
    private TweenOnOffBehaviour m_infoParent;

    PipButton m_currentColorButton = null;
    PipButton m_currentGameButton = null;

    bool m_hasActivatedGameButtons = false;

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());

        foreach (PipButton button in m_colorButtons)
        {
            button.Pressing += OnPressColorButton;
        }

        foreach (PipButton button in m_playButtons)
        {
            button.Unpressed += OnPressPlayButton;
        }

        if (GameMenuInfo.Instance.HasBookmark())
        {
            ColorInfo.PipColor currentPipColor = GameMenuInfo.Instance.GetPipColor();
            m_currentColorButton = Array.Find(m_colorButtons, x => x.pipColor == currentPipColor);
            
            if (m_currentColorButton != null)
            {
                m_currentColorButton.ChangeSprite(true);
            }

            ActivateGameButtons();
        }

        GameMenuInfo.Instance.DestroyBookmark();
    }

    void ActivateGameButtons()
    {
        m_hasActivatedGameButtons = true;

        foreach (PipButton button in m_gameButtons)
        {
            button.GetComponent<ChooseGameButton>().SetUp(DataHelpers.FindGame(button.GetString()));
            button.Pressing += OnPressGameButton;
        }

        RefreshGameButtons();

        string currentGameName = GameMenuInfo.Instance.GetGameName();
        m_currentGameButton = Array.Find(m_gameButtons, x => x.GetString() == currentGameName);
        
        if (m_currentGameButton == null)
        {
            Array.Sort(m_gameButtons, CollectionHelpers.ComparePosX);
            m_currentGameButton = m_gameButtons[0];
        }

        m_currentGameButton.ChangeSprite(true);
        ChooseGame(m_currentGameButton);

        m_gameButtonParent.On();
        m_infoParent.On();
    }
    
    void OnPressColorButton(PipButton button)
    {
        if (m_currentColorButton != null)
        {
            m_currentColorButton.ChangeSprite(false);
        }

        m_currentColorButton = button;

        if (!m_hasActivatedGameButtons)
        {
            ActivateGameButtons();
        }
        else
        {
            RefreshGameButtons();
        }
    }

    void OnPressGameButton(PipButton button)
    {
        if (m_currentColorButton != null)
        {
            m_currentGameButton.ChangeSprite(false);
        }

        ChooseGame(button);
    }

    void ChooseGame(PipButton button)
    {
        m_currentGameButton = button;

        DataRow game = DataHelpers.FindGame(button.GetString());

        if (game != null)
        {
            m_pipAnimManager.PlayAnimation("THUMBS_UP");

            if(game["labeltext"] != null)
            {
                m_gameLabel.text = game["labeltext"].ToString();
                NGUIHelpers.MaxLabelWidth(m_gameLabel, 400);
            }

            //m_temporaryGameIcon.mainTexture = button.GetComponent<ChooseGameButton>().GetTemporaryIconTexture();
            m_gameIcon.atlas = button.GetComponent<ChooseGameButton>().GetSpriteAtlas();
            m_gameIcon.spriteName = button.GetComponent<ChooseGameButton>().GetBlackboardSpriteName();
            m_gameIcon.MakePixelPerfect();
           
            bool isTwoPlayer = game["multiplayer"] != null && game["multiplayer"].ToString() == "t";

#if UNITY_STANDALONE
            isTwoPlayer = false;
#endif

            float tweenDuration = 0.5f;

            Vector3 twoPlayerScale = isTwoPlayer ? Vector3.one : Vector3.zero;
            iTween.ScaleTo(m_twoPlayerButtonParent, twoPlayerScale, tweenDuration);

            Vector3 onePlayerScale = isTwoPlayer ? Vector3.one : Vector3.one * 1.5f;
            iTween.ScaleTo(m_onePlayerButtonParent, onePlayerScale, tweenDuration);
            
            Vector3 onePlayerPos = isTwoPlayer ? new Vector3(0, 70) : Vector3.zero;
            Hashtable tweenArgs = new Hashtable();
            tweenArgs.Add("position", onePlayerPos);
            tweenArgs.Add("time", tweenDuration);
            tweenArgs.Add("isLocal", true);
            iTween.MoveTo(m_onePlayerButtonParent, tweenArgs);
        }

        RefreshBoardStars();
    }

    void OnPressPlayButton(PipButton button)
    {
        if (m_currentGameButton != null)
        {
            DataRow game = DataHelpers.FindGame(m_currentGameButton.GetString());

            if (game != null && m_currentColorButton != null)
            {
                SessionInformation.Instance.SetNumPlayers(button.GetInt());

                ColorInfo.PipColor pipColor = m_currentColorButton.pipColor;

                GameManager.Instance.AddGame(game);
                
                GameManager.Instance.SetReturnScene("NewGameMenu");
                
                // Get and set all the data associated with the color
                int moduleId = DataHelpers.GetModuleId(pipColor);

                Debug.Log("moduleId: " + moduleId);
                
                GameManager.Instance.AddData("phonemes", DataHelpers.GetModulePhonemes(moduleId));
                GameManager.Instance.AddData("words", DataHelpers.GetModuleWords(moduleId));
                GameManager.Instance.AddData("keywords", DataHelpers.GetModuleKeywords(moduleId));
                GameManager.Instance.AddData("sillywords", DataHelpers.GetModuleSillywords(moduleId));

                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
                GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
                GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));
                
                DataTable sessionsTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE programmodule_id=" + moduleId + " ORDER BY number DESC");
                
                if (sessionsTable.Rows.Count > 0)
                {
                    GameManager.Instance.AddData("numbers", DataHelpers.CreateNumber(1));
                    GameManager.Instance.AddData("numbers", DataHelpers.CreateNumber(System.Convert.ToInt32(sessionsTable.Rows [0] ["highest_number"])));
                }
                
                GameManager.Instance.SetScoreType(ColorInfo.GetColorString(pipColor));
                
                GameMenuInfo.Instance.CreateBookmark(m_currentGameButton.GetString(), pipColor);
                
                StartCoroutine(StartGames());
            }
        }
    }

    IEnumerator StartGames()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YAY");
        m_pipAnimManager.PlayAnimation("JUMP");
        
        yield return new WaitForSeconds(1.2f);

        GameManager.Instance.StartGames();
    }

    void RefreshBoardStars()
    {
        if (m_currentGameButton != null)
        {
            ColorInfo.PipColor pipColor = m_currentColorButton != null ? m_currentColorButton.pipColor : ColorInfo.PipColor.Pink;
            ScoreInfo.RefreshTimeStars(m_starSprites, DataHelpers.FindGame(m_currentGameButton.GetString())["name"].ToString(), ColorInfo.GetColorString(pipColor));
        }
    }

    void RefreshGameButtons()
    {
        ColorInfo.PipColor pipColor = m_currentColorButton != null ? m_currentColorButton.pipColor : ColorInfo.PipColor.Pink;

        string colorName = ColorInfo.GetColorString(pipColor);

        foreach (PipButton button in m_gameButtons)
        {
            button.GetComponent<ChooseGameButton>().Refresh(colorName);
        }

        RefreshBoardStars();
    }
}
