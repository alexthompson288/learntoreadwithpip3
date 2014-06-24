using UnityEngine;
using System.Collections;
using System.Linq;
using Wingrove;

public class GameWidget : MonoBehaviour 
{
    [SerializeField]
    private BoxCollider m_collider;
    [SerializeField]
    protected UILabel m_label;
    [SerializeField]
    protected UIWidget m_icon;
    [SerializeField]
    private UISprite m_background;
    [SerializeField]
    private string[] m_backgroundsStateA;
    [SerializeField]
    private string[] m_backgroundsStateB;
    [SerializeField]
    private bool m_linkABIndex;
    [SerializeField]
    private string m_pressedAudio;
    [SerializeField]
    private string m_unpressedAudio;
    [SerializeField]
    private float m_scaleTweenDuration;
    [SerializeField]
    private float m_positionTweenDuration = 0.5f;
    [SerializeField]
    private bool m_canDrag = true;
    [SerializeField]
    private ClickReaction m_clickReaction;

    enum ClickReaction
    {
        None,
        Offset,
        Scale,
        ChangeSprite
    }

    protected DataRow m_data;
    public DataRow data
    {
        get
        {
            return m_data;
        }
    }

    public string labelText
    {
        get
        {
            return m_label.text;
        }
    }

    public float backgroundWidth
    {
        get
        {
            return m_background.width;
        }
    }

    public float labelWidth
    {
        get
        {
            return NGUIHelpers.GetLabelWidth(m_label);
        }
    }

    public float scaleTweenDuration
    {
        get
        {
            return m_scaleTweenDuration;
        }
    }

    public float positionTweenDuration
    {
        get
        {
            return m_positionTweenDuration;
        }
    }

    public void SetPressedAudio(string myPressedAudio)
    {
        m_pressedAudio = myPressedAudio;
    }

    public void SetUnpressedAudio(string myUnpressedAudio)
    {
        m_unpressedAudio = myUnpressedAudio;
    }

    Vector3 m_dragOffset;

    Vector3 m_startPosition;

    bool m_hasDragged;

    int m_backgroundIndex;

    void OnDestroy()
    {
        if (PrivateDestroying != null)
        {
            PrivateDestroying(this);
        }
    }

    // SetUp methods
    void Awake()
    {
        transform.localScale = Vector3.zero;
        iTween.ScaleTo(gameObject, Vector3.one, m_scaleTweenDuration);
    }

    public virtual void SetUp(DataRow data) {}

    public void SetUp(string myDataType, DataRow myData, Texture2D iconTexture, bool changeBackgroundWidth = false)
    {
        if (m_icon != null)
        {
            m_icon.mainTexture = iconTexture;
        }

        SetUp(myDataType, myData, changeBackgroundWidth);
    }

    public void SetUp(string myDataType, DataRow myData, UIAtlas iconAtlas, string iconSpritename, bool changeBackgroundWidth = false)
    {
        if (m_icon != null)
        {
            UISprite iconSprite = m_icon as UISprite;
            if(iconSprite != null)
            {
                iconSprite.atlas = iconAtlas;
                iconSprite.spriteName = iconSpritename;
            }
        }

        SetUp(myDataType, myData, changeBackgroundWidth);
    }

    public void SetUp(string myDataType, DataRow myData, bool changeBackgroundWidth)
    {
        m_data = myData;

        if (m_label != null)
        {
            m_label.text = DataHelpers.GetLabelText(m_data);
        }

        if (changeBackgroundWidth)
        {
            ChangeBackgroundWidth();
        }

        SetUpBackground();
    }

    public void SetUp(string labelText, bool changeBackgroundWidth)
    {
        m_label.text = labelText;

        SetUpBackground();

        if (changeBackgroundWidth)
        {
            ChangeBackgroundWidth();
        }
    }

