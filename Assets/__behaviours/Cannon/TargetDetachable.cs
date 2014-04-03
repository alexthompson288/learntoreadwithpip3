using UnityEngine;
using System.Collections;

public class TargetDetachable : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private bool m_doesTweenUseDuration;
    [SerializeField]
    private float m_moveTweenDuration;
    [SerializeField]
    private float m_moveTweenSpeed;
    [SerializeField]
    private float m_scaleTweenDuration;
    [SerializeField]
    private Vector3 m_gravity = new Vector3(0, -1.5f, 0);
    [SerializeField]
    private Vector3 m_upwardForce = new Vector3(0, 0.5f, 0);

    Transform m_target;

    public void SetUp(DataRow data)
    {
        if (data [Game.textAttribute] != null)
        {
            m_label.text = data [Game.textAttribute].ToString();
        }
    }



    public void On(Transform target)
    {
        m_target = target;

        StartCoroutine(OnCo());
    }

    IEnumerator OnCo()
    {
        transform.parent = m_target;
        gameObject.layer = m_target.gameObject.layer;

        Hashtable moveArgs = new Hashtable();
        moveArgs.Add("position", m_target);
        moveArgs.Add("easetype", iTween.EaseType.linear);

        if (m_doesTweenUseDuration)
        {
            moveArgs.Add("time", m_moveTweenDuration);
        } 
        else
        {
            moveArgs.Add("speed", m_moveTweenSpeed);
            moveArgs.Add("oncomplete", "OnTweenComplete");
        }

        iTween.MoveTo(gameObject, moveArgs);

        if (m_doesTweenUseDuration)
        {
            yield return new WaitForSeconds(m_moveTweenDuration);
            OnTweenComplete();
        }
    }

    void OnTweenComplete()
    {
        StartCoroutine(OnTweenCompleteCo());
    }

    IEnumerator OnTweenCompleteCo()
    {
        m_target.SendMessage("On");
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
