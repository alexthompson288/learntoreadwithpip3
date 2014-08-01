﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoinStack : MonoBehaviour 
{
    [SerializeField]
    private int m_value;
    [SerializeField]
    private UISprite m_coinSprite;
    [SerializeField]
    private UIGrid m_stackableGrid;
    [SerializeField]
    private GameObject m_stackablePrefab;
    [SerializeField]
    private int m_maxCoins = 10;
    [SerializeField]
    private ClickEvent m_addButton;
    [SerializeField]
    private ClickEvent m_removeButton;

    string m_stackableSpriteName;

    void Awake()
    {
        m_addButton.SingleClicked += OnClickAddButton;
        m_removeButton.SingleClicked += OnClickRemoveButton;
    }

    public int GetStackedValue()
    {
        return m_value * m_stackableGrid.transform.childCount;
    }

	public void SetValue(int myValue)
    {
        m_value = myValue;

        m_coinSprite.spriteName = System.String.Format("coin{0}", m_value.ToString());
        m_stackableSpriteName = System.String.Format("coin_stack{0}", m_value.ToString());
    }

    void OnClickAddButton(ClickEvent click)
    {
        AddCoin();
    }

    void AddCoin()
    {
        if (m_stackableGrid.transform.childCount < m_maxCoins)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
            WingroveAudio.WingroveRoot.Instance.PostEvent("DING");
            GameObject newStackableCoin = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_stackablePrefab, m_stackableGrid.transform);
            newStackableCoin.GetComponent<UISprite>().spriteName = m_stackableSpriteName;
            m_stackableGrid.Reposition();
        }
    }

    void OnClickRemoveButton(ClickEvent click)
    {
        RemoveCoin();
    }

    void RemoveCoin()
    {
        if (m_stackableGrid.transform.childCount > 0)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");
            Destroy(m_stackableGrid.transform.GetChild(m_stackableGrid.transform.childCount - 1).gameObject);
            m_stackableGrid.Reposition();
        }
    }

    public void ClearStack()
    {
        List<GameObject> stackables = new List<GameObject>();

        for (int i = 0; i < m_stackableGrid.transform.childCount; ++i)
        {
            stackables.Add(m_stackableGrid.transform.GetChild(i).gameObject);
        }

        CollectionHelpers.DestroyObjects(stackables);

        m_stackableGrid.Reposition();
    }
}
