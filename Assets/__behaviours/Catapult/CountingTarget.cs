using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CountingTarget : Target 
{
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private float m_scaleTweenDuration = 0.3f;

    int m_currentLocatorIndex = 0;

    List<GameObject> m_storedAmmo = new List<GameObject>();

    public int locatorCount
    {
        get
        {
            return m_locators.Length;
        }
    }

    public override void Off()
    {
        StartCoroutine(OffCo());
    }

    IEnumerator OffCo()
    {
        collider.enabled = false;
        iTween.ScaleTo(gameObject, Vector3.zero, m_scaleTweenDuration);

        yield return new WaitForSeconds(m_scaleTweenDuration);

        for (int i = m_storedAmmo.Count - 1; i > -1; --i)
        {
            Destroy(m_storedAmmo[i]);
        }

        m_storedAmmo.Clear();

        m_currentLocatorIndex = 0;
    }

    public override IEnumerator On(float initialDelay)
    {
        iTween.ScaleTo(gameObject, Vector3.one, m_scaleTweenDuration);
        collider.enabled = true;
        yield return null;
    }

    public void StoreAmmo(Collider other)
    {
        if (m_currentLocatorIndex < m_locators.Length)
        {
            iTween.MoveTo(collider.gameObject, m_locators [m_currentLocatorIndex].position, 0.25f);
            m_storedAmmo.Add(other.gameObject);
            ++m_currentLocatorIndex;
        } 
        else
        {
            Destroy(other.gameObject);
        }
    }
}
