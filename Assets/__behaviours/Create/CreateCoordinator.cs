using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ReadWriteCsv;
using Wingrove;

public class CreateCoordinator : Singleton<CreateCoordinator> 
{
    [SerializeField]
    private string m_filename = "/CreateEnvironments.csv";
	[SerializeField]
	private TweenBehaviour m_sidebar;
    [SerializeField]
    private TweenBehaviour m_menuBar;
    [SerializeField]
    private EventRelay m_callSidebarButton;
    [SerializeField]
    private EventRelay m_dismissSidebarCollider;
    [SerializeField]
    private EventRelay m_callEnvironmentsButton;
    [SerializeField]
    private EventRelay m_callStickersButton;
    [SerializeField]
    private EventRelay m_callColorsButton;
    [SerializeField]
    private Texture2D m_colorTexture;
    [SerializeField]
    private Texture2D[] m_bgs;
    [SerializeField]
    private ColorInfo.PipColor[] m_colors;
    [SerializeField]
    private GameObject m_menuItemPrefab;
    [SerializeField]
    private UITexture m_background;
    [SerializeField]
    private UISprite m_colorDisplay;

    List<EventRelay> m_spawnedButtons = new List<EventRelay>();

    List<Environment> m_enviros = new List<Environment>();

    Environment m_currentEnviro = null;
    Color m_currentColor = Color.white;

    Menu m_currentMenu = Menu.Environments;

    enum Menu
    {
        Environments,
        Stickers,
        Colors
    }

    void OnClickCallEnvironmentsButton(EventRelay relay)
    {
        m_currentMenu = Menu.Environments;
        StartCoroutine(CallMenubar());
    }

    void OnClickCallStickersButton(EventRelay relay)
    {
        m_currentMenu = Menu.Stickers;
        StartCoroutine(CallMenubar());
    }

    void OnClickCallColorsButton(EventRelay relay)
    {
        m_currentMenu = Menu.Colors;
        StartCoroutine(CallMenubar());
    }

    void OnChooseEnvironment(EventRelay relay)
    {
        m_background.mainTexture = relay.GetComponent<UITexture>().mainTexture;
        m_currentEnviro = m_enviros.Find(x => x.m_bg == m_background.mainTexture.name);
        Debug.Log("Changed enviro to " + m_currentEnviro.m_name);
    }

    void OnChooseColor(EventRelay relay)
    {
        m_currentColor = relay.GetComponent<UITexture>().color;
        m_colorDisplay.color = m_currentColor;
    }

    Texture2D GetEnviroBackground(Environment enviro)
    {
        return enviro != null ? System.Array.Find(m_bgs, x => x.name == enviro.m_bg) : null;
    }

    void PopulateMenubar()
    {
        if (m_currentMenu == Menu.Colors)
        {
            foreach(ColorInfo.PipColor color in m_colors)
            {
                GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_menuItemPrefab, m_menuBar.transform, false);
                newItem.GetComponent<UITexture>().mainTexture = m_colorTexture;
                newItem.GetComponent<UITexture>().color = ColorInfo.GetColor(color);
                newItem.GetComponent<EventRelay>().SingleClicked += OnChooseColor;
            }
        }
        else
        {
            List<Texture2D> textures = new List<Texture2D>();

            if(m_currentMenu == Menu.Environments)
            {
                foreach(Environment enviro in m_enviros)
                {
                    Texture2D bg = GetEnviroBackground(enviro);
                    if(bg != null)
                    {
                        textures.Add(bg);
                    }
                }
            }
            else
            {
                foreach(string item in m_currentEnviro.m_stickers)
                {
                    Texture2D tex = Resources.Load<Texture2D>("Images/word_images_png_350/_" + item);
                    if(tex != null)
                    {
                        textures.Add(tex);
                    }
                }
            }

            foreach(Texture2D tex in textures)
            {
                GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_menuItemPrefab, m_menuBar.transform, false);
                newItem.GetComponent<UITexture>().mainTexture = tex; 

                if(m_currentMenu == Menu.Environments)
                {
                    newItem.GetComponent<EventRelay>().SingleClicked += OnChooseEnvironment;
                }
            }

            m_menuBar.GetComponentInChildren<UIGrid>().Reposition();
        }
    }

    IEnumerator CallMenubar()
    {
        if (m_menuBar.isOn)
        {
            m_menuBar.Off();
            yield return new WaitForSeconds(m_menuBar.GetTotalDurationOff());
        }

        TransformHelpers.DestroyChildren(m_menuBar.transform);

        PopulateMenubar();

        m_menuBar.On();
    }

    void OnClickDismissSidebar(EventRelay relay)
    {
        m_sidebar.Off();
    }

    void OnClickCallSidebar(EventRelay relay)
    {
        if (m_sidebar.isOn)
        {
            m_sidebar.Off();
        }
        else
        {
            m_sidebar.On();
        }
    }

    class Environment
    {
        public string m_name;
        public string m_labelText;
        public string m_bg;
        public int m_id;
        public List<string> m_stickers = new List<string>();
        
        public void AddItem(string item)
        {
            m_stickers.Add(item);
        }
    }

    void Awake()
    {
        string path = Application.streamingAssetsPath + m_filename;
        
        using (CsvFileReader reader = new CsvFileReader(path))
        {
            CsvRow row = new CsvRow();
            while (reader.ReadRow(row))
            {
                string[] enviroData = row.LineText.Split(',');
                
                D.Log(enviroData.Length);
                
                if(enviroData.Length > 4)
                {
                    Environment enviro = new Environment();
                    
                    enviro.m_name = enviroData[0];
                    enviro.m_labelText = enviroData[1];
                    enviro.m_bg = enviroData[2];
                    
                    try
                    {
                        enviro.m_id = System.Convert.ToInt32(enviroData[3]);
                    }
                    catch
                    {
                        Debug.Log(string.Format("Could not convert {0} in enviro {1}", enviroData[2], enviroData[0]));
                    }
                    
                    for(int i = 4; i < enviroData.Length; ++i)
                    {
                        if(!System.String.IsNullOrEmpty(enviroData[i]))
                        {
                            enviro.AddItem(enviroData[i]);
                        }
                    }
                    
                    m_enviros.Add(enviro);
                }
            }
        }

        if (m_enviros.Count > 0)
        {
            m_currentEnviro = m_enviros [0];
            m_background.mainTexture = GetEnviroBackground(m_currentEnviro);
        }
    }

    void Start()
    {
        if (m_colors.Length > 0)
        {
            m_currentColor = ColorInfo.GetColor(m_colors[0]);
        }

        m_colorDisplay.color = m_currentColor;
    }
}