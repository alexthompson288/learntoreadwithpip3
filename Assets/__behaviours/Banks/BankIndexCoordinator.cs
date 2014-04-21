using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class BankIndexCoordinator : MonoBehaviour 
{
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

    List<DataRow> m_data = new List<DataRow>();

    ClickEvent m_currentColor = null;

    IEnumerator Start()
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        if (String.IsNullOrEmpty(GameManager.Instance.dataType))
        {
            GameManager.Instance.SetDataType("words");
        }

        m_data = GameManager.Instance.GetData(GameManager.Instance.dataType);

        Array.Sort(m_colorButtons, SortByColor);

        foreach (ClickEvent click in m_colorButtons)
        {
            click.OnSingleClick += ChangeColor;
            click.GetComponentInChildren<UISprite>().color = ColorInfo.GetColor(click.GetString());
        }

        // Start on the first color
        ChangeColor(m_colorButtons [0]);
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

    void ChangeColor(ClickEvent click)
    {
        if (click != m_currentColor)
        {
            GameManager.Instance.ClearAllData();

            int childCount = m_grid.transform.childCount;
            for(int i = childCount - 1; i > -1; --i)
            {
                Destroy(m_grid.transform.GetChild(i).gameObject);
            }
        }

        m_currentColor = click;

        string query = click.GetString() == "All" ? 
            "select * from phonicssets" :
                ("select * from phonicssets WHERE programmodule_id=" + ColorInfo.GetColorIndex(m_currentColor.GetString()));

        DataTable setsTable = GameDataBridge.Instance.GetDatabase().ExecuteQuery(query);

        foreach(DataRow set in setsTable.Rows)
        {
            GameManager.Instance.AddData(GameManager.Instance.dataType, DataHelpers.GetSetData(set, DataHelpers.setAttribute, DataHelpers.tableName));
        }

        GameObject prefab = GameManager.Instance.dataType == "phonemes" ? m_phonemePrefab : m_wordPrefab;

        List<DataRow> data = GameManager.Instance.GetData(GameManager.Instance.dataType);
        foreach (DataRow row in data)
        {
            GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(prefab, m_grid.transform);
            newButton.GetComponent<BankButton>().SetUp(row);
            newButton.GetComponent<ClickEvent>().OnSingleClick += OnClickBankButton;
            newButton.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
        }

        Debug.Log("data.Count: " + data.Count);

        m_grid.Reposition();
    }

    void OnClickBankButton(ClickEvent click)
    {

    }
}
