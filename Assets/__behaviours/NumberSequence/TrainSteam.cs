using UnityEngine;
using System.Collections;

public class TrainSteam : MonoBehaviour 
{
    [SerializeField]
    private UISprite m_steam;

    public IEnumerator On()
    {
        float speed = 150f;

        m_steam.alpha = 0;
        
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", new Vector3(m_steam.transform.localPosition.x, m_steam.transform.localPosition.y + 1000, m_steam.transform.localPosition.z));
        tweenArgs.Add("speed", speed);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        tweenArgs.Add("islocal", true);
        iTween.MoveTo(gameObject, tweenArgs);

        float enterTweenDuration = 0.5f;
        
        TweenAlpha.Begin(m_steam.gameObject, enterTweenDuration, 1);
        TweenScale.Begin(m_steam.gameObject, enterTweenDuration, Vector3.one);
        
        yield return new WaitForSeconds(enterTweenDuration + 0.5f);
        yield return new WaitForSeconds(3.5f);
        
        float exitTweenDuration = 1f;
        TweenAlpha.Begin(m_steam.gameObject, exitTweenDuration, 0);
        
        yield return new WaitForSeconds(exitTweenDuration);
        
        iTween.Stop(gameObject);
        Destroy(gameObject);
    }

    /*
    public IEnumerator On(Transform newParent)
    {
        float speed = 80f;
        //float speed = 0.2f;
        
        m_steam.alpha = 0;
        
        Hashtable tweenArgs = new Hashtable();
        //tweenArgs.Add("position", new Vector3(m_steam.transform.position.x, m_steam.transform.position.y + 2, m_steam.transform.position.z));
        tweenArgs.Add("position", new Vector3(m_steam.transform.localPosition.x, m_steam.transform.localPosition.y + 1000, m_steam.transform.localPosition.z));
        //tweenArgs.Add("amount", new Vector3(0, 1000, 0));
        tweenArgs.Add("speed", speed);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        tweenArgs.Add("islocal", true);
        iTween.MoveTo(gameObject, tweenArgs);
        //iTween.MoveBy(gameObject, tweenArgs);
        
        float enterTweenDuration = 1f;
        
        TweenAlpha.Begin(m_steam.gameObject, enterTweenDuration, 1);
        TweenScale.Begin(m_steam.gameObject, enterTweenDuration, Vector3.one);

        yield return new WaitForSeconds(enterTweenDuration + 0.5f);

        //m_steam.transform.parent = newParent;
        
        yield return new WaitForSeconds(3.5f);
        
        float exitTweenDuration = 1f;
        TweenAlpha.Begin(m_steam.gameObject, exitTweenDuration, 0);
        
        yield return new WaitForSeconds(exitTweenDuration);
        
        iTween.Stop(gameObject);
        Destroy(gameObject);
    }
    */
}
