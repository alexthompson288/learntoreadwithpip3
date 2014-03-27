using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    protected Transform m_parent;
    [SerializeField]
    protected Transform m_startLocation;

    public delegate void TargetHit(Target target, Collider ball);
    public event TargetHit OnTargetHit;
    
    public delegate void TargetDestroy(Target target);
    public event TargetDestroy OnTargetDestroy;

    public delegate void CompleteMove(Target target);
    public event CompleteMove OnCompleteMove;

    protected void InvokeOnCompleteMove()
    {
        if (OnCompleteMove != null)
        {
            OnCompleteMove(this);
        }
    }
    
    protected DataRow m_data;
    public DataRow data
    {
        get
        {
            return m_data;
        }
    }

    public void SetData(DataRow data, Game.Data dataType)
    {
        m_data = data;

        string attributeName = dataType == Game.Data.Phonemes ? "phoneme" : "word";
        m_label.text = m_data [attributeName].ToString();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "CannonBall" && !other.GetComponent<CannonBall>().canDrag && OnTargetHit != null)
        {
            OnTargetHit(this, other);
        }
    }
    
    void OnDestroy()
    {
        if (OnTargetDestroy != null)
        {
            OnTargetDestroy(this);
        }
    }

    public virtual void ApplyHitForce(Transform ball)
    {
        StopAllCoroutines();

        
        rigidbody.isKinematic = false;
        
        rigidbody.AddForce(ball.rigidbody.velocity * 1.2f, ForceMode.VelocityChange);
    }
    
    public virtual void Explode()
    {
        StartCoroutine(ExplodeCo());
    }

    protected IEnumerator ExplodeCo()
    {
        iTween.PunchScale(gameObject, Vector3.one * 1.25f, 0.2f);

        yield return new WaitForSeconds(0.2f);

        transform.localScale = Vector3.zero;
        transform.position = m_startLocation.position;

        iTween.ScaleTo(gameObject, Vector3.one, 0.2f);
    }

    public virtual IEnumerator On(float initialDelay) { yield break; }
    public virtual void Off() {}
    public virtual void SetOffPosition(Vector3 direction, float distance) {}
}
