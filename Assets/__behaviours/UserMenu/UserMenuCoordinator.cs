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
    [SerializeField]
    private PipButton m_doneButton;

    UserMenuButton m_currentButton = null;

	// Use this for initialization
	IEnumerator Start () 
	{
        m_doneButton.Unpressing += OnClickDone;

		Dictionary<string, string> users = UserInfo.Instance.GetUsers();

		Debug.Log("Users");
		foreach(KeyValuePair<string, string> kvp in users)
		{
			Debug.Log(kvp.Key + " - " + kvp.Value);
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_UserMenuButtonPrefab, m_grid.transform);
			newButton.GetComponent<UserMenuButton>().SetUp(kvp.Key, kvp.Value, m_draggablePanel);
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

	public void SelectButton (UserMenuButton button)
	{
        if (button != m_currentButton)
        {
            if (m_currentButton != null)
            {
                m_currentButton.ChangeSprite(false);
            }

            m_currentButton = button;

            m_currentButton.ChangeSprite(true);
        }
	}

    void OnClickDone(PipButton button)
    {
        TransitionScreen.Instance.ChangeLevel("NewVoyage", false);
    }
}
