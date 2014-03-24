using UnityEngine;
using System.Collections;

public class CastleBehaviour : MonoBehaviour 
{
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;

    public void On()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>() as Rigidbody[];
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }

        m_tweenBehaviour.On();
    }
}
