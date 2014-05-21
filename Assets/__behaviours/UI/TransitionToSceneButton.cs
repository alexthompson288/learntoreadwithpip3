using UnityEngine;
using System.Collections;

public class TransitionToSceneButton : MonoBehaviour {

    [SerializeField]
    private string m_sceneToTransitionTo;
    [SerializeField]
    private bool m_addToBackStack = true;
    [SerializeField]
    private bool m_transitionBack = false;

    void OnClick()
    {
		Debug.Log(name + " - TransitionToSceneButton.OnClick()");
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

	public void SetScene(string sceneToTransitionTo)
	{
		m_sceneToTransitionTo = sceneToTransitionTo;
	}
}
