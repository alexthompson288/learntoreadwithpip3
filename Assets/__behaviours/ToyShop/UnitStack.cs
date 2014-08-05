using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitStack : MonoBehaviour 
{
    [SerializeField]
    private int m_value;
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private UIGrid m_stackableGrid;
    [SerializeField]
    private GameObject m_stackablePrefab;
    [SerializeField]
    private int m_maxStacks = 10;
    [SerializeField]
    private ClickEvent m_addButton;
    [SerializeField]
    private ValueSpriteName[] m_valueSpriteNames;

    string m_stackableSpriteName;

    [System.Serializable]
    class ValueSpriteName
    {
        public int m_value;
        public string m_spriteName;
        public string m_stackableSpriteName;
    }

    void Awake()
    {
        m_addButton.SingleClicked += OnClickAddButton;
    }

    public int GetStackedValue()
    {
        return m_value * m_stackableGrid.transform.childCount;
    }

	public void SetValue(int myValue)
    {
        m_value = myValue;

        ValueSpriteName correctUnit = System.Array.Find(m_valueSpriteNames, x => x.m_value == m_value);

        if (correctUnit != null)
        {
            m_background.spriteName = correctUnit.m_spriteName;
            m_stackableSpriteName = correctUnit.m_stackableSpriteName;
        }
    }

    void OnClickAddButton(ClickEvent click)
    {
        AddStack();
    }

    void AddStack()
    {
        if (m_stackableGrid.transform.childCount < m_maxStacks)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
            WingroveAudio.WingroveRoot.Instance.PostEvent("DING");
            GameObject newStackableCoin = Wingrove.SpawningHelpers.InstantiateUnderWithPrefabTransforms(m_stackablePrefab, m_stackableGrid.transform);
            newStackableCoin.GetComponent<UISprite>().spriteName = m_stackableSpriteName;
            newStackableCoin.GetComponent<ClickEvent>().SingleClicked += OnClickRemoveButton;
            m_stackableGrid.Reposition();
        }
    }

    void OnClickRemoveButton(ClickEvent click)
    {
        RemoveStack();
    }

    void RemoveStack()
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
