using UnityEngine;
using System.Collections;

public class CreateUserOnButton : MonoBehaviour {

	void OnClick () 
	{
		CreateUserCoordinator.Instance.On();
	}
}
