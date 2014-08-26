using UnityEngine;
using System.Collections;

public class CreateUserOnButton : MonoBehaviour {

	void OnPress (bool isDown) 
	{
        if (!isDown)
        {
            CreateUserCoordinator.Instance.On();
        }
	}
}
