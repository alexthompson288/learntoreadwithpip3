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
            return m_label.font.CalculatePrintedSize(m_label.text, false, UIFont.SymbolStyle.None).x * m_label.transform.localScale.x;
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

    Vector3 m_dragOffset;

    Vector3 m_startPosition;

    bool m_hasDragged;

    int m_backgroundIndex;

    void Start()
    {
        //iTween.ScaleFrom(gameObject, Vector3.zero, m_scaleTweenDuration);
        transform.localScale = Vector3.zero;

        iTween.ScaleTo(gameObject, Vector3.one, m_scaleTweenDuration);
    }

    public virtual void SetUp(DataRow data) {}

    public void SetUp(string dataType, DataRow newData, Texture2D iconTexture, bool changeBackgroundWidth = false)
    {
        if (m_icon != null)
        {
            m_icon.mainTexture = iconTexture;
        }

        SetUp(dataType, newData, changeBackgroundWidth);
    }

    public void SetUp(string dataType, DataRow newData, UIAtlas iconAtlas, string iconSpritename, bool changeBackgroundWidth = false)
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

        SetUp(dataType, newData, changeBackgroundWidth);
    }

    public void SetUp(string dataType, DataRow newData, bool changeBackgroundWidth)
    {
        m_data = newData;

        if (m_label != null)
        {
            m_label.text = DataHelpers.GetLabelText(dataType, m_data);
        }

        if (changeBackgroundWidth)
        {
            ChangeBackgroundWidth();
        }

        transform.localScale = Vector3.one;
        iTween.ScaleFrom(gameObject, Vector3.zero, 0.5f);

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
            m_backgroundsStateB[0] = DataHelpers.GetLinkedSpriteName(m_backgroundsStateA[0]);
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
        } 
        else
        {
            if(m_hasDragged)
            {
                if(OnDragRelease != null)
                {
                    OnDragRelease(this);
                }
            }
            else
            {
                switch(m_clickReaction)
                {
                    case ClickReaction.Offset:
                        StartCoroutine(OnClickCo());
                        break;
                    case ClickReaction.ChangeSprite:
                        ChangeBackgroundState();
                        if(OnWidgetClick != null)
                        {
                            OnWidgetClick(this);
                        }
                        break;
                    default:
                        if(OnWidgetClick != null)
                        {
                            OnWidgetClick(this);
                        }
                        break;
                }

            }
        }
    }

    IEnumerator OnClickCo()
    {
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("islocal", true);

        tweenArgs.Add("position", new Vector3(transform.localPosition.x, transform.localPosition.y - 15));
        
        float tweenDuration = 0.2f;
        tweenArgs.Add("time", tweenDuration);

        iTween.MoveTo(gameObject, tweenArgs);
        yield return new WaitForSeconds(tweenDuration);

        tweenArgs ["position"] = m_startPosition;
        tweenArgs ["islocal"] = false;

        iTween.MoveTo(gameObject, tweenArgs);
        yield return new WaitForSeconds(tweenDuration);
        
        if(OnWidgetClick != null)
        {
            OnWidgetClick(this);
        }
    }

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

    public void TweenToStartPos()
    {
        iTween.MoveTo(gameObject, m_startPosition, m_positionTweenDuration);
    }

    public void TweenToPos(Vector3 pos)
    {
        iTween.MoveTo(gameObject, pos, m_positionTweenDuration);
    }

    public void FadeBackground(bool fadeOut)
    {
        float alpha = fadeOut ? 0 : 1;
        TweenAlpha.Begin(m_background.gameObject, 0.5f, alpha);
    }

    public void Off()
    {
        StartCoroutine(OffCo());
    }

    IEnumerator OffCo()
    {
        collider.enabled = false;
        iTween.Stop(gameObject);
        iTween.ScaleTo(gameObject, Vector3.zero, m_scaleTweenDuration);
        
        yield return new WaitForSeconds(m_scaleTweenDuration);
        
        Destroy(gameObject);
    }

    public void ChangeBackgroundState(bool stateB = true)
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

    public void EnableDrag(bool enable)
    {
        m_canDrag = enable;
    }

    public void EnableCollider(bool enable)
    {
        m_collider.enabled = enable;
    }

    public void Tint(Color newCol)
    {
        m_background.color = newCol;
    }

    public void Shake()
    {
        iTween.ShakePosition(gameObject, Vector3.one * 0.01f, 0.5f);
    }

    public delegate void GameWidgetEvent(GameWidget widget);

    private event GameWidgetEvent OnWidgetClick;
    public event GameWidgetEvent onClick
    {
        add
        {
            if(OnWidgetClick == null || !OnWidgetClick.GetInvocationList().Contains(value))
            {
                OnWidgetClick += value;
            }
        }
        remove
        {
            OnWidgetClick -= value;
        }
    }

    private event GameWidgetEvent OnFlick;
    public event GameWidgetEvent onFlick
    {
        add
        {
            if(OnFlick == null || !OnFlick.GetInvocationList().Contains(value))
            {
                OnFlick += value;
            }
        }
        remove
        {
            OnFlick -= value;
        }
    }

    private event GameWidgetEvent OnDragRelease;
    public event GameWidgetEvent onDragRelease
    {
        add
        {
            if(OnDragRelease == null || !OnDragRelease.GetInvocationList().Contains(value))
            {
                OnDragRelease += value;
            }
        }
        remove
        {
            OnDragRelease -= value;
        }
    }

    public event GameWidgetEvent onAll
    {
        add
        {
            onClick += value;
            onFlick += value;
            onDragRelease += value;
        }
        remove
        {
            onClick -= value;
            onFlick -= value;
            onDragRelease -= value;
        }
    }
}
