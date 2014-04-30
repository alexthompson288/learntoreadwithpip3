using UnityEngine;
using System.Collections;

public class Catapultable : MonoBehaviour 
{
    [SerializeField]
    private ScoreCatapult m_catapult;
    
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "ScoreCatapultOrigin")
        {
            m_catapult.ReleaseLineEnd();
        }
    }
}
