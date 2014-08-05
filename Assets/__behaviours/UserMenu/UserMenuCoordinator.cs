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
    [SerializeField]
    private PipButton m_logoutButton;

    UserMenuButton m_currentButton = null;

	// Use this for initialization
	IEnumerator Start () 
	{
        m_logoutButton.Unpressing += OnClickLogout;
        m_doneButton.Unpressing += OnClickDone;

		Dictionary<string, string> users = UserInfo.Instance.GetUsers();

		//D.Log("Users");
		foreach(KeyValuePair<string, string> kvp in users)
		{
			//D.Log(kvp.Key + " - " + kvp.Value);
			GameObject newButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_UserMenuButtonPrefab, m_grid.transform);
			newButton.GetComponent<UserMenuButton>().SetUp(kvp.Key, kvp.Value, m_draggablePanel);
		}

		m_grid.Reposition();

		yield return new WaitForEndOfFrame();

		//m_grid.transform.parent.GetComponent<UIDraggablePanel>().ResetPosition();
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
        //D.Log("UserMenuCoordinator.SelectButton()");
        float tweenDuration = 0.25f;

        if (button != m_currentButton)
        {
            //D.Log("button != m_currentButton");
            if (m_currentButton != null)
            {
                m_currentButton.ChangeSprite(false, tweenDuration);
                iTween.ScaleTo(m_currentButton.gameObject, Vector3.one, tweenDuration);
            }

            m_currentButton = button;

            iTween.ScaleTo(m_currentButton.gameObject, Vector3.one * 1.5f, tweenDuration);
            m_currentButton.ChangeSprite(true, tweenDuration);

            //StartCoroutine(ResetDraggablePanelPosition());
        }
	}

    void OnClickLogout(PipButton button)
    {
        LoginInfo.Instance.Logout();
    }

    void OnClickDone(PipButton button)
    {
        TransitionScreen.Instance.ChangeToDefaultLevel();
    }

    IEnumerator ResetDraggablePanelPosition()
    {
        yield return new WaitForSeconds(0.1f);
        m_grid.transform.parent.GetComponent<UIDraggablePanel>().ResetPosition();
    }
}
