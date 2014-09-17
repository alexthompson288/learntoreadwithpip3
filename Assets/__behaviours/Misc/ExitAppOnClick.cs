using UnityEngine;
using System.Collections;

public class ExitAppOnClick : MonoBehaviour 
{	
	void OnPress (bool isDown) 
    {
        if (!isDown)
        {
            Application.Quit();
        }
	}
}
