using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CountingTarget : Target 
{
    [SerializeField]
    private Transform[] m_locators;
    [SerializeField]
    private float m_scaleTweenDuration = 0.3f;

    List<GameObject> m_storedAmmo = new List<GameObject>();

    public int storedAmmoCount
    {
        get
        {
            return m_storedAmmo.Count;
        }
    }

    public int locatorCount
    {
        get
        {
            return m_locators.Length;
        }
    }

    void Awake()
    {
        System.Array.Sort(m_locators, CollectionHelpers.ComparePosYThenX);
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
    }

    protected override IEnumerator OnCo(float initialDelay)
    {
        iTween.ScaleTo(gameObject, Vector3.one, m_scaleTweenDuration);
        collider.enabled = true;
        yield return null;
    }

    public void StoreAmmo(Collider ammo)
    {
        ammo.rigidbody.velocity = Vector3.zero;
        ammo.rigidbody.isKinematic = true;
        ammo.GetComponent<CatapultAmmo>().enabled = false;

        if (storedAmmoCount < m_locators.Length)
        {
            ammo.transform.parent = m_locators[storedAmmoCount];
            iTween.MoveTo(ammo.gameObject, m_locators [storedAmmoCount].position, 0.25f);
            m_storedAmmo.Add(ammo.gameObject);
        } 
        else
        {
            Destroy(ammo.gameObject);
        }
    }
}
