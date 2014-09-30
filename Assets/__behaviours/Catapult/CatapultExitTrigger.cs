using UnityEngine;
using System.Collections;

public class CatapultExitTrigger : MonoBehaviour 
{
    /*
    void OnTriggerExit(Collider other)
    {
        ////////D.Log("CatapultExitTrigger.OnTriggerExit");

        CatapultAmmo ammoBehaviour = other.GetComponent<CatapultAmmo>() as CatapultAmmo;

        if (ammoBehaviour != null && ammoBehaviour.HasLaunched())
        {
            ////////D.Log("CALL RESET");
            CatapultBehaviour.Instance.ResetLineRendererPos();
        }
        else
        {
            ////////D.Log("NO RESET");
            ////////D.Log("ammoBehaviour: " + ammoBehaviour);

            string hasLaunchedMessage = ammoBehaviour != null ? System.String.Format("hasLaunched: {0}", ammoBehaviour.HasLaunched()) : "No Behaviour";
            ////////D.Log("hasLaunched: " + hasLaunchedMessage);
            ////////D.Log("localDelta: " + (other.transform.localPosition - transform.localPosition));
        }
    }
    */

}
