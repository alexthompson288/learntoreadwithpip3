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
        ShowDefault();
    }

    protected void ShowDefault()
    {
        m_texture.gameObject.SetActive(m_showPicture && m_texture.mainTexture != null);
        m_label.gameObject.SetActive(!m_showPicture);
        /*
        float pictureAlpha = m_showPicture ? 1 : 0;
        m_texture.color = new Color(m_texture.color.r, m_texture.color.g, m_texture.color.b, pictureAlpha);

        m_texture.gameObject.SetActive(m_texture.mainTexture != null);

        float labelAlpha = m_showPicture ? 0 : 1;
        m_label.color = new Color(m_label.color.r, m_label.color.g, m_label.color.b, labelAlpha);
        */
    }
    
    public void ShowLabel()
    {
        float tweenDuration = 0.1f;
        m_label.color = new Color(m_label.color.r, m_label.color.g, m_label.color.b, 0);
        m_label.gameObject.SetActive(true);
        TweenAlpha.Begin(m_label.gameObject, tweenDuration, 1);
        TweenAlpha.Begin(m_texture.gameObject, tweenDuration, 0);
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

    void OnClick()
    {
        if (m_data != null)
        {
            DataAudio.Instance.PlayShort(m_data);
        }
    }

    public void SetData(DataRow newData, string dataType)
    {
        m_data = newData;
        m_dataType = dataType;
       
        m_label.text = DataHelpers.GetLabelText(m_data);

        if (m_showPicture)
        {
            m_texture.mainTexture = DataHelpers.GetPicture(m_data);
        }

        ShowDefault();
    }

    void OnCollisionEnter(Collision other)
    {
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

        rigidbody.AddForce(ball.rigidbody.velocity.normalized * 2, ForceMode.VelocityChange);
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

    public void ParentOff() 
    {
        MyStopCoroutines();

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("scale", Vector3.zero);
        tweenArgs.Add("time", 0.2f);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        iTween.ScaleTo(transform.parent.gameObject, tweenArgs);
    }

    protected virtual IEnumerator OnCo(float initialDelay) { yield break; }
    public virtual void Off() {}
    public virtual void SetOffPosition(Vector3 direction, float distance) {}
}
