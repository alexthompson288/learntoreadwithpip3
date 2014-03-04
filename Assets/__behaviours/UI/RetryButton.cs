using UnityEngine;
using System.Collections;

public class RetryButton : MonoBehaviour
{

    void OnClick()
    {
        TransitionScreen ts = (TransitionScreen)GameObject.FindObjectOfType(typeof(TransitionScreen));
        string scene = SessionInformation.Instance.GetRetryScene();
        if (ts != null)
        {
            ts.ChangeLevel(scene, false);
        }
        else
        {
			FlurryBinding.logEvent("Retry game", false);
			FlurryBinding.endTimedEvent(Application.loadedLevelName);
            Application.LoadLevel(scene);
        }
    }
}
