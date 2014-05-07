using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class UserMenuCoordinator : Singleton<UserMenuCoordinator> 
{
	[SerializeField]
	private GameObject m_UserMenuButtonPrefab;
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;

	GameObject m_selectedButton = null;

	// Use this for initialization
	IEnumerator Start () 
	{
		Dictionary<string, string> users = UserInfo.Instance.GetUsers();

		//int i = 0;
		Debug.Log("Users");
		foreach(KeyValuePair<string, string> kvp in users)
		{
			Debug.Log(kvp.Key + " - " + kvp.Value);
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_UserMenuButtonPrefab, m_grid.transform);
			newButton.GetComponent<UserMenuButton>().SetUp(kvp.Key, kvp.Value, m_draggablePanel);
			//++i;
		}

		m_grid.Reposition();

		yield return new WaitForEndOfFrame();

		m_grid.transform.parent.GetComponent<UIDraggablePanel>().ResetPosition();
	}

	public void CreateUser(string user, string imageName)
	{
        StartCoroutine(CreateUserCo(user, imageName));
	}

    IEnumerator CreateUserCo(string user, string imageName)
    {
        GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_UserMenuButtonPrefab, m_grid.transform);
        newButton.GetComponent<UserMenuButton>().SetUp(user, imageName, m_draggablePanel);
        
        m_grid.Reposition();

        yield return new WaitForEndOfFrame();
        
        m_grid.transform.parent.GetComponent<UIDraggablePanel>().ResetPosition();
    }

	public void SelectButton (GameObject button)
	{
        /*
		Debug.Log("SelectButton()");
		if(button != m_selectedButton)
		{
			Debug.Log("button != m_selectButton");
			if(m_selectedButton != null)
			{
				m_selectedButton.GetComponent<ThrobGUIElement>().Off();
			}

			m_selectedButton = button;

			m_selectedButton.GetComponent<ThrobGUIElement>().On();
		}
        */      
	}
}
