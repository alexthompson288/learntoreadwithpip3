using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LivesDisplay : MonoBehaviour {

    [SerializeField]
    private string[] m_lifeIconNames;
    [SerializeField]
    private Transform m_offTransform;
    [SerializeField]
    private Vector3 m_spacing;
    [SerializeField]
    private GameObject m_lifeIconPrefab;

    string m_lifeIcon;

    List<GameObject> m_currentLifeIcons = new List<GameObject>();

    public void SetLifeIcon(int index)
    {
        m_lifeIcon = m_lifeIconNames[index];
    }

    public void SetLives(int numLives)
    {
        while (numLives > m_currentLifeIcons.Count)
        {
            GameObject newLifeAdded = SpawningHelpers.InstantiateUnderWithIdentityTransforms
                (m_lifeIconPrefab, transform);
            newLifeAdded.transform.localPosition = m_spacing * m_currentLifeIcons.Count;

            newLifeAdded.GetComponent<TweenOnOffBehaviour>().SetUp(
                m_offTransform, newLifeAdded.transform);
            newLifeAdded.GetComponent<LifeSprite>().SetSprite(m_lifeIcon);
            m_currentLifeIcons.Add(newLifeAdded);
        }
        while ((numLives < m_currentLifeIcons.Count)&&(m_currentLifeIcons.Count>0))
        {
			//D.Log("About to tween off");
                m_currentLifeIcons[m_currentLifeIcons.Count - 1].GetComponent<TweenOnOffBehaviour>().Off();
                m_currentLifeIcons.RemoveAt(m_currentLifeIcons.Count - 1);            
        }
    }
}
