using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GameMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private PipButton[] m_gameButtons;
    [SerializeField]
    private TweenOnOffBehaviour m_gameButtonParent;
    [SerializeField]
    private GameObject m_starPrefab;
    [SerializeField]
    private UIGrid m_starSpawnGrid;
    [SerializeField]
    private GameObject m_chooseColorLabel;
    [SerializeField]
    private UIWidget[] m_coloredWidgets;
    
    PipButton m_currentColorButton = null;
    
    bool m_hasActivatedGameButtons = false;

    
    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());
        yield return StartCoroutine(TransitionScreen.WaitForScreenExit());
        
        foreach (PipButton button in m_colorButtons)
        {
            button.Pressing += OnPressColorButton;
        }
        
        if (GameMenuInfo.Instance.HasBookmark())
        {
            ColorInfo.PipColor currentPipColor = GameMenuInfo.Instance.GetPipColor();
            m_currentColorButton = Array.Find(m_colorButtons, x => x.pipColor == currentPipColor);
            
            if (m_currentColorButton != null)
            {
                m_currentColorButton.ChangeSprite(true);

                Color color = ColorInfo.GetColor(m_currentColorButton.pipColor);
                foreach (UIWidget widget in m_coloredWidgets)
                {
                    TweenColor.Begin(widget.gameObject, 0.25f, color);
                }
            }
            
            ActivateGameButtons();
            
            yield return new WaitForSeconds(1f);
            
            if(ScoreInfo.Instance.HasNewHighScore())
            {
                string gameName = ScoreInfo.Instance.GetNewHighScoreGame();
                
                ChooseGameButton[] gameButtons = UnityEngine.Object.FindObjectsOfType(typeof(ChooseGameButton)) as ChooseGameButton[];
                
                ChooseGameButton newHighScoreButton = Array.Find(gameButtons, x => x.GetNumPlayers() == 1 && x.GetComponent<PipButton>().GetString() == gameName);
                
                if(newHighScoreButton != null)
                {
                    WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YAY");
                    newHighScoreButton.TweenScoreStars(m_starPrefab, m_starSpawnGrid);
                }
            }
        }
        
        ScoreInfo.Instance.RemoveNewHighScore();
        GameMenuInfo.Instance.DestroyBookmark();
        
        #if UNITY_EDITOR
        string programmeName = Application.loadedLevelName == "NewGameMenu" ? "Reading1" : "Maths1";
        GameManager.Instance.SetProgramme(programmeName);
        #endif
    }
    
    void ActivateGameButtons()
    {
        TweenAlpha.Begin(m_chooseColorLabel, 0.25f, 0);

        m_hasActivatedGameButtons = true;
        
        foreach (PipButton button in m_gameButtons)
        {
            button.GetComponent<ChooseGameButton>().SetUp(DataHelpers.GetGame(button.GetString()));
            button.Pressing += OnPressGameButton;
        }
        
        RefreshGameButtons();
        
        m_gameButtonParent.On();
    }
    
    void OnPressColorButton(PipButton button)
    {
        if (m_currentColorButton != null)
        {
            m_currentColorButton.ChangeSprite(false);
        }
        
        m_currentColorButton = button;

        Color color = ColorInfo.GetColor(button.pipColor);
        foreach (UIWidget widget in m_coloredWidgets)
        {
            TweenColor.Begin(widget.gameObject, 0.25f, color);
        }
        
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
        DataRow game = DataHelpers.GetGame(button.GetString());

        if (game != null)
        {
            GameManager.Instance.AddGame(game);
        }
        else
        {
            GameManager.Instance.AddGame(button.GetString());
        }
        
        if (m_currentColorButton != null)
        {
            ChooseGameButton chooseGameButton = button.GetComponent<ChooseGameButton>() as ChooseGameButton;
            int numPlayers = chooseGameButton != null ? chooseGameButton.GetNumPlayers() : 1;
            SessionInformation.Instance.SetNumPlayers(numPlayers);
            
            ColorInfo.PipColor pipColor = m_currentColorButton.pipColor;

            GameManager.Instance.SetCurrentColor(pipColor);
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            
            // Get and set all the data associated with the color
            if(GameManager.Instance.programme.Contains("Reading"))
            {
                int moduleId = DataHelpers.GetModuleId(pipColor);

                GameManager.Instance.AddData("phonemes", DataHelpers.GetModulePhonemes(moduleId));
                GameManager.Instance.AddData("words", DataHelpers.GetModuleWords(moduleId));
                GameManager.Instance.AddData("keywords", DataHelpers.GetModuleKeywords(moduleId));
                GameManager.Instance.AddData("sillywords", DataHelpers.GetModuleSillywords(moduleId));
                
                DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
                GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
                GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));
            }
            else if(GameManager.Instance.programme.Contains("Maths"))
            {
                DataSetters.AddModuleNumbers(m_currentColorButton.pipColor);
            }
            
            ScoreInfo.Instance.SetScoreType(ColorInfo.GetColorString(pipColor));
            
            GameMenuInfo.Instance.CreateBookmark(pipColor);

            //D.Log("Starting games");
            GameManager.Instance.StartGames();
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
    }	
}
