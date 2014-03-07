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
#if UNITY_IPHONE
			FlurryBinding.logEvent("Retry game", false);

			System.Collections.Generic.Dictionary<string, string> ep = new System.Collections.Generic.Dictionary<string, string>();
			ep.Add("Name", Application.loadedLevelName);
			FlurryBinding.endTimedEvent("NewLevel", ep);
#endif

            Application.LoadLevel(scene);
        }
    }
}
