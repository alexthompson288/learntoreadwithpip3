using UnityEngine;
using System.Collections;

public class CreateUserOffButton : MonoBehaviour 
{
	[SerializeField]
	private bool m_createUser;

	void OnPress (bool isDown) 
	{
        if (!isDown)
        {
            CreateUserCoordinator.Instance.Off(m_createUser);
        }
	}
}
