using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class BankIndexCoordinator : Singleton<BankIndexCoordinator> 
{
    public delegate void MoveToShow(DataRow data, string s);
    public event MoveToShow OnMoveToShow;

    [SerializeField]
    private UIPanel m_defaultGridPanel;
    [SerializeField]
    private UIDraggablePanel m_defaultDraggablePanel;
    [SerializeField]
    private UIGrid m_defaultGrid;
    [SerializeField]
    private UIGrid m_alphabetGrid;
    [SerializeField]
    private PipButton[] m_colorButtons;
    [SerializeField]
    private GameObject m_phonemePrefab;
    [SerializeField]
    private GameObject m_wordPrefab;
    [SerializeField]
    private GameObject m_alphabetLetterPrefab;
    [SerializeField]
    private PipButton m_testButton;
    [SerializeField]
    private PipButton m_clearButton;
    [SerializeField]
    private GameObject[] m_alphabetButtons;
    [SerializeField]
    private TweenOnOffBehaviour m_noDataSign;
    [SerializeField]
    private Transform m_colorGrid;
    [SerializeField]
    private Transform m_alternateColorGridLocation;

    List<DataRow> m_data = new List<DataRow>();

    PipButton m_currentButton = null;

    Vector3 m_gridStartPosition;

    string m_dataType;
    public string GetDataType()
    {
        return m_dataType;
    }

    bool m_hasSetDataType = false;

    public static IEnumerator WaitForSetDataType()
    {
        while (Instance == null || !Instance.m_hasSetDataType)
        {
            yield return null;
        }
    }

    IEnumerator Start()
    {
        m_defaultGridPanel.alpha = 0;

        m_gridStartPosition = m_defaultGrid.transform.localPosition;

        Debug.Log("gridStartPos: " + m_gridStartPosition);

        m_clearButton.Unpressing += ClearAnswers;
        m_testButton.Unpressing += OnClickTestButton;

        NavMenu.Instance.HideCallButton();

        Debug.Log("gameName: " + DataHelpers.GetGameName());

        string defaultDataType = Application.loadedLevelName == "NewBankWords" ? "words" : "phonemes";
        m_dataType = DataHelpers.GameOrDefault(defaultDataType);

        if (m_dataType == "shapes")
        {
            m_dataType = "alphabet";
        }

        m_hasSetDataType = true;

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_data = GameManager.Instance.GetData(m_dataType);

        if (m_dataType != "alphabet")
        {
            foreach(GameObject button in m_alphabetButtons)
            {
                button.SetActive(false);
            }

            Array.Sort(m_colorButtons, SortByColor);

            if(m_dataType != "phonemes")
            {
                m_colorButtons[m_colorButtons.Length - 1].gameObject.SetActive(false);
            }

            m_colorGrid.position = m_alternateColorGridLocation.position;

            foreach (PipButton button in m_colorButtons)
            {
                button.Pressing += ChangeColor;
            }

            // Start on the first color
            ChangeColor(m_colorButtons [0]);
            m_colorButtons[0].ChangeSprite(true);
        } 
        else
        {
            m_colorGrid.gameObject.SetActive(false);

            for (char i = 'a'; i <= 'z'; ++i)
            {
                GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_alphabetLetterPrefab, m_alphabetGrid.transform);
                newButton.GetComponent<BankButton>().SetUp(m_dataType, i.ToString());
                newButton.GetComponent<ClickEvent>().SetString(i.ToString());
                newButton.GetComponent<ClickEvent>().SingleClicked += OnClickTestButton;
                newButton.GetComponent<UIDragPanelContents>().draggablePanel = m_defaultDraggablePanel;
            }
            
            m_alphabetGrid.Reposition();

            float alphaTweenDuration = 0.2f;
            
            TweenAlpha.Begin(m_defaultGridPanel.gameObject, alphaTweenDuration, 1);
        }
    }

    int SortByColor(PipButton x, PipButton y)
    {
        int xIndex = (int)x.pipColor;
        int yIndex = (int)y.pipColor;

        if (xIndex < 0)
        {
            return 1;
        } 
        else if (yIndex < 0)
        {
            return -1;
        }
        else if (xIndex < yIndex)
        {
            return - 1;
        } 
        else if (xIndex > yIndex)
        {
            return 1;
        } 
        else
        {
            return 0;
        }
    }

    public void RefreshButtons()
    {
        BankButton[] bankButtons = UnityEngine.Object.FindObjectsOfType(typeof(BankButton)) as BankButton[];
        foreach (BankButton button in bankButtons)
        {
            button.Refresh();
        }
    }

    void ChangeColor(PipButton button)
    {
        if (button != m_currentButton)
        {
            if(m_currentButton != null)
            {
                m_currentButton.ChangeSprite(false);
            }

            m_currentButton = button;
            //StartCoroutine(MoveCurrentColorSprite(button));
            StartCoroutine(ChangeColorCo(button.pipColor));
        }
    }

    IEnumerator ChangeColorCo(ColorInfo.PipColor color)
    {
        float alphaTweenDuration = 0.2f;

        TweenAlpha.Begin(m_defaultGridPanel.gameObject, alphaTweenDuration, 0);

        yield return new WaitForSeconds(alphaTweenDuration);
        
        int childCount = m_defaultGrid.transform.childCount;
        for(int i = childCount - 1; i > -1; --i)
        {
            Destroy(m_defaultGrid.transform.GetChild(i).gameObject);
        }
        
        m_defaultGrid.transform.localPosition = m_gridStartPosition;

        GameObject prefab = m_dataType == "phonemes" ? m_phonemePrefab : m_wordPrefab;

        List<DataRow> data = new List<DataRow>();

        string dataType = m_dataType;

        int moduleId = DataHelpers.GetModuleId(color);

        if (dataType == "phonemes")
        {
            data = DataHelpers.GetModulePhonemes(moduleId);
        } 
        else if (dataType == "words")
        {
            data = DataHelpers.GetModuleWords(moduleId);
        } 
        else if (dataType == "keywords")
        {
            data = DataHelpers.GetModuleKeywords(moduleId);
        }

        GameManager.Instance.SetData(m_dataType, data);
 
        if (data.Count == 0)
        {
            Debug.Log("No Data!");
            m_noDataSign.On();
        }
        else if(m_noDataSign.IsOn())
        {
            Debug.Log("Dismiss no data sign");
            m_noDataSign.Off();
        }

        foreach (DataRow row in data)
        {
            GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(prefab, m_defaultGrid.transform);
            newButton.GetComponent<BankButton>().SetUp(m_dataType, row);
            newButton.GetComponent<ClickEvent>().SetData(row);
            newButton.GetComponent<ClickEvent>().SingleClicked += OnClickTestButton;
            newButton.GetComponent<UIDragPanelContents>().draggablePanel = m_defaultDraggablePanel;
        }
        
        //Debug.Log("data.Count: " + data.Count);
        
        yield return new WaitForSeconds(0.1f);
        
        m_defaultGrid.Reposition();

        yield return new WaitForSeconds(0.1f);

        TweenAlpha.Begin(m_defaultGridPanel.gameObject, alphaTweenDuration, 1); 
    }

    void OnClickTestButton(PipButton button)
    {
        if (OnMoveToShow != null)
        {
            OnMoveToShow(null, "");
        }
        
        BankCamera.Instance.MoveToShow();
    }
    
    void OnClickTestButton(ClickEvent click)
    {
        if (OnMoveToShow != null)
        {
            OnMoveToShow(click.GetData(), click.GetString());
        }

        BankCamera.Instance.MoveToShow();
    }

    void ClearAnswers(PipButton click)
    {
        BankInfo.Instance.ClearAnswers();
        RefreshButtons();
    }
}
