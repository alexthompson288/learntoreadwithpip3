using UnityEngine;
using System.Collections;

public class ClockHand : MonoBehaviour 
{
    [SerializeField]
    private int index;

    bool m_isPressing;

    float m_angle
    {
        get
        {
            // TODO: Make this more robust using world->local coordinates instead of num players
            //float modifier = SessionInformation.Instance.GetNumPlayers() == 1 ? transform.up.x : -transform.up.y;

            float modifier = transform.up.x;

            if(SessionInformation.Instance.GetNumPlayers() == 2)
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

            if(m_isPressing)
            {
                D.Log("modifier: " + modifier);
            }

            return modifier > 0 ? Vector3.Angle(transform.parent.up, transform.up) : Vector3.Angle(-transform.parent.up, transform.up) + 180;
        }
    }

    void OnPress(bool pressed)
    {
        m_isPressing = pressed;
    }

    /*
    void Update()
    {
        float modifier = transform.up.x;
        
        if(SessionInformation.Instance.GetNumPlayers() == 2)
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
        
        if(m_isPressing)
        {
            D.Log(transform.parent.up);
            D.Log(System.String.Format("{0} : {1}",Mathf.Approximately(transform.eulerAngles.x, 1), modifier));
        }
    }
    */
    
    void OnDrag(Vector2 delta)
    {
        Ray camPos = UICamera.currentCamera.ScreenPointToRay(
            new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));

        Vector3 fingerPos = new Vector3(camPos.origin.x, camPos.origin.y, transform.position.z);

        transform.up = (fingerPos - transform.position).normalized;

        // TODO: Make only 60 discrete legal values
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
