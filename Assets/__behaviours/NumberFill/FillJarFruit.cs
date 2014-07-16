using UnityEngine;
using System.Collections;

public class FillJarFruit : MonoBehaviour 
{
    public delegate void FillJarFruitEventHandler(FillJarFruit fruit);
    public event FillJarFruitEventHandler Destroying;

    [SerializeField]
    private Vector3 m_gravity = new Vector3(0, -0.2f, 0);
    [SerializeField]
    private UISprite m_sprite;
    [SerializeField]
    private string[] m_spriteNames;

    bool m_applyInitialForce = true;

    void Awake()
    {
        if (m_spriteNames.Length > 0)
        {
            m_sprite.spriteName = m_spriteNames[Random.Range(0, m_spriteNames.Length)];
        }
    }

    void FixedUpdate()
    {
        if (!rigidbody.isKinematic)
        {
            Vector3 force = m_gravity;

            if(m_applyInitialForce)
            {
                rigidbody.AddTorque(new Vector3(0, 0, Random.Range(-30f, 30f)));
                m_applyInitialForce = false;
            }

            rigidbody.AddForce(force);
        }
    }

    public void EnableRB(bool enable)
    {
        if (!enable)
        {
            rigidbody.velocity = Vector3.zero;
        }

        rigidbody.isKinematic = enable;
    }

    void OnDestroy()
    {
        if (Destroying != null)
        {
            Destroying(this);
        }
    }
}
