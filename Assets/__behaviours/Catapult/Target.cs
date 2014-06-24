using UnityEngine;
using System.Collections;
using Wingrove;

public class Target : MonoBehaviour 
{
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UITexture m_texture;
    [SerializeField]
    protected Transform m_parent;
    [SerializeField]
    protected Transform m_startLocation;
    [SerializeField]
    private GameObject m_detachablePrefab;
    [SerializeField]
    private Transform m_detachableLocation;
    [SerializeField]
    private bool m_isAlwaysCorrect;
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private bool m_showPicture;

    protected bool m_isOn = false;

    public void SetShowPicture(bool showPicture)
    {
        m_showPicture = showPicture;
        m_texture.gameObject.SetActive(m_showPicture);
        m_label.gameObject.SetActive(!m_showPicture);
    }

    public bool isAlwaysCorrect
    {
        get
        {
            return m_isAlwaysCorrect;
        }
    }

    public delegate void TargetHitEventHandler(Target target, Collider ball);
    public event TargetHitEventHandler TargetHit;
    
    public delegate void TargetEventHandler(Target target);
    public event TargetEventHandler Destroying;
    public event TargetEventHandler MoveCompleted;


    protected void InvokeMoveCompleted()
    {
        if (MoveCompleted != null)
        {
            MoveCompleted(this);
        }
        //Debug.Log("Finished Invoke: " + transform.localScale.x);
    }
    
    protected DataRow m_data;
    public DataRow data
    {
        get
        {
            return m_data;
        }
    }

    string m_dataType;

    public void SetData(DataRow newData, string dataType)
    {
        m_data = newData;
        m_dataType = dataType;
       
        m_label.text = DataHelpers.GetLabelText(m_data);

        m_texture.mainTexture = DataHelpers.GetPicture(newData);
        m_texture.gameObject.SetActive(m_showPicture && m_texture.mainTexture != null);
    }

    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Target.OnCollisionEnter()");
        //Debug.Log("TARGET ENTER: " + other.name + " - " + other.transform.parent.name);
        if (!Mathf.Approximately(transform.localScale.y, 0))
        {
            CatapultAmmo ammoBehaviour = other.gameObject.GetComponent<CatapultAmmo>() as CatapultAmmo;
            
            if (ammoBehaviour != null && !ammoBehaviour.canDrag && TargetHit != null)
            {
                TargetHit(this, other.collider);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Target.OnTriggerEnter()");
        //Debug.Log("TARGET ENTER: " + other.name + " - " + other.transform.parent.name);
        if (!Mathf.Approximately(transform.localScale.y, 0))
        {
            CatapultAmmo ammoBehaviour = other.GetComponent<CatapultAmmo>() as CatapultAmmo;

            if (ammoBehaviour != null && !ammoBehaviour.canDrag && TargetHit != null)
            {
                TargetHit(this, other);
            }
        }
    }


    public void OnHit()
    {
        if (m_background != null)
        {
            m_background.spriteName = NGUIHelpers.GetLinkedSpriteName(m_background.spriteName);
        }
    }

    protected void ResetSpriteName()
    {
        if (m_background != null && m_background.spriteName.LastIndexOf('b') == m_background.spriteName.Length - 1)
        {
            m_background.spriteName = NGUIHelpers.GetLinkedSpriteName(m_background.spriteName);
        }
    }
    
    void OnDestroy()
    {
        if (Destroying != null)
        {
            Destroying(this);
        }
    }

    public virtual void ApplyHitForce(Transform ball)
    {
        StopAllCoroutines();

        rigidbody.isKinematic = false;

        Debug.Log("ball.speed: " + ball.rigidbody.velocity.magnitude);

        rigidbody.AddForce(ball.rigidbody.velocity.normalized * 2, ForceMode.VelocityChange);
        //rigidbody.AddForce(ball.rigidbody.velocity * 1.2f, ForceMode.VelocityChange);
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

    public GameObject SpawnDetachable()
    {
        GameObject newDetachable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_detachablePrefab, m_detachableLocation);

        newDetachable.transform.parent = null;
        
        newDetachable.GetComponent<TargetDetachable>().SetUp(m_label.text);

        return newDetachable;
    }

    public virtual void MyStopCoroutines()
    {
        StopAllCoroutines();
        m_isOn = false;
    }

    public void On(float initialDelay)
    {
        if (!m_isOn)
        {
            m_isOn = true;
            StartCoroutine(OnCo(initialDelay));
        }
    }

    protected virtual IEnumerator OnCo(float initialDelay) { yield break; }
    public virtual void Off() {}
    public virtual void SetOffPosition(Vector3 direction, float distance) {}
}
