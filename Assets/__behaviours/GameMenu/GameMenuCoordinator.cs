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
    private ClickEvent[] m_numPlayerButtons;
	[SerializeField]
	private GameObject m_colorMenu;
    [SerializeField]
    private ClickEvent[] m_colorButtons;
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

        foreach (ClickEvent click in m_numPlayerButtons)
        {
            click.OnSingleClick += OnChooseNumPlayers;
        }

        foreach (ClickEvent click in m_colorButtons)
        {
            click.OnSingleClick += OnChooseColor;

            click.GetComponentInChildren<UISprite>().color = ColorInfo.GetColor(click.GetString());
            click.GetComponentInChildren<UILabel>().text = click.GetString();
        }
    }

    void OnChooseNumPlayers(ClickEvent click)
    {
        m_isTwoPlayer = click.GetInt() == 2;

        StartCoroutine(MoveCamera(m_numPlayerMenu, m_colorMenu));
    }

    void OnChooseColor(ClickEvent click)
    {
        m_color = ColorInfo.GetPipColor(click.GetString());

        StartCoroutine(MoveCamera(m_colorMenu, m_gameMenu));

        StartCoroutine(DestroyGameButtons(0));

        SpawnGameButtons();
    }

    void OnChooseGame(ClickEvent click)
    {
        DataRow game = click.GetData();
        
        // Set the game scene
        if (game ["name"] != null)
        {
            string sceneName = GameLinker.Instance.GetSceneName(game["name"].ToString());

            if(!System.String.IsNullOrEmpty(sceneName))
            {
                GameManager.Instance.SetScenes(sceneName);

                GameManager.Instance.SetReturnScene("NewScoreDanceScene");

                // Get and set all the data associated with the color
            }
        }
    }

    IEnumerator DestroyGameButtons(float delay)
    {
        yield return new WaitForSeconds(delay);

        Transform grid = m_gameGrid.transform;

        int gameCount = grid.childCount;
        for (int i = gameCount - 1; i > -1; --i)
        {
            Destroy(grid.GetChild(i));
        }
    }

    void SpawnGameButtons()
    {
        // TODO: Adding 1 should be done in ColorInfo
        DataTable joinTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from gamecolourjoin WHERE colour=" + (((int)m_color) + 1)); 

        List<DataRow> games = new List<DataRow>();

        foreach (DataRow join in joinTable.Rows)
        {
            DataTable gameTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from games WHERE id=" + System.Convert.ToInt32(join["game"]));

            if(gameTable.Rows.Count > 0)
            {
                DataRow game = gameTable.Rows[0];

                bool gameIsMultiplayer = game["multiplayer"] != null && game["multiplayer"].ToString() == "t";

                if(gameIsMultiplayer == m_isTwoPlayer)
                {
                    GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_chooseGamePrefab, m_gameGrid.transform);
                    newButton.GetComponent<ClickEvent>().SetData(game);
                    newButton.GetComponent<ClickEvent>().OnSingleClick += OnChooseGame;
                }
            }
        }
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
            StartCoroutine(DestroyGameButtons(m_cameraTweenDuration));
        }
    }

    IEnumerator MoveCamera(GameObject from, GameObject to)
    {
        to.SetActive(true);
        iTween.MoveTo(m_camera, to.transform.position, m_cameraTweenDuration);
        
        yield return new WaitForSeconds(m_cameraTweenDuration);
        
        from.SetActive(false);
    }  
}
