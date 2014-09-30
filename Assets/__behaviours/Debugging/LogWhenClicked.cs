using UnityEngine;
using System.Collections;

public class LogWhenClicked : MonoBehaviour 
{
    void Awake()
    {
        if (!Debug.isDebugBuild)
        {
            Destroy(this);
        }
    }

	void OnClick () 
	{
		//D.Log("Clicked " + name);
	}
}
