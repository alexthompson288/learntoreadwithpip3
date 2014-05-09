using UnityEngine;
using System.Collections;
using Wingrove;

public class PerspectiveButton : MonoBehaviour 
{
    [SerializeField]
    private Transform m_pressableButton;
    [SerializeField]
    private Transform m_pressedLocation;
    [SerializeField]
    private GameObject m_emptyPrefab;
    [SerializeField]
    private float m_tweenDuration;
    [SerializeField]
    private iTween.EaseType m_easeType = iTween.EaseType.linear;
    [SerializeField]
    private Transform[] m_positionFollowers;
    [SerializeField]
    private Transform m_followerLocation;
    [SerializeField]
    private UISprite[] m_sprites;
    [SerializeField]
    private SimpleSpriteAnim[] m_spriteAnims;
    [SerializeField]
    private string m_pressedAudio = "BUTTON_PRESS";
    [SerializeField]
    private string m_unpressedAudio = "BUTTON_UNPRESS";
    [SerializeField]
    private float m_resetDelay = -1;

    private Transform m_unpressedLocation;

    bool m_isMoving;

    Hashtable m_tweenArgs = new Hashtable();

    public float tweenDuration
    {
        get
        {
            return m_tweenDuration;
        }
    }

    void Awake()
    {
        GameObject unpressedLocationGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, transform);
        m_unpressedLocation = unpressedLocationGo.transform;
        m_unpressedLocation.position = m_pressableButton.position;

        m_tweenArgs.Add("time", m_tweenDuration);
        m_tweenArgs.Add("easetype", m_easeType);
    }

    void Update()
    {
        foreach (Transform follower in m_positionFollowers)
        {
            follower.position = new Vector3(follower.position.x, m_followerLocation.position.y, follower.position.z);
        }
    }

    void OnPress(bool pressed)
    {
        if (pressed)
        {
            StartCoroutine(Press());
        } 
        else
        {
            StartCoroutine(Unpress());
        }
    }

    IEnumerator Press()
    {
        while (m_isMoving)
        {
            yield return null;
        }

        m_isMoving = true;
        collider.enabled = false;

        m_tweenArgs["position"] = m_pressedLocation;

        iTween.MoveTo(m_pressableButton.gameObject, m_tweenArgs);

        WingroveAudio.WingroveRoot.Instance.PostEvent(m_pressedAudio);

        yield return new WaitForSeconds(m_tweenDuration);

        collider.enabled = true;
        m_isMoving = false;
    }

    IEnumerator Unpress()
    {
        while (m_isMoving)
        {
            yield return null;
        }

        collider.enabled = false;
        m_isMoving = true;

        m_tweenArgs["position"] = m_unpressedLocation;

        iTween.MoveTo(m_pressableButton.gameObject, m_tweenArgs);

        foreach (UISprite sprite in m_sprites)
        {
            string spriteName = sprite.spriteName.Substring(0, sprite.spriteName.Length - 1);
            spriteName += "b";
            sprite.spriteName = spriteName;
        }

        foreach (SimpleSpriteAnim spriteAnim in m_spriteAnims)
        {
            spriteAnim.PlayAnimation("ON");
        }

        WingroveAudio.WingroveRoot.Instance.PostEvent(m_unpressedAudio);
        
        yield return new WaitForSeconds(m_tweenDuration);

        collider.enabled = true;
        m_isMoving = false;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        //GUILayout.Label("isMoving: " + m_isMoving);
    }
#endif

    public void Reset()
    {
        foreach (UISprite sprite in m_sprites)
        {
            string spriteName = sprite.spriteName.Substring(0, sprite.spriteName.Length - 1);
            spriteName += "a";
            sprite.spriteName = spriteName;
        }
        
        foreach (SimpleSpriteAnim spriteAnim in m_spriteAnims)
        {
            spriteAnim.PlayAnimation("OFF");
        }
    }

    IEnumerator AutoReset()
    {
        if (m_resetDelay > 0)
        {
            yield return new WaitForSeconds(m_resetDelay);
            Reset();
        } 
        else
        {
            yield break;
        }
    }
}
