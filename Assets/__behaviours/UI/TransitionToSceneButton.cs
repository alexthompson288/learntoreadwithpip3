using UnityEngine;
using System.Collections;

public class TransitionToSceneButton : MonoBehaviour {

    [SerializeField]
    private string m_sceneToTransitionTo;
    [SerializeField]
    private bool m_addToBackStack = true;
    [SerializeField]
    private bool m_transitionBack = false;

    Vector2 m_totalDelta;

    void OnPress(bool isDown)
    {
        if (!isDown)
        {
            ////D.Log("TransitionToSceneButton.OnClick()");
            TransitionScreen ts = (TransitionScreen)GameObject.FindObjectOfType(typeof(TransitionScreen));
            if (ts != null)
            {
                if (m_transitionBack)
                {
                    ts.GoBack(m_addToBackStack);
                } else
                {
                    ts.ChangeLevel(m_sceneToTransitionTo, m_addToBackStack);
                }
            } 
            else
            {
                #if UNITY_IPHONE
                //FlurryBinding.endTimedEvent(Application.loadedLevelName);
                #endif
                
                Application.LoadLevel(m_sceneToTransitionTo);
            }
        }
    }

    /*
    void OnClick()
    {
        ////D.Log("TransitionToSceneButton.OnClick()");
        TransitionScreen ts = (TransitionScreen)GameObject.FindObjectOfType(typeof(TransitionScreen));
        if (ts != null)
        {
            if (m_transitionBack)
            {
				ts.GoBack(m_addToBackStack);
            }
            else
            {
                ts.ChangeLevel(m_sceneToTransitionTo, m_addToBackStack);
            }
        }
        else
        {
#if UNITY_IPHONE
			//FlurryBinding.endTimedEvent(Application.loadedLevelName);
#endif

            Application.LoadLevel(m_sceneToTransitionTo);
        }
    }
    */

	public void SetScene(string sceneToTransitionTo)
	{
		m_sceneToTransitionTo = sceneToTransitionTo;
	}
}
