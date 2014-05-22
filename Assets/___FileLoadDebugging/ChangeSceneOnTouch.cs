using UnityEngine;
using System.Collections;

public class ChangeSceneOnTouch : MonoBehaviour 
{
    [SerializeField]
    private string m_sceneToTransitionTo;

	void Update()
	{
		if ((Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began) || Input.GetMouseButtonDown(0)) 
		{
            /*
			int sceneIndex = Application.loadedLevel;

			++sceneIndex;

			if(sceneIndex >= Application.levelCount)
			{
				sceneIndex = 0;
			}

			Application.LoadLevel(sceneIndex);
            */  

            TransitionScreen.Instance.ChangeLevel(m_sceneToTransitionTo, false);
		}
	}
}
