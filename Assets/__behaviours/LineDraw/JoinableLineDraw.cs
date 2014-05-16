using UnityEngine;
using System.Collections;

public class JoinableLineDraw : LineDraw 
{
    public delegate void JoinableJoinedEvent(JoinableLineDraw self, JoinableLineDraw other);
    public event JoinableJoinedEvent JoinableJoinEventHandler;

    public delegate void JoinablePressedEvent(JoinableLineDraw self, bool pressed);
    public event JoinablePressedEvent JoinablePressEventHandler;

    public delegate void JoinableClickedEvent(JoinableLineDraw self);
    public event JoinableClickedEvent JoinableClickEventHandler;

    [SerializeField]
    private bool m_isPicture;
    [SerializeField]
    private UITexture m_pictureTexture;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private UISprite m_background;

    public bool isPicture
    {
        get
        {
            return m_isPicture;
        }
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
        iTween.ScaleFrom(gameObject, Vector3.zero, 0.3f);
    }

    public void SetUp(string dataType, DataRow myData)
    {
        m_data = myData;

        if (m_isPicture)
        {
            m_pictureTexture.mainTexture = DataHelpers.GetPicture(dataType, m_data);
        } 
        else
        {
            m_label.text = DataHelpers.GetLabelText(dataType, m_data);
        }
    }

    protected override void OnPress(bool pressed)
    {
        base.OnPress(pressed);

        if (JoinablePressEventHandler != null)
        {
            JoinablePressEventHandler(this, pressed);
        }

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
                if (JoinableJoinEventHandler != null && other.GetComponent<JoinableLineDraw>() != null)
                {
                    JoinableJoinEventHandler(this, other.GetComponent<JoinableLineDraw>());
                }
            }

            LineDrawManager.Instance.DestroyLine(this);
        }
    }

    void OnClick()
    {
        if (JoinableClickEventHandler != null)
        {
            JoinableClickEventHandler(this);
        }
    }

    public void Tint(Color newCol)
    {
        m_background.color = newCol;
    }

    public void TransitionOff(Transform targetPosition)
    {
        StartCoroutine(TweenToPos(targetPosition));
    }

    IEnumerator TweenToPos(Transform targetPosition)
    {
        iTween.Stop(gameObject);
        collider.enabled = false;
        iTween.MoveTo(gameObject, targetPosition.position, 1.0f);
        yield return new WaitForSeconds(1.0f);
        iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    public IEnumerator DestroyJoinable()
    {
        iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
        WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEARS");
        yield return new WaitForSeconds(0.9f);
        Destroy(gameObject);
    }
}
