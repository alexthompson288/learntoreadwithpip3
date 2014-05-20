using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class GameMenuCoordinator : Singleton<GameMenuCoordinator> 
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
	
	GameObject m_currentGameMenu = null;

	bool m_isTwoPlayer;
    ColorInfo.PipColor m_color;

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

            string buttonAudioEvent = i == 0 ? "NAV_ONE_PLAYER" : "NAV_TWO_PLAYER";
            m_numPlayerButtons[i].AddUnpressedAudio(buttonAudioEvent);
        }

        foreach (PipButton button in m_colorButtons)
        {
            Transform buttonParent = button.transform.parent;

            string colorName = buttonParent.name;

            buttonParent.GetComponentInChildren<UILabel>().text = colorName;

            button.SetPipColor(ColorInfo.GetPipColor(colorName), true);
            button.AddPressedAudio("COLOR_" + colorName.ToUpper());
            button.Unpressing += OnChooseColor;
        }
    }

    void OnChooseNumPlayers(PipButton button)
    {
        int numPlayers = System.Array.IndexOf(m_numPlayerButtons, button) + 1;
        SessionInformation.Instance.SetNumPlayers(numPlayers);

        m_isTwoPlayer = numPlayers == 2;

        StartCoroutine(MoveCamera(m_numPlayerMenu, m_colorMenu));
    }

    IEnumerator ResetChooseNumPlayersButton(PerspectiveButton button, float delay)
    {
        yield return new WaitForSeconds(delay);
        button.Reset();
    }

    void OnChooseColor(PipButton button)
    {
        m_color = button.pipColor;

        DestroyGameButtons();
        
        StartCoroutine(MoveCamera(m_colorMenu, m_gameMenu));

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

            GameManager.Instance.SetReturnScene("NewScoreDanceScene");

            // Get and set all the data associated with the color
            int moduleId = DataHelpers.GetModuleId(m_color);

            GameManager.Instance.AddData("phonemes", DataHelpers.GetModulePhonemes(moduleId));
            GameManager.Instance.AddData("words", DataHelpers.GetModuleWords(moduleId));
            GameManager.Instance.AddData("keywords", DataHelpers.GetModuleKeywords(moduleId));
            GameManager.Instance.AddData("sillywords", DataHelpers.GetModuleSillywords(moduleId));


            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from datasentences WHERE programmodule_id=" + moduleId);
            GameManager.Instance.AddData("correctcaptions", dt.Rows.FindAll(x => x ["correctsentence"] != null && x ["correctsentence"].ToString() == "t"));
            GameManager.Instance.AddData("quizquestions", dt.Rows.FindAll(x => x ["quiz"] != null && x ["quiz"].ToString() == "t"));

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
        DataTable joinTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from gamecolourjoins WHERE programmodule_id=" + DataHelpers.GetModuleId(m_color)); 

        foreach (DataRow join in joinTable.Rows)
        {
            DataTable gameTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE id=" + System.Convert.ToInt32(join["game_id"]));

            if(gameTable.Rows.Count > 0)
            {
                DataRow game = gameTable.Rows[0];
                Debug.Log("game: " + game);

                // TODO: Temporary fix, remove when database is correct - game["name"].ToString() != "NewSplatGame" 
                bool gameIsMultiplayer = game["multiplayer"] != null && game["multiplayer"].ToString() == "t" && game["name"].ToString() != "NewSplatGame";

                if(!m_isTwoPlayer || m_isTwoPlayer && gameIsMultiplayer)
                {
                    GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_chooseGamePrefab, m_gameGrid.transform);
                    newButton.GetComponent<PipButton>().SetData(game);
                    newButton.GetComponent<PipButton>().Unpressed += OnChooseGame;
                    //newButton.GetComponent<ClickEvent>().SetData(game);
                    //newButton.GetComponent<ClickEvent>().OnSingleClick += OnChooseGame;
                    newButton.GetComponent<ChooseGameButton>().SetUp(game);
                }
            }
        }

        m_gameGrid.Reposition();
    }

    void OnClickBack(ClickEvent click)
    {
        if (TransformHelpers.ApproxPos(m_camera, m_colorMenu))
        {
            StartCoroutine(MoveCamera(m_colorMenu, m_numPlayerMenu));
        } 
        else if (TransformHelpers.ApproxPos(m_camera, m_gameMenu))
        {
            StartCoroutine(MoveCamera(m_gameMenu, m_colorMenu));
            StartCoroutine(DestroyGameButtonsCo(m_cameraTweenDuration));
        }
    }

    IEnumerator MoveCamera(GameObject from, GameObject to, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        //to.SetActive(true);
        iTween.MoveTo(m_camera, to.transform.position, m_cameraTweenDuration);
        
        //yield return new WaitForSeconds(m_cameraTweenDuration);
        
        //from.SetActive(false);
    }  
}
