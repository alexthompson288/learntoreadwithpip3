using UnityEngine;
using System.Collections;
using Wingrove;

public class CatapultAmmo : MonoBehaviour 
{
    [SerializeField]
    private string m_pressedAudio = "BUTTON_PRESS";
    [SerializeField]
    private string m_unpressedAudio = "BUTTON_UNPRESS";
    [SerializeField]
    private Vector3 m_gravity = new Vector3(0, -0.5f, 0);
    [SerializeField]
    private bool m_limitSpeed;
    [SerializeField]
    private Vector3 m_maxSpeed = new Vector3 (2, 2, 0);
    [SerializeField]
    UISprite m_sprite;
    
    CatapultBehaviour m_cannon = null;
    
    #if UNITY_EDITOR
    [SerializeField]
    private bool m_debugColorChange;
    #endif

    
    bool m_useGravity = false;
    
    DataRow m_data;
    
    Vector3 m_dragOffset;
    
    private bool m_canDrag = true;
    public bool canDrag
    {
        get
        {
            return m_canDrag;
        }
    }

    bool m_hasLaunched = false;

    public bool HasLaunched()
    {
        return m_hasLaunched;
    }

    public void SetHasLaunchedTrue()
    {
        m_hasLaunched = true;
    }


    void OnTriggerExit(Collider other)
    {
        //Debug.Log("CatapultAmmo.OnTriggerExit");
        if (m_hasLaunched && other.collider.tag == "BallExitTrigger")
        {
            //Debug.Log("CALL RESET");

            CatapultBehaviour.Instance.ResetLineRendererPos();
        } 
        else
        {
            //Debug.Log("NO RESET");
            //Debug.Log("hasLaunched: " + m_hasLaunched);
            //Debug.Log("BallExitTrigger: " + (other.collider.tag == "BallExitTrigger"));
        }
    }



    void OnDestroy()
    {
        m_cannon.RemoveBall(this);
    }
    
    void Start()
    {
        iTween.ScaleFrom (gameObject, Vector3.zero, 0.5f); // This may or may not interfere with rigidbody motion, test it out first
    }
    
    #if UNITY_EDITOR
    void Update()
    {
        if (m_debugColorChange)
        {
            Vector3 cannonDelta = transform.position - m_cannon.ballCentrePos;
            if (cannonDelta.magnitude > m_cannon.maxPull)
            {
                m_sprite.color = Color.red;
            } else if (cannonDelta.magnitude < m_cannon.minPull)
            {
                m_sprite.color = Color.black;
            } else
            {
                m_sprite.color = Color.white;
            }
        }
    }
    #endif

    public Vector3 FindOppositePosition(Transform lineEnd)
    {
        Vector3 ballCentrePos = CatapultBehaviour.Instance.ballCentrePos;
        Vector3 ballCentreLocal = transform.InverseTransformPoint(ballCentrePos);
        
        Vector3 delta = transform.localPosition - ballCentreLocal;

        Vector3 localPos = delta.magnitude < m_cannon.minPull ? Vector3.zero : (delta.normalized * m_sprite.localSize.x / 2);
        
        return transform.TransformPoint(localPos); 
    }
    
    public void SetUp(CatapultBehaviour cannon, DataRow data = null)
    {
        m_cannon = cannon;
        m_data = data;
    }
    
    public void SetCanDrag(bool canDrag)
    {
        m_canDrag = canDrag;
    }
    
    void FixedUpdate()
    {
        rigidbody.AddForce(m_gravity, ForceMode.Acceleration);
        
        if(m_limitSpeed)
        {
            Vector3 velocity = rigidbody.velocity;
            
            velocity.x = Mathf.Clamp(velocity.x, -m_maxSpeed.x, m_maxSpeed.x);
            velocity.y = Mathf.Clamp(velocity.y, -m_maxSpeed.y, m_maxSpeed.y);
            
            rigidbody.velocity = velocity;
        }
    }
    
    void OnPress(bool press)
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent (press ? m_pressedAudio : m_unpressedAudio);
        
        if(m_canDrag)
        {
            if (press)
            {
                WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_STRETCH");

                iTween.Stop(gameObject);
                
                Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
                m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
            }
            else
            {
                m_cannon.OnBallRelease(this);
            }
        }
    }
    
    void OnDrag(Vector2 dragAmount)
    {
        if(m_canDrag)
        {
            Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
            transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;
            
            m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);
            
            Vector3 cannonDelta = transform.position - m_cannon.ballCentrePos;
            if(cannonDelta.magnitude > m_cannon.maxPull)
            {
                transform.position = m_cannon.ballCentrePos + (cannonDelta.normalized * m_cannon.maxPull);
            }
        }
    }
    
    public void OnLaunch()
    {
        rigidbody.isKinematic = false;
        m_canDrag = false;
    }

    public void Explode()
    {
        string spriteName = m_sprite.spriteName.Substring(0, m_sprite.spriteName.Length - 1);
        spriteName += "b";
        m_sprite.spriteName = spriteName;

        collider.enabled = false;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "StopBall")
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("SPLAT_MUSHROOM");

            Explode();
        }
    }
}