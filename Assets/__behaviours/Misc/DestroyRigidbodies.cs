using UnityEngine;
using System.Collections;

public class DestroyRigidbodies : MonoBehaviour 
{
    public delegate void DestroyingEventHandler(GameObject Go);
    public event DestroyingEventHandler DestroyingRB;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            if(DestroyingRB != null)
            {
                DestroyingRB(gameObject);
            }

            Destroy(other.gameObject);
        }
    }
}
