using UnityEngine;
using System.Collections;

public class TargetDetachable : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private float m_moveTweenDuration;
    [SerializeField]
    private float m_scaleTweenDuration;
    [SerializeField]
    private Vector3 m_gravity = new Vector3(0, -1.5f, 0);
    [SerializeField]
    private Vector3 m_upwardForce = new Vector3(0, 0.5f, 0);

    public void SetUp(DataRow data)
    {
        if (data [Game.textAttribute] != null)
        {
            m_label.text = data [Game.textAttribute].ToString();
        }
    }

    public void On(Transform target)
    {
        StartCoroutine(OnCo(target));
    }

    IEnumerator OnCo(Transform target)
    {
        transform.parent = target;
        gameObject.layer = target.gameObject.layer;

        iTween.MoveTo(gameObject, target.position, m_moveTweenDuration);

        yield return new WaitForSeconds(m_moveTweenDuration);

        iTween.ScaleTo(gameObject, Vector3.zero, m_scaleTweenDuration);

        yield return new WaitForSeconds(m_scaleTweenDuration);

        Destroy(gameObject);
    }

    /*
    bool m_applyUpwardForce;

    public void Off()
    {
        iTween.ScaleTo(gameObject, Vector3.zero, 0.2f);
    }

    public void PhysicsOn()
    {
        m_applyUpwardForce = true;
        rigidbody.isKinematic = false;
    }

    public void PhysicsOff()
    {
        StartCoroutine(PhysicsOffCo());
    }

    IEnumerator PhysicsOffCo()
    {
        rigidbody.velocity = Vector3.zero;

        yield return null;

        rigidbody.isKinematic = true;
    }

    void FixedUpdate()
    {
        if(m_applyUpwardForce)
        {
            rigidbody.AddForce(m_upwardForce, ForceMode.VelocityChange);
            m_applyUpwardForce = false;
        }
        else
        {
            rigidbody.AddForce(m_gravity, ForceMode.Acceleration);
        }
    }
    */
}
