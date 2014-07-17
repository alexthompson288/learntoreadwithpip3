using UnityEngine;
using System.Collections;

public class CatapultCountingTarget : MonoBehaviour 
{
    public delegate void TargetEventHandler(CatapultCountingTarget target);
    public event TargetEventHandler Destroying;

    public delegate void TargetHitEventHandler(CatapultCountingTarget target, Collider other);
    public event TargetHitEventHandler TargetHit;

    [SerializeField]
    private UISprite m_sprite;
    [SerializeField]
    private string[] m_spriteNames;

    bool m_hasBeenHit = false;

	void Start () 
    {
        if (m_spriteNames.Length > 0)
        {
            m_sprite.spriteName = m_spriteNames[Random.Range(0, m_spriteNames.Length)];
        }

        StartCoroutine("On");
	}

    void OnCollisionEnter(Collision other)
    {
        CatapultAmmo ammoBehaviour = other.gameObject.GetComponent<CatapultAmmo>() as CatapultAmmo;
        
        if (ammoBehaviour != null && !ammoBehaviour.canDrag)
        {
            ammoBehaviour.Explode();
            ammoBehaviour.StopMomentum();
            StartCoroutine(OnHit(other.collider));
        }
    }

    IEnumerator OnHit(Collider other)
    {
        StopCoroutine("On");
        iTween.Stop(gameObject);
        yield return null;
        collider.isTrigger = false;
        rigidbody.isKinematic = false;
        rigidbody.velocity = Vector3.zero;
        m_hasBeenHit = true;

        if(TargetHit != null)
        {
            TargetHit(this, other);
        }
    }
    
    void FixedUpdate()
    {
        if (m_hasBeenHit)
        {
            rigidbody.AddForce(new Vector3(0, -2f, 0));
        }
    }
    
    IEnumerator On()
    {
        float speed = 400f;
        Vector3 distance = new Vector3(2000, 0, 0);
        
        Hashtable tweenArgs = new Hashtable();

        tweenArgs.Add("position", transform.localPosition + distance);
        tweenArgs.Add("speed", speed);
        tweenArgs.Add("islocal", true);
        tweenArgs.Add("easetype", iTween.EaseType.linear);

        iTween.MoveTo(gameObject, tweenArgs);

        yield return new WaitForSeconds(TransformHelpers.GetDuration(transform, transform.localPosition + distance, speed, true));

        iTween.Stop(gameObject);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        iTween.Stop(gameObject);

        if (Destroying != null)
        {
            Destroying(this);
        }
    }
}
