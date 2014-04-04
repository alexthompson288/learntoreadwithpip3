using UnityEngine;
using System.Collections;
using Wingrove;

public class CannonPop : MonoBehaviour 
{
    [SerializeField]
    private Transform m_scaleable;
    [SerializeField]
    private float m_scaleIncrement = 0.2f;
    [SerializeField]
    private UITexture m_texture;
    [SerializeField]
    private Texture2D m_celebrationTexture;

    // TODO: Do this in a sensible way that interacts with coordinator 
    int m_score = 0;
    int m_targetScore = 5;

    public void On()
    {
        ++m_score;

        if (m_score < m_targetScore)
        {
            iTween.ShakeRotation(m_scaleable.gameObject, new Vector3(0f, 0f, 30f), 0.8f);
            //iTween.PunchScale(gameObject, Vector3.one * 1.25f, 0.5f);

            Grow();
        } 
        else
        {
            iTween.Stop(gameObject);
            WingroveAudio.WingroveRoot.Instance.PostEvent("SPARKLE_2");
            Celebrate();
        }
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
        iTween.RotateBy(m_scaleable.gameObject, tweenArgs);
    }

    public void Celebrate()
    {
        m_texture.mainTexture = m_celebrationTexture;
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
