using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour 
{
    public delegate void TimerEventHandler(Timer timer);
    public event TimerEventHandler Finished;

    protected float m_timeRemaining;

    public void SetTimeRemaing(float myTimeRemaining)
    {
        m_timeRemaining = myTimeRemaining;
    }

    public void On()
    {
        StartCoroutine("OnCo");
    }

    IEnumerator OnCo()
    {
        while (true)
        {
            m_timeRemaining -= Time.deltaTime;
            Refresh();

            if(m_timeRemaining <= 0 && Finished != null)
            {
                Finished(this);
                StopCoroutine("OnCo");
            }

            yield return null;
        }
    }

    public void Pause()
    {
        StopCoroutine("OnCo");
    }

    protected virtual void Refresh(){}
}
