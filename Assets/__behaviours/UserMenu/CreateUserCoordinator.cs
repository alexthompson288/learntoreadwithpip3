using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CreateUserCoordinator : Singleton<CreateUserCoordinator> 
{
	[SerializeField]
	GameObject m_choosePictureButtonPrefab;
	[SerializeField]
	TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;
	[SerializeField]
	private UILabel m_inputLabel;
    [SerializeField]
    private string[] m_spriteNames;
	
	string m_imageName;

	UserPictureButton m_selectedPictureButton;

	List<GameObject> m_spawnedPictures = new List<GameObject>();

    void Awake()
    {
        m_tweenBehaviour.gameObject.SetActive(true);
    }

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.N))
		{
			On();
		}
		else if(Input.GetKeyDown(KeyCode.M))
		{
			Off (false);
		}
	}

	public void On()
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        foreach (string spriteName in m_spriteNames)
        {
            GameObject newPicture = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_choosePictureButtonPrefab, m_grid.transform);
            m_spawnedPictures.Add(newPicture);
            
            newPicture.GetComponent<UserPictureButton>().SetUp(spriteName, m_draggablePanel);
        }

        m_grid.Reposition();
        
        m_tweenBehaviour.On();     
	}

	public void Off (bool createUser) 
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");

		if(createUser && !UserInfo.Instance.HasUser(m_inputLabel.text))
		{
            if(System.String.IsNullOrEmpty(m_imageName))
            {
                m_imageName = m_spawnedPictures[0].GetComponent<UserPictureButton>().GetSpriteNameA();
            }

			UserInfo.Instance.CreateUser(m_inputLabel.text, m_imageName);
			UserMenuCoordinator.Instance.CreateUser(m_inputLabel.text, m_imageName);
		}

		m_tweenBehaviour.Off();

		StartCoroutine(OffCo());
	}

	IEnumerator OffCo()
	{
		yield return new WaitForSeconds(0.5f);
		for(int i = 0; i < m_spawnedPictures.Count; ++i)
		{
			Destroy(m_spawnedPictures[i]);
		}

		m_spawnedPictures.Clear();
	}

	public void OnPictureChoose (UserPictureButton button)
	{
		if(m_selectedPictureButton != button)
		{
            m_imageName = button.GetSpriteNameA();

			if(m_selectedPictureButton != null)
			{
                m_selectedPictureButton.ChangeSprite(false);
			}

			m_selectedPictureButton = button;
            m_selectedPictureButton.ChangeSprite(true);
		}
	}
}
