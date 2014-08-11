using UnityEngine;
using System.Collections;

public class ClockHand : MonoBehaviour 
{
    public delegate void ClockHandEventHandler(ClockHand hand);
    public event ClockHandEventHandler Dragged;

    [SerializeField]
    private Clock m_clock;
    [SerializeField]
    private bool m_isMinuteHand;
    
    float m_angle
    {
        get
        {
            float modifier = transform.up.x;
            
            if(!Mathf.Approximately(transform.parent.up.x, 0))
            {
                if(Mathf.Approximately(transform.parent.up.x, 1))
                {
                    modifier = -transform.up.y;
                }
                else
                {
                    modifier = transform.up.y;
                }
            }
            
            return modifier > 0 ? Vector3.Angle(transform.parent.up, transform.up) : Vector3.Angle(-transform.parent.up, transform.up) + 180;
        }
    }

    void OnPress(bool pressed)
    {
        if (pressed)
        {
            m_clock.OnHandPress(this);
        }
        else
        {
            m_clock.OnHandUnpress(this);
        }
    }
    
    void OnDrag(Vector2 delta)
    {
        if (Dragged != null)
        {
            Dragged(this);
        }
    }

    public void Move()
    {
        Ray camPos = UICamera.currentCamera.ScreenPointToRay(
            new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
        
        Vector3 fingerPos = new Vector3(camPos.origin.x, camPos.origin.y, transform.position.z);
        
        transform.up = (fingerPos - transform.position).normalized;
    }
    
    public int GetMinutes()
    {
        int minutes = Mathf.RoundToInt(m_angle / 6);
        
        if (minutes > 59)
        {
            minutes = 0;
        }
        
        return minutes;
    }

    public int GetHours(bool wrapAround = true)
    {
        int hours = Mathf.FloorToInt(m_angle / 30);
        
        if (wrapAround && hours == 0)
        {
            hours = 12;
        }
        
        return hours;
    }

    public float GetProportional()
    {
        if (m_isMinuteHand)
        {
            return m_angle / 360;
        } 
        else
        {
            return (m_angle / 30f) - (float)GetHours(false);
        }
    }

    public void Follow(float proportional)
    {
        if (m_isMinuteHand)
        {
            float angle = proportional * 360;
            SetPositionFromAngle(angle);
        }
        else
        {
            float proportionalHours = (float)GetHours(false) + proportional;

            if(proportional < 0.1f && GetProportional() > 0.9f)
            {
                proportionalHours += 1f;
            }
            else if(proportional > 0.9f && GetProportional() < 0.1f)
            {
                proportionalHours -= 1f;
            }

            float angle = proportionalHours * 30;
            SetPositionFromAngle(angle);
        }
    }

    void SetPositionFromAngle(float targetAngle)
    {
        float rotationAngle = m_angle - targetAngle;
        transform.Rotate(new Vector3(0, 0, rotationAngle));
    }
    
    public string GetHourString()
    {
        int hours = GetHours();
        
        return MathHelpers.GetDigitCount(hours) == 2 ? hours.ToString() : System.String.Format("0{0}", hours);
    }
    
    public string GetMinuteString()
    {
        int minutes = GetMinutes();
        
        return MathHelpers.GetDigitCount(minutes) == 2 ? minutes.ToString() : System.String.Format("0{0}", minutes);
    }
}

/*
using UnityEngine;
using System.Collections;

public class ClockHand : MonoBehaviour 
{
    float m_angle
    {
        get
        {
            float modifier = transform.up.x;

            if(!Mathf.Approximately(transform.parent.up.x, 0))
            {
                if(Mathf.Approximately(transform.parent.up.x, 1))
                {
                    modifier = -transform.up.y;
                }
                else
                {
                    modifier = transform.up.y;
                }
            }

            return modifier > 0 ? Vector3.Angle(transform.parent.up, transform.up) : Vector3.Angle(-transform.parent.up, transform.up) + 180;
        }
    }
    
    void OnDrag(Vector2 delta)
    {
        Ray camPos = UICamera.currentCamera.ScreenPointToRay(
            new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));

        Vector3 fingerPos = new Vector3(camPos.origin.x, camPos.origin.y, transform.position.z);

        transform.up = (fingerPos - transform.position).normalized;
    }

    public int GetMinutes()
    {
        int minutes = Mathf.RoundToInt(m_angle / 6);

        if (minutes > 59)
        {
            minutes = 0;
        }

        return minutes;
    }

    public int GetHours()
    {
        int hours = Mathf.RoundToInt((m_angle - 15) / 30);

        if (hours == 0)
        {
            hours = 12;
        }

        return hours;
    }

    public string GetHourString()
    {
        int hours = GetHours();

        return MathHelpers.GetDigitCount(hours) == 2 ? hours.ToString() : System.String.Format("0{0}", hours);
    }

    public string GetMinuteString()
    {
        int minutes = GetMinutes();
        
        return MathHelpers.GetDigitCount(minutes) == 2 ? minutes.ToString() : System.String.Format("0{0}", minutes);
    }
}
*/
