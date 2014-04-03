using UnityEngine;
using System.Collections;

public class CannonPop : MonoBehaviour 
{
    [SerializeField]
    private Transform m_scaleable;
    [SerializeField]
    private float m_scaleIncrement = 0.15f;

    public void On()
    {
        iTween.ShakeRotation(gameObject, new Vector3(0f, 0f, 30f), 0.8f);
        //iTween.PunchScale(gameObject, Vector3.one * 1.25f, 0.5f);

        Grow();
    }

    public void Grow()
    {
        float newScale = m_scaleable.localScale.x + m_scaleIncrement;

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("time", 0.25f);
        tweenArgs.Add("scale", Vector3.one * newScale);
        tweenArgs.Add("easetype", iTween.EaseType.linear);

        iTween.ScaleTo(m_scaleable.gameObject, tweenArgs);
    }

    public void Spin()
    {
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("time", 0.5f);
        tweenArgs.Add("amount", new Vector3(0, 0, 1));
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        iTween.RotateBy(gameObject, tweenArgs);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Grow();
        }
    }
#endif
}
