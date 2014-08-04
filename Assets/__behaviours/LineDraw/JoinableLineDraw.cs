using UnityEngine;
using System.Collections;

public class JoinableLineDraw : LineDraw 
{
    public delegate void JoinableJoinedEventHandler(JoinableLineDraw self, JoinableLineDraw other);
    public event JoinableJoinedEventHandler JoinableJoined;

    public delegate void JoinablePressedEventHandler(JoinableLineDraw self, bool pressed);
    public event JoinablePressedEventHandler JoinablePressed;

    public delegate void JoinableClickedEventHandler(JoinableLineDraw self);
    public event JoinableClickedEventHandler JoinableClicked;

    [SerializeField]
    private bool m_isPicture;
    [SerializeField]
    private UITexture m_pictureTexture;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private JoinableType m_joinableType;
    [SerializeField]
    private UISprite[] m_sprites;
    [SerializeField]
    private string[] m_spriteNames;
    [SerializeField]
    private WobbleGUIElement m_wobbleBehaviour;

    public JoinableType joinableType
    {
        get
        {
            return m_joinableType;
        }
    }

    public enum JoinableType
    {
        Picture,
        Text,
        Number
    }

    DataRow m_data;
    public DataRow data
    {
        get
        {
            return m_data;
        }
    }

    void Start()
    {
        iTween.ScaleTo(gameObject, Vector3.one, 0.3f);
    }

    public void SetUp(string dataType, DataRow myData)
    {
        m_data = myData;

        if (m_joinableType == JoinableType.Picture)
        {
            m_pictureTexture.mainTexture = DataHelpers.GetPicture(m_data);
            //string shapeName = DataHelpers.GetSpriteName(m_data);
            //m_sprites[0].spriteName = NGUIHelpers.GetRandomSpriteName(m_sprites[0].atlas, shapeName);
        } 
        else if (m_joinableType == JoinableType.Text)
        {
            m_label.text = DataHelpers.GetLabelText(m_data);
            NGUIHelpers.MaxLabelWidth(m_label, 450);
        } 
        else
        {
            System.Array.Sort(m_sprites, CollectionHelpers.LocalLeftToRight_TopToBottom);

            string spriteName = m_spriteNames[Random.Range(0, m_spriteNames.Length)];

            int value = System.Convert.ToInt32(myData["value"]);
            for(int i = 0; i < m_sprites.Length; ++i)
            {
                m_sprites[i].spriteName = spriteName;
                m_sprites[i].gameObject.SetActive(i < value);
            }
        }
    }

    protected override void OnPress(bool pressed)
    {
        base.OnPress(pressed);
        
        if (JoinablePressed != null)
        {
            JoinablePressed(this, pressed);
        }
    }

    public void CheckForJoin()
    {
        Ray camPos = camera.ScreenPointToRay(new Vector3(input.pos.x, input.pos.y, 0));
        
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(camPos, out hit, 1 << gameObject.layer))
        {
            GameObject other = hit.collider.gameObject;
            if (JoinableJoined != null && other.GetComponent<JoinableLineDraw>() != null)
            {
                JoinableJoined(this, other.GetComponent<JoinableLineDraw>());
            }
        }
        
        LineDrawManager.Instance.DestroyLine(this);
    }

    /*
    protected override void OnPress(bool pressed)
    {
        base.OnPress(pressed);

        if (JoinablePressed != null)
        {
            JoinablePressed(this, pressed);
        }

        string eventName = pressed ? "BUTTON_PRESS_SCALE" : "BUTTON_UNPRESS_SCALE";
        WingroveAudio.WingroveRoot.Instance.PostEvent(eventName);

        if (pressed)
        {
            CreateLine();
        } 
        else
        {
            Ray camPos = camera.ScreenPointToRay(new Vector3(input.pos.x, input.pos.y, 0));
                
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(camPos, out hit, 1 << gameObject.layer))
            {
                GameObject other = hit.collider.gameObject;
                if (JoinableJoined != null && other.GetComponent<JoinableLineDraw>() != null)
                {
                    JoinableJoined(this, other.GetComponent<JoinableLineDraw>());
                }
            }

            LineDrawManager.Instance.DestroyLine(this);
        }
    }
    */

    void OnClick()
    {
        if (JoinableClicked != null)
        {
            JoinableClicked(this);
        }
    }

    public void Tint(Color newCol)
    {
        m_background.color = newCol;
    }

    public void EnableWobble(bool enable)
    {
        m_wobbleBehaviour.enabled = enable;

        if (!enable)
        {
            m_wobbleBehaviour.transform.localPosition = Vector3.zero;
        }
    }

    public void TransitionOff(Transform targetPosition)
    {
        EnableWobble(false);
        StartCoroutine(TweenToPos(targetPosition));
    }

    IEnumerator TweenToPos(Transform targetPosition)
    {
        iTween.Stop(gameObject);
        collider.enabled = false;
        iTween.MoveTo(gameObject, targetPosition.position, 1.0f);
        yield return new WaitForSeconds(1.5f);
        iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    public IEnumerator DestroyJoinable()
    {
        iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");
        yield return new WaitForSeconds(0.9f);
        Destroy(gameObject);
    }

    public void EnableCollider(bool enabled)
    {
        collider.enabled = enabled;
    }
}
