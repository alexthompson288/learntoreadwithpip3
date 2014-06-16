using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class GameWidgetHolder : MonoBehaviour 
{
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private GameObject m_widgetPrefab;
    [SerializeField]
    private float m_scaleTweenDuration = 0.3f;

    List<GameWidget> m_heldWidgets = new List<GameWidget>();

    public int heldWidgetCount
    {
        get
        {
            return m_heldWidgets.Count;
        }
    }

    Transform FindEmptyLocator()
    {
        foreach (Transform locator in m_locators)
        {
            if(locator.childCount == 0)
            {
                return locator;
            }
        }

        return m_locators [0];
    }

    public void AddWidget(GameWidget widget)
    {
        Transform emptyLocator = FindEmptyLocator();
        iTween.MoveTo(widget.gameObject, emptyLocator.position, 0.5f);
        //iTween.MoveTo(widget.gameObject, m_locators [m_heldWidgets.Count].position, 0.5f);
        widget.transform.parent = emptyLocator;
        m_heldWidgets.Add(widget);
    }

    public void RemoveWidget(GameWidget widget)
    {
        m_heldWidgets.Remove(widget);
    }

	public List<GameWidget> SpawnWidgets(int numSpawn)
    {
        List<GameWidget> newWidgets = new List<GameWidget>();
        int firstLocatorIndex = m_heldWidgets.Count;

        System.Array.Sort(m_locators, CollectionHelpers.ComparePosYThenX);

        for (int i = 0; i < numSpawn; ++i)
        {
            GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_widgetPrefab, m_locators[firstLocatorIndex + i]);
            GameWidget widget = newGo.GetComponent<GameWidget>() as GameWidget;
            widget.SetUpBackground();
            newWidgets.Add(widget);
        }

        m_heldWidgets.AddRange(newWidgets);

        return newWidgets;
    }

    public void Shake()
    {
        iTween.ShakePosition(gameObject, Vector3.one * 0.2f, 0.35f);
    }

    public IEnumerator Off()
    {
        iTween.ScaleTo(gameObject, Vector3.zero, m_scaleTweenDuration);

        yield return new WaitForSeconds(m_scaleTweenDuration);

        CollectionHelpers.DestroyObjects(m_heldWidgets);

        Destroy(gameObject);
    }
}
