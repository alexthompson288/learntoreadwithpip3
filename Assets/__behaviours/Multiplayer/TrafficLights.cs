using UnityEngine;
using System.Collections;

public class TrafficLights : MonoBehaviour 
{
    [SerializeField]
    private TweenOnOffBehaviour m_tweenBehaviour;
    [SerializeField]
    private UISprite m_red;
    [SerializeField]
    private UISprite m_yellow;
    [SerializeField]
    private UISprite m_green;

#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(On());
        }
    }
#endif

    public IEnumerator On()
    {
        m_tweenBehaviour.On();

        yield return new WaitForSeconds(m_tweenBehaviour.GetDuration());

        WingroveAudio.WingroveRoot.Instance.PostEvent("PIP_READY_STEADY_GO");

        yield return new WaitForSeconds(0.11f); // 0.11

        m_red.spriteName = NGUIHelpers.GetLinkedSpriteName(m_red.spriteName);
        WingroveAudio.WingroveRoot.Instance.PostEvent("DING_QUIET");

        yield return new WaitForSeconds(1.14f); // 1.25

        m_yellow.spriteName = NGUIHelpers.GetLinkedSpriteName(m_yellow.spriteName);
        WingroveAudio.WingroveRoot.Instance.PostEvent("DING_QUIET");

        yield return new WaitForSeconds(1.65f); // 2.90

        m_green.spriteName = NGUIHelpers.GetLinkedSpriteName(m_green.spriteName);
        WingroveAudio.WingroveRoot.Instance.PostEvent("DING_QUIET");

        yield return new WaitForSeconds(0.1f);

        m_tweenBehaviour.Off();
    }
}
