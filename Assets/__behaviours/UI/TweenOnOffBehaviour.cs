using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class TweenOnOffBehaviour : MonoBehaviour 
{    
    [SerializeField]
    private Transform m_offLocation;
    [SerializeField]
    private bool m_startOn;
    [SerializeField]
    private bool m_disableCollidersOnStart = true;
    [SerializeField]
    private float m_delay;
    [SerializeField]
    private float m_delayOff;
    [SerializeField]
    private GoEaseType m_easeType;
    [SerializeField]
    private GoEaseType m_easeTypeOff;
    [SerializeField]
    private float m_duration;
    [SerializeField]
    private bool m_destroyAfterOff;
    [SerializeField]
    private string m_audioOn = "";
    [SerializeField]
    private string m_audioOff = "";
    
    
    private Vector3 m_onLocation;
    
    private bool m_isOn;
    private bool m_initialised = false;
    
    public void SetUp(Transform offLocation, Transform onLocation)
    {
        m_offLocation = offLocation;
        m_onLocation = onLocation.position;
        if (!m_initialised)
        {
            transform.position = m_offLocation.position;
            m_initialised = true;
        }
    }
    
    public void SetOffDelay(float delay)
    {
        m_delayOff = delay;
    }
    
    // Use this for initialization
    void Start () 
    {
        m_duration = Mathf.Clamp(m_duration, 0.1f, m_duration);
        
        if (!m_initialised)
        {
            m_onLocation = transform.position;
            transform.position = m_offLocation.position;
            m_initialised = true;
        }
        
        if(m_disableCollidersOnStart)
        {
            Collider[] collider = GetComponentsInChildren<Collider>();
            foreach (Collider cl in collider)
            {
                cl.enabled = false;
            }
        }
        
        if (m_startOn)
        {
            On();
        }
    }
    /*
    void Start () 
    {
        if(Mathf.Approximately(m_duration, 0))
        {
            m_duration = 0.1f;
        }

        if (!m_initialised)
        {
            m_onLocation = transform.position;
            transform.position = m_offLocation.position;
            m_initialised = true;
        }

        Collider[] collider = GetComponentsInChildren<Collider>();
        foreach (Collider cl in collider)
        {
            cl.enabled = false;
        }

        if (m_startOn)
        {
            On();
        }
    }
    */
    
    public void On(bool enableColliders = true)
    {
        Debug.Log("ON");

        if (!m_isOn)
        {
            Debug.Log("TWEENING");
            StartCoroutine(PostAudioOn());
            
            List<GoTween> existing = Go.tweensWithTarget(transform);
            foreach (GoTween gtRemove in existing)
            {
                Go.removeTween(gtRemove);
            }
            m_isOn = true;
            GoTweenConfig gtc = new GoTweenConfig();
            gtc.setDelay(m_delay);
            gtc.setEaseType(m_easeType);
            gtc.vector3Prop("position", m_onLocation);
            GoTween gt = new GoTween(transform, m_duration, gtc);
            Go.addTween(gt);
            
            if(enableColliders)
            {
                Collider[] collider = GetComponentsInChildren<Collider>();
                foreach (Collider cl in collider)
                {
                    cl.enabled = true;
                }
            }
        }
    }
    
    IEnumerator PostAudioOn()
    {
        yield return new WaitForSeconds(m_delay);
        
        if(!System.String.IsNullOrEmpty(m_audioOn))
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(m_audioOn);
        }
        
    }
    
    public void Stop()
    {
        iTween.Stop(gameObject);
        m_isOn = false;
    }
    
    void DestroyObject()
    {
        Destroy(gameObject);
    }
    
    public void Off(bool disableColliders = true)
    {
        if (m_isOn)
        {
            StartCoroutine(PostAudioOff());
            
            List<GoTween> existing = Go.tweensWithTarget(transform);
            foreach(GoTween gtRemove in existing)
            {
                Go.removeTween(gtRemove);
            }
            m_isOn = false;
            GoTweenConfig gtc = new GoTweenConfig();
            gtc.setDelay(m_delayOff);
            gtc.setEaseType(m_easeTypeOff);
            gtc.vector3Prop("position", m_offLocation.position);
            GoTween gt = new GoTween(transform, m_duration, gtc);
            Go.addTween(gt);
            if (m_destroyAfterOff)
            {
                gt.setOnCompleteHandler(c => DestroyObject());
            }
            
            if(disableColliders)
            {
                Collider[] collider = GetComponentsInChildren<Collider>();
                foreach (Collider cl in collider)
                {
                    cl.enabled = false;
                }
            }
        }
    }
    
    IEnumerator PostAudioOff()
    {
        yield return new WaitForSeconds(m_delayOff);
        
        if(!System.String.IsNullOrEmpty(m_audioOff))
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(m_audioOff);
        }
        
    }
    
    public float GetTotalDuration()
    {
        return m_duration + m_delay;
    }
    
    public float GetTotalDurationOff()
    {
        return m_duration + m_delayOff;
    }
    
    public float GetDuration()
    {
        return m_duration;
    }
    
    public float GetDelay()
    {
        return m_delay;
    }
    
    public float GetDelayOff()
    {
        return m_delayOff;
    }
    
    public bool IsOn()
    {
        return m_isOn;
    }
    
    public Transform GetOffLocation()
    {
        return m_offLocation;
    }
}
