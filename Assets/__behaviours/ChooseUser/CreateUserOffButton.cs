using UnityEngine;
using System.Collections;

public class CreateUserOffButton : MonoBehaviour {
	[SerializeField]
	private bool m_createUser;

	void OnClick () 
	{
		CreateUserCoordinator.Instance.Off(m_createUser);
	}
}