    public void SetUpBackground()
    {
        if (m_backgroundsStateA.Length == 0)
        {
            m_backgroundsStateA = new string[1];
            m_backgroundsStateA[0] = m_background.spriteName;
        }
        
        if (m_backgroundsStateB.Length == 0)
        {
            m_backgroundsStateB = new string[1];
            m_backgroundsStateB[0] = NGUIHelpers.GetLinkedSpriteName(m_backgroundsStateA[0]);
        }

        m_background.spriteName = m_backgroundsStateA[Random.Range(0, m_backgroundsStateA.Length)];
    }

    protected void ChangeBackgroundWidth()
    {
        if(m_label != null)
        {
            int newWidth = (int)((m_label.font.CalculatePrintedSize(m_label.text, false, UIFont.SymbolStyle.None).x + 60) * m_label.transform.localScale.x);

            if(newWidth < 150)
            {
                newWidth = 150;
            }
            
            if(m_background != null)
            {
                m_background.width = newWidth;
            }
        }
        
        if (m_collider != null && m_background != null)
        {
            m_collider.size = m_background.localSize;
        } 
    }

    // Player Interactions
    void OnDrag(Vector2 delta)
    {
        if(m_canDrag)
        {
            Ray camPos = UICamera.currentCamera.ScreenPointToRay(
                new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
            transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;
            
            m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);
            
            m_hasDragged = true;
        }
    }

    void OnPress(bool pressed)
    {
        WingroveAudio.WingroveRoot.Instance.PostEvent (pressed ? m_pressedAudio : m_unpressedAudio);

        if (pressed)
        {
            iTween.Stop(gameObject);
            
            m_startPosition = transform.position;
            Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
            m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
            
            m_hasDragged = false;

            if(!m_canDrag && !m_isRunningClickDown)
            {
                ClickDownReaction();
            }
        } 
        else
        {
            if(m_hasDragged)
            {
                if(PrivateDragReleased != null)
                {
                    PrivateDragReleased(this);
                }
            }
            else
            {
                StartCoroutine(ClickUpReaction());
            }
        }
    }

    void ClickDownReaction()
    {
        switch(m_clickReaction)
        {
            case ClickReaction.Offset:
                StartCoroutine(ClickOffsetDown());
                break;
            case ClickReaction.Scale:
                StartCoroutine(ClickScaleDown());
                break;
            case ClickReaction.ChangeSprite:
                ChangeBackgroundState(true);
                break;
            default:
                break;
        }
    }

    bool m_isRunningClickDown = false;

    IEnumerator ClickUpReaction()
    {
        EnableCollider(false);

        while (m_isRunningClickDown)
        {
            yield return null;
        }

        switch(m_clickReaction)
        {
            case ClickReaction.Offset:
                yield return StartCoroutine(ClickOffsetUp());
                break;
            case ClickReaction.Scale:
                yield return StartCoroutine(ClickScaleUp());
                break;
            default:
                break;
        }

        EnableCollider(true);

        if(PrivateClicked != null)
        {
            PrivateClicked(this);
        }
    }

    IEnumerator ClickOffsetDown()
    {
        m_isRunningClickDown = true;

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("islocal", true);
        
        tweenArgs.Add("position", new Vector3(transform.localPosition.x, transform.localPosition.y - 15));
        
        float tweenDuration = 0.2f;
        tweenArgs.Add("time", tweenDuration);
        
        iTween.MoveTo(gameObject, tweenArgs);
        
        yield return new WaitForSeconds(tweenDuration);
        
        m_isRunningClickDown = false;
    }

    IEnumerator ClickOffsetUp()
    {
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("islocal", true);
        
        tweenArgs.Add("position", m_startPosition);
        
        float tweenDuration = 0.2f;
        tweenArgs.Add("time", tweenDuration);
        
        iTween.MoveTo(gameObject, tweenArgs);
        
        yield return new WaitForSeconds(tweenDuration);
    }

    IEnumerator ClickScaleDown()
    {
        m_isRunningClickDown = true;

        float tweenDuration = 0.2f;
        
        iTween.ScaleTo(gameObject, Vector3.one * 0.8f, tweenDuration);

        yield return new WaitForSeconds(tweenDuration);

        m_isRunningClickDown = false;
    }

