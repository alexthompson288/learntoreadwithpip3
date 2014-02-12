using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class ChooseUserCoordinator : Singleton<ChooseUserCoordinator> 
{
	[SerializeField]
	private GameObject m_chooseUserButtonPrefab;
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;

	GameObject m_selectedButton = null;

	// Use this for initialization
	IEnumerator Start () 
	{
		Dictionary<string, string> users = ChooseUser.Instance.GetUsers();

		//int i = 0;
		Debug.Log("Users");
		foreach(KeyValuePair<string, string> kvp in users)
		{
			Debug.Log(kvp.Key + " - " + kvp.Value);
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_chooseUserButtonPrefab, m_grid.transform);
			newButton.GetComponent<ChooseUserButton>().SetUp(kvp.Key, kvp.Value, m_draggablePanel);
			//++i;
		}

		m_grid.Reposition();

		yield return new WaitForEndOfFrame();

		m_grid.transform.parent.GetComponent<UIDraggablePanel>().ResetPosition();
	}

	public void CreateUser(string user, string imageName)
	{
		GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_chooseUserButtonPrefab, m_grid.transform);
		newButton.GetComponent<ChooseUserButton>().SetUp(user, imageName, m_draggablePanel);

		m_grid.Reposition();
	}

	public void SelectButton (GameObject button)
	{
		Debug.Log("SelectButton()");
		if(button != m_selectedButton)
		{
			Debug.Log("button != m_selectButton");
			if(m_selectedButton != null)
			{
				/*
				ThrobGUIElement oldThrobBehaviour = m_selectedButton.GetComponent<ThrobGUIElement>() as ThrobGUIElement;

				if(throbBehaviour != null)
				{
					Destroy(throbBehaviour);
				}
				*/

				m_selectedButton.GetComponent<ThrobGUIElement>().Off();
			}

			m_selectedButton = button;

			m_selectedButton.GetComponent<ThrobGUIElement>().On();
			//ThrobGUIElement newThrobBehaviour = m_selectedButton.AddComponent<ThrobGUIElement>() as ThrobGUIElement;
		}
	}
}
