using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class OldGameMenuCoordinator : MonoBehaviour
{
	[SerializeField]
	private GameObject m_camera;
	[SerializeField]
	private float m_cameraTweenDuration = 0.5f;
	[SerializeField]
	private GameObject m_numPlayerMenu;
    [SerializeField]
    private PipButton[] m_numPlayerButtons;
	[SerializeField]
	private GameObject m_colorMenu;
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private GameObject m_gameMenu;
	[SerializeField]
	private ClickEvent[] m_backButtons;
	[SerializeField]
	private GameObject m_chooseGamePrefab;
	[SerializeField]
	private UIGrid m_gameGrid;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private AudioClip m_onePlayerAudio;
    [SerializeField]
    private AudioClip m_twoPlayerAudio;
	
	GameObject m_currentGameMenu = null;

	bool m_isTwoPlayer;
    ColorInfo.PipColor m_pipColor;

    PipButton m_currentColorButton = null;

    void Start()
    {
        EnviroManager.Instance.SetEnvironment(EnviroManager.Environment.Forest);
        NavMenu.Instance.HideCallButton();

        foreach(ClickEvent click in m_backButtons)
        {
            click.OnSingleClick += OnClickBack;
        }

        System.Array.Sort(m_numPlayerButtons, CollectionHelpers.ComparePosX);

        for (int i = 0; i < m_numPlayerButtons.Length; ++i)
        {
            m_numPlayerButtons[i].Unpressed += OnChooseNumPlayers;

            //string buttonAudioEvent = i == 0 ? "NAV_ONE_PLAYER" : "NAV_TWO_PLAYER";
            //m_numPlayerButtons[i].AddUnpressedAudio(buttonAudioEvent);
        }

        System.Array.Sort(m_colorButtons, CollectionHelpers.ComparePosX);

        foreach (PipButton button in m_colorButtons)
        {
            Transform buttonParent = button.transform.parent;

            string colorName = buttonParent.name;

            buttonParent.GetComponentInChildren<UILabel>().text = colorName;

            button.SetPipColor(ColorInfo.GetPipColor(colorName), true);
            button.AddPressedAudio("COLOR_" + colorName.ToUpper());
            button.Pressing += OnChooseColor;
        }

        if (GameMenuInfo.Instance.HasBookmark())
        {
            m_isTwoPlayer = GameMenuInfo.Instance.IsTwoPlayer();
            m_pipColor = GameMenuInfo.Instance.GetPipColor();

            foreach(PipButton button in m_colorButtons)
            {
                if(button.pipColor == m_pipColor)
                {
                    button.ChangeSprite(true);
                    break;
                }
            }

            SpawnGameButtons();

            m_camera.transform.position = m_colorMenu.transform.position;
        }
    }

    void OnChooseNumPlayers(PipButton button)
    {
        int numPlayers = System.Array.IndexOf(m_numPlayerButtons, button) + 1;
        SessionInformation.Instance.SetNumPlayers(numPlayers);

        AudioClip clip = numPlayers == 1 ? m_onePlayerAudio : m_twoPlayerAudio;
        if (clip != null)
        {
            m_audioSource.clip = clip;
            m_audioSource.Play();
        }

        m_isTwoPlayer = numPlayers == 2;

        OnChooseColor(m_colorButtons [0]);
        m_colorButtons [0].ChangeSprite(true);

        iTween.MoveTo(m_camera, m_colorMenu.transform.position, m_cameraTweenDuration);
    }

    IEnumerator ResetChooseNumPlayersButton(PerspectiveButton button, float delay)
    {
        yield return new WaitForSeconds(delay);
        button.Reset();
    }

    void OnChooseColor(PipButton button)
    {
        if (button != m_currentColorButton)
        {
            StartCoroutine(OnChooseColorCo(button));
        }
    }

    IEnumerator OnChooseColorCo(PipButton button)
    {
        if (m_currentColorButton != null)
        {
            m_currentColorButton.ChangeSprite(false);
        }

        m_currentColorButton = button;

        m_pipColor = m_currentColorButton.pipColor;

        DestroyGameButtons();
        
        yield return new WaitForSeconds(0.25f);
        
        SpawnGameButtons();
    }

    void OnChooseGame(PipButton button)
    {
        Debug.Log("OnChooseGame");
        DataRow game = button.data;
        
        // Set the game scene
        if (game != null)
        {
            Debug.Log("Found game");
            GameManager.Instance.AddGame(game);

            GameManager.Instance.SetReturnScene("NewGameMenu");

            // Get and set all the data associated with the color
            int moduleId = DataHelpers.GetModuleId(m_pipColor);

            GameManager.Instance.AddData("phonemes", DataHelpers.GetModulePhonemes(moduleId));
            GameManager.Instance.AddData("words", DataHelpers.GetModuleWords(moduleId));
            GameManager.Instance.AddData("keywords", DataHelpers.GetModuleKeywords(moduleId));
            GameManager.Instance.AddData("sillywords", DataHelpers.GetModuleSillywords(moduleId));


            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
            GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
            GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));

            DataTable sessionsTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE programmodule_id=" + moduleId + " ORDER BY number DESC");

            if(sessionsTable.Rows.Count > 0)
            {
                GameManager.Instance.AddData("numbers", DataHelpers.CreateNumber(1));
                GameManager.Instance.AddData("numbers", DataHelpers.CreateNumber(System.Convert.ToInt32(sessionsTable.Rows[0]["highest_number"])));
            }

            GameManager.Instance.SetScoreType(ColorInfo.GetColorString(m_pipColor));

            GameMenuInfo.Instance.CreateBookmark(m_isTwoPlayer, m_pipColor);

            GameManager.Instance.StartGames();
        } 
        else
        {
            Debug.LogError("Game button has no data!");
        }
    }

    IEnumerator DestroyGameButtonsCo(float delay)
    {
        yield return new WaitForSeconds(delay);

        DestroyGameButtons();
    }

    void DestroyGameButtons()
    {
        Transform grid = m_gameGrid.transform;
        
        int gameCount = grid.childCount;
        for (int i = gameCount - 1; i > -1; --i)
        {
            Destroy(grid.GetChild(i).gameObject);
        }
    }

    void SpawnGameButtons()
    {
        DataTable joinTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from gamecolourjoins WHERE programmodule_id=" + DataHelpers.GetModuleId(m_pipColor)); 

        int numGames = 0;

        foreach (DataRow join in joinTable.Rows)
        {
            DataTable gameTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE id=" + System.Convert.ToInt32(join["game_id"]));

            if(gameTable.Rows.Count > 0)
            {
                DataRow game = gameTable.Rows[0];

                bool gameIsMultiplayer = game["multiplayer"] != null && game["multiplayer"].ToString() == "t";

                if(!m_isTwoPlayer || m_isTwoPlayer && gameIsMultiplayer)
                {
                    GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_chooseGamePrefab, m_gameGrid.transform);
                    newButton.GetComponent<PipButton>().SetData(game);
                    newButton.GetComponent<PipButton>().Unpressed += OnChooseGame;
                    //newButton.GetComponent<ClickEvent>().SetData(game);
                    //newButton.GetComponent<ClickEvent>().OnSingleClick += OnChooseGame;
                    newButton.GetComponent<ChooseGameButton>().SetUp(game);
                    ++numGames;
                }
            }
        }

        m_gameGrid.transform.localPosition = new Vector3((numGames - 1) * -100, m_gameGrid.transform.localPosition.y);

        m_gameGrid.Reposition();
    }

    void OnClickBack(ClickEvent click)
    {
        if (TransformHelpers.ApproxPos(m_camera, m_colorMenu))
        {
            iTween.MoveTo(m_camera, m_numPlayerMenu.transform.position, m_cameraTweenDuration);
        } 
        else if (TransformHelpers.ApproxPos(m_camera, m_gameMenu))
        {
            iTween.MoveTo(m_camera, m_colorMenu.transform.position, m_cameraTweenDuration);
            StartCoroutine(DestroyGameButtonsCo(m_cameraTweenDuration));
        }
    }

    IEnumerator ResetCurrentColorButton()
    {
        yield return new WaitForSeconds(m_cameraTweenDuration);

        if (m_currentColorButton != null)
        {
            m_currentColorButton.ChangeSprite(false);
        }

        m_currentColorButton = null;
    }
}
