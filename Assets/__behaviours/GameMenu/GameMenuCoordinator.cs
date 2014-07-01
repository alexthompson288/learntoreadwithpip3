using UnityEngine;
using System.Collections;
using System;

public class GameMenuCoordinator : MonoBehaviour 
{
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private PipButton[] m_gameButtons;
    [SerializeField]
    private AnimManager m_singlePlayerAnim;
    [SerializeField]
    private AnimManager[] m_twoPlayerAnims;
    [SerializeField]
    private UISprite m_rodSprite;
    [SerializeField]
    private TweenOnOffBehaviour[] m_gameButtonParents;
    
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
            }
            
            ActivateGameButtons();
        }
        
        GameMenuInfo.Instance.DestroyBookmark();

#if UNITY_EDITOR
        string programmeName = Application.loadedLevelName == "NewGameMenu" ? "Reading1" : "Maths1";
        GameManager.Instance.SetProgramme(programmeName);
#endif
    }
    
    void ActivateGameButtons()
    {
        m_hasActivatedGameButtons = true;
        
        foreach (PipButton button in m_gameButtons)
        {
            button.GetComponent<ChooseGameButton>().SetUp(DataHelpers.GetGame(button.GetString()));
            button.Pressing += OnPressGameButton;
        }
        
        RefreshGameButtons();
        
        foreach (TweenOnOffBehaviour parent in m_gameButtonParents)
        {
            parent.On();
        }
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
        DataRow game = DataHelpers.GetGame(button.GetString());

        if (game != null && m_currentColorButton != null)
        {
            ChooseGameButton chooseGameButton = button.GetComponent<ChooseGameButton>() as ChooseGameButton;
            int numPlayers = chooseGameButton != null ? chooseGameButton.GetNumPlayers() : 1;
            SessionInformation.Instance.SetNumPlayers(numPlayers);
            
            ColorInfo.PipColor pipColor = m_currentColorButton.pipColor;
            
            GameManager.Instance.AddGame(game);
            
            GameManager.Instance.SetReturnScene(Application.loadedLevelName);
            
            // Get and set all the data associated with the color
            int moduleId = DataHelpers.GetModuleId(pipColor);

            D.Log("moduleId: " + moduleId);
            
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
                int highestNumber = sessionsTable.Rows[0]["highest_number"] != null ? sessionsTable.Rows[0].GetInt("highest_number") : 10;
                GameManager.Instance.AddData("numbers", DataHelpers.CreateNumbers(1, highestNumber));
            }
            
            ScoreInfo.Instance.SetScoreType(ColorInfo.GetColorString(pipColor));
            
            GameMenuInfo.Instance.CreateBookmark(pipColor);
            
            StartCoroutine(StartGames());
        }
    }
    
    IEnumerator StartGames()
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_YAY");
        if (SessionInformation.Instance.GetNumPlayers() == 1)
        {
            m_singlePlayerAnim.PlayAnimation("JUMP");
        } 
        else
        {
            m_rodSprite.spriteName = NGUIHelpers.GetLinkedSpriteName(m_rodSprite.spriteName);
            foreach(AnimManager anim in m_twoPlayerAnims)
            {
                anim.PlayAnimation("JUMP");
            }
        }
        
        yield return new WaitForSeconds(1.2f);
        
        GameManager.Instance.StartGames();
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
