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

    string m_dataType;

    public void SetData(DataRow newData, string dataType)
    {
        m_data = newData;
        m_dataType = dataType;
       
        m_label.text = DataHelpers.GetLabelText(m_dataType, m_data);

        m_texture.mainTexture = DataHelpers.GetPicture(dataType, newData);
        m_texture.gameObject.SetActive(m_showPicture && m_texture.mainTexture != null);
    }
    
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("TARGET ENTER: " + other.name + " - " + other.transform.parent.name);
        CatapultAmmo ammoBehaviour = other.GetComponent<CatapultAmmo>() as CatapultAmmo;

        if (ammoBehaviour != null && !ammoBehaviour.canDrag && OnTargetHit != null)
        {
            OnTargetHit(this, other);
        } 
    }

    public void OnHit()
    {
        if (m_background != null)
        {
            m_background.spriteName = DataHelpers.GetLinkedSpriteName(m_background.spriteName);
        }
    }

    protected void ResetSpriteName()
    {
        if (m_background != null && m_background.spriteName.LastIndexOf('b') == m_background.spriteName.Length - 1)
        {
            m_background.spriteName = DataHelpers.GetLinkedSpriteName(m_background.spriteName);
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

    /*
    public void DetachableOn(Transform detachableTarget)
    {
        GameObject newDetachable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_detachablePrefab, m_detachableLocation);

        newDetachable.GetComponent<TargetDetachable>().SetUp(m_data);
        newDetachable.GetComponent<TargetDetachable>().On(detachableTarget);
    }
    */

    public GameObject SpawnDetachable()
    {
        GameObject newDetachable = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_detachablePrefab, m_detachableLocation);
        
        newDetachable.GetComponent<TargetDetachable>().SetUp(m_label.text);

        return newDetachable;
    }

    public virtual IEnumerator On(float initialDelay) { yield break; }
    public virtual void Off() {}
    public virtual void SetOffPosition(Vector3 direction, float distance) {}
}
