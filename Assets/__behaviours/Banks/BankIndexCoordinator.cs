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
    private UIPanel m_gridPanel;
    [SerializeField]
    private UIDraggablePanel m_draggablePanel;
    [SerializeField]
    private UIGrid m_grid;
    [SerializeField]
    private ClickEvent[] m_colorButtons;
    [SerializeField]
    private GameObject m_phonemePrefab;
    [SerializeField]
    private GameObject m_wordPrefab;
    [SerializeField]
    private ClickEvent m_nextButton;
    [SerializeField]
    private ClickEvent m_clearButton;
    [SerializeField]
    private GameObject m_songButton;
    [SerializeField]
    private TweenOnOffBehaviour m_noDataSign;
    [SerializeField]
    private UISprite m_currentColorSprite;

    List<DataRow> m_data = new List<DataRow>();

    ClickEvent m_currentColor = null;
    ThrobGUIElement m_currentColorThrob;

    Vector3 m_gridStartPosition;

    IEnumerator Start()
    {
        m_gridPanel.alpha = 0;

        m_gridStartPosition = m_grid.transform.localPosition;

        Debug.Log("gridStartPos: " + m_gridStartPosition);

        m_clearButton.OnSingleClick += ClearAnswers;
        m_nextButton.OnSingleClick += OnClickBankButton;

        NavMenu.Instance.HideCallButton();


#if UNITY_EDITOR
        if (String.IsNullOrEmpty(GameManager.Instance.dataType))
        {
            GameManager.Instance.SetDataType("phonemes");
        }
#endif

        m_songButton.SetActive(GameManager.Instance.dataType == "alphabet");

        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        m_data = GameManager.Instance.GetData(GameManager.Instance.dataType);

        if (GameManager.Instance.dataType != "alphabet")
        {
            Array.Sort(m_colorButtons, SortByColor);

            foreach (ClickEvent click in m_colorButtons)
            {
                click.OnSingleClick += ChangeColor;
                click.GetComponentInChildren<UISprite>().color = ColorInfo.GetColor(click.GetString());
            }

            // Start on the first color
            ChangeColor(m_colorButtons [0]);
        } 
        else
        {
            foreach(ClickEvent click in m_colorButtons)
            {
                click.gameObject.SetActive(false);
            }

            for (char i = 'a'; i <= 'z'; ++i)
            {
                GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_phonemePrefab, m_grid.transform);
                newButton.GetComponent<BankButton>().SetUp(i.ToString());
                newButton.GetComponent<ClickEvent>().SetString(i.ToString());
                newButton.GetComponent<ClickEvent>().OnSingleClick += OnClickBankButton;
                newButton.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
            }
            
            m_grid.Reposition();
        }
    }

    int SortByColor(ClickEvent x, ClickEvent y)
    {
        int xIndex = ColorInfo.GetColorIndex(x.GetString());
        int yIndex = ColorInfo.GetColorIndex(y.GetString());

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

    void ChangeColor(ClickEvent click)
    {
        if (click != m_currentColor)
        {
            m_currentColor = click;
            StartCoroutine(MoveCurrentColorSprite(click));
            StartCoroutine(ChangeColorCo(click.GetString()));
        }
    }

    IEnumerator MoveCurrentColorSprite(ClickEvent click)
    {
        float scaleTweenDuration = 0.3f;
        
        TweenScale.Begin(m_currentColorSprite.gameObject, scaleTweenDuration, Vector3.one * 0.8f);
        
        yield return new WaitForSeconds(scaleTweenDuration);

        m_currentColorSprite.transform.position = click.transform.position;
        m_currentColorSprite.color = ColorInfo.GetColor(click.GetString());
        
        TweenScale.Begin(m_currentColorSprite.gameObject, scaleTweenDuration, Vector3.one * 1.2f);
    }

    IEnumerator ChangeColorCo(string color)
    {
        float alphaTweenDuration = 0.2f;

        TweenAlpha.Begin(m_gridPanel.gameObject, alphaTweenDuration, 0);

        yield return new WaitForSeconds(alphaTweenDuration);

        GameManager.Instance.ClearAllData();
        
        int childCount = m_grid.transform.childCount;
        for(int i = childCount - 1; i > -1; --i)
        {
            Destroy(m_grid.transform.GetChild(i).gameObject);
        }
        
        m_grid.transform.localPosition = m_gridStartPosition;
        

        string query = color == "All" ? 
            "select * from phonicssets" :
                ("select * from phonicssets WHERE programmodule_id=" + ColorInfo.GetColorIndex(m_currentColor.GetString()));
        
        DataTable setsTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery(query);
        
        //Debug.Log("Found " + setsTable.Rows.Count + " sets");
        //Debug.Log("setAttribute: " + DataHelpers.setAttribute);
        //Debug.Log("tableName: " + DataHelpers.tableName);
        
        foreach(DataRow set in setsTable.Rows)
        {
            //Debug.Log("setId: " + set["id"].ToString());
            GameManager.Instance.AddData(GameManager.Instance.dataType, DataHelpers.GetSetData(set, DataHelpers.setAttribute, DataHelpers.tableName));
        }
        
        GameObject prefab = GameManager.Instance.dataType == "phonemes" ? m_phonemePrefab : m_wordPrefab;
        
        List<DataRow> data = GameManager.Instance.GetData(GameManager.Instance.dataType);

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
            GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(prefab, m_grid.transform);
            newButton.GetComponent<BankButton>().SetUp(row);
            newButton.GetComponent<ClickEvent>().SetData(row);
            newButton.GetComponent<ClickEvent>().OnSingleClick += OnClickBankButton;
            newButton.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
        }
        
        Debug.Log("data.Count: " + data.Count);
        
        yield return new WaitForSeconds(0.1f);
        
        m_grid.Reposition();

        yield return new WaitForSeconds(0.1f);

        TweenAlpha.Begin(m_gridPanel.gameObject, alphaTweenDuration, 1); 
    }
    
    
    void OnClickBankButton(ClickEvent click)
    {
        if (OnMoveToShow != null)
        {
            OnMoveToShow(click.GetData(), click.GetString());
        }

        BankCamera.Instance.MoveToShow();
    }

    void ClearAnswers(ClickEvent click)
    {
        BankInfo.Instance.ClearAnswers();
        RefreshButtons();
    }
}
