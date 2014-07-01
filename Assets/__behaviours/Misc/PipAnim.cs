using UnityEngine;
using System.Collections;

public class PipAnim : Collector
{
    public override void StartAnim()
    {
        m_anim.PlayAnimation("THUMBS_UP");
    }

    public override void MoveToPos(Vector3 targetPos)
    {
        m_targetPos = targetPos;

        if (m_isMoving)
        {
            StopCoroutine("MoveToPosCo");
        }

        m_isMoving = true;
        StartCoroutine("MoveToPosCo");
    }

    IEnumerator MoveToPosCo()
    {   
        Hashtable tweenArgs = new Hashtable();
        
        tweenArgs.Add("speed", m_moveSpeed);
        tweenArgs.Add("position", m_targetPos);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        
        iTween.Stop(gameObject);
        iTween.MoveTo(gameObject, tweenArgs);
        
        float tweenDuration = TransformHelpers.GetDuration(transform, m_targetPos, m_moveSpeed);
        
        int localScaleX = m_targetPos.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(localScaleX, 1, 1);
        
        D.Log("WALK");
        m_anim.PlayAnimation("WALK", false);
        
        D.Log("tweenDuration: " + tweenDuration);
        
        yield return new WaitForSeconds(tweenDuration);

        m_anim.PlayAnimation("THUMBS_UP", false);
        
        m_isMoving = false;
    }
}
