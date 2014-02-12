using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChooseContentCamera : MonoBehaviour 
{
	List<ChooseContentSet> m_sets = new List<ChooseContentSet>();

	public void AddSet(ChooseContentSet newSet)
	{
		m_sets.Add(newSet);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(transform.hasChanged)// && m_sets.Count > 0)
		{
			SetSetsActive();
		}
	}

	void OnEnable()
	{
		SetSetsActive();
	}

	void SetSetsActive()
	{
		int lowerSet = FindLowerSet();
		
		int lowerBound = lowerSet - 2;
		int upperBound = lowerSet + 2;
		
		for(int i = 0; i < m_sets.Count; ++i)
		{
			if(i >= lowerBound && i <= upperBound)
			{
				m_sets[i].SetButtonsActive(true);
			}
			else
			{
				m_sets[i].SetButtonsActive(false);
			}
		}
	}

	int FindLowerSet ()
	{
		for(int i = 0; i < m_sets.Count; ++i)
		{
			if(transform.position.y > m_sets[i].transform.position.y)
			{
				return i;
			}
		}

		return 0;
	}
}