    IEnumerator ClickScaleUp()
    {
        float tweenDuration = 0.2f;
        
        iTween.ScaleTo(gameObject, Vector3.one, tweenDuration);
        
        yield return new WaitForSeconds(tweenDuration);
    }

    // Miscellaneous Methods
    public void Off()
    {
        StartCoroutine(OffCo());
    }
    
    public IEnumerator OffCo()
    {
        collider.enabled = false;
        iTween.Stop(gameObject);
        iTween.ScaleTo(gameObject, Vector3.zero, m_scaleTweenDuration);
        
        yield return new WaitForSeconds(m_scaleTweenDuration);
        
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    public void EnableDrag(bool enable)
    {
        m_canDrag = enable;
    }
    
    public void EnableCollider(bool enable)
    {
        m_collider.enabled = enable;
    }

    public void TweenToStartPos()
    {
        iTween.MoveTo(gameObject, m_startPosition, m_positionTweenDuration);
    }

    public void TweenToPos(Vector3 pos)
    {
        iTween.MoveTo(gameObject, pos, m_positionTweenDuration);
    }

    public void ChangeBackgroundState(bool stateB)
    {
        string[] spriteNames = stateB ? m_backgroundsStateB : m_backgroundsStateA;

        if (spriteNames.Length > 0)
        {
            m_background.spriteName = (m_linkABIndex && spriteNames.Length > m_backgroundIndex) ? spriteNames [m_backgroundIndex] : spriteNames [Random.Range(0, spriteNames.Length)];
        } 
        else 
        {
            m_background.spriteName = m_linkABIndex ? EnviroLoader.Instance.GetContainerOnName(m_background.spriteName) : EnviroLoader.Instance.GetContainerOnName();
        } 
    }

    public void TintWhite()
    {
        Tint(Color.white);
    }

    public void TintGray()
    {
        Tint(Color.gray);
    }

    void Tint(Color col)
    {
        col.a = m_background.color.a;
        TweenColor.Begin(gameObject, 0.1f, col);
    }

    public void FadeBackground(bool fadeOut)
    {
        float alpha = fadeOut ? 0 : 1;
        TweenAlpha.Begin(m_background.gameObject, 0.5f, alpha);
    }

    public void Shake()
    {
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("islocal", true);
        tweenArgs.Add("amount", Vector3.one * 20);
        tweenArgs.Add("time", 0.5f);

        iTween.ShakePosition(gameObject, tweenArgs);
    }

    // Events
    public delegate void GameWidgetEventHandler(GameWidget widget);

    private event GameWidgetEventHandler PrivateDestroying;
    public event GameWidgetEventHandler Destroying
    {
        add
        {
            if(PrivateDestroying == null || !PrivateDestroying.GetInvocationList().Contains(value))
            {
                PrivateDestroying += value;
            }
        }
        remove
        {
            PrivateDestroying -= value;
        }
    }

    private event GameWidgetEventHandler PrivateClicked;
    public event GameWidgetEventHandler Clicked
    {
        add
        {
            if(PrivateClicked == null || !PrivateClicked.GetInvocationList().Contains(value))
            {
                PrivateClicked += value;
            }
        }
        remove
        {
            PrivateClicked -= value;
        }
    }

    private event GameWidgetEventHandler PrivateDragReleased;
    public event GameWidgetEventHandler DragReleased
    {
        add
        {
            if(PrivateDragReleased == null || !PrivateDragReleased.GetInvocationList().Contains(value))
            {
                PrivateDragReleased += value;
            }
        }
        remove
        {
            PrivateDragReleased -= value;
        }
    }

    public event GameWidgetEventHandler AllReleaseInteractions
    {
        add
        {
            Clicked += value;
            DragReleased += value;
        }
        remove
        {
            Clicked -= value;
            DragReleased -= value;
        }
    }
}
