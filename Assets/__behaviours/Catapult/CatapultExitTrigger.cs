using UnityEngine;
using System.Collections;

public class CatapultExitTrigger : MonoBehaviour 
{
    /*
    void OnTriggerExit(Collider other)
    {
        Debug.Log("CatapultExitTrigger.OnTriggerExit");

        CatapultAmmo ammoBehaviour = other.GetComponent<CatapultAmmo>() as CatapultAmmo;

        if (ammoBehaviour != null && ammoBehaviour.HasLaunched())
        {
            Debug.Log("CALL RESET");
            CatapultBehaviour.Instance.ResetLineRendererPos();
        }
        else
        {
            Debug.Log("NO RESET");
            Debug.Log("ammoBehaviour: " + ammoBehaviour);

            string hasLaunchedMessage = ammoBehaviour != null ? System.String.Format("hasLaunched: {0}", ammoBehaviour.HasLaunched()) : "No Behaviour";
            Debug.Log("hasLaunched: " + hasLaunchedMessage);
            Debug.Log("localDelta: " + (other.transform.localPosition - transform.localPosition));
        }
    }
    */

}
