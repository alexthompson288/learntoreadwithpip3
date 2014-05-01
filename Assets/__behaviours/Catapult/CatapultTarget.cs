using UnityEngine;
using System.Collections;

public class CatapultTarget : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private Vector3 m_localMovement = Vector3.zero;
    [SerializeField]
    private Vector2 m_localMovementVariance = Vector2.zero;

    enum MovementType
    {
        Pop,
        Continuous,
    }

    public delegate void TargetHit(CatapultTarget target, Collider ball);
    public event TargetHit OnTargetHit;

    public delegate void DestroyGo(CatapultTarget target);
    public event DestroyGo OnDestroyGo;

    private DataRow m_data;
    public DataRow data
    {
        get
        {
            return m_data;
        }
    }

    void Start()
    {
        m_localMovement *= Random.Range(m_localMovementVariance.x, m_localMovementVariance.y);
    }

    void Update()
    {
        transform.localPosition += m_localMovement;
    }

    public void SetUp(DataRow newData, Game.Data dataType)
    {
        m_data = newData;

        string attributeName = dataType == Game.Data.Phonemes ? "phoneme" : "word";
        m_label.text = m_data [attributeName].ToString();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "CatapultAmmo" && !other.GetComponent<CatapultAmmo>().canDrag && OnTargetHit != null)
        {
            OnTargetHit(this, other);
        }
    }

    void OnDestroy()
    {
        if (OnDestroyGo != null)
        {
            OnDestroyGo(this);
        }
    }

    public void ApplyHitForce(Transform ball)
    {
        m_localMovement = Vector3.zero;
        StopAllCoroutines();

        rigidbody.isKinematic = false;
   
        rigidbody.AddForce(ball.rigidbody.velocity * 1.2f, ForceMode.VelocityChange);
    }

    public void Explode()
    {
        iTween.PunchScale(gameObject, Vector3.one * 1.25f, 0.2f);
        Destroy(gameObject);
    }
}
