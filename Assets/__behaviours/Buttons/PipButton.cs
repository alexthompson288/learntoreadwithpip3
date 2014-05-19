using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PipButton : MonoBehaviour 
{
    public delegate void PressEventHandler (PipButton button);
    public event PressEventHandler Pressing;
    public event PressEventHandler Unpressing;
    public event PressEventHandler Unpressed;
    
    
    // Shared Variables
    [SerializeField]
    private bool m_autoReset = true;
    [SerializeField]
    private int m_int;
    [SerializeField]
    private string m_string;
    [SerializeField]
    private UISprite m_pressableButton;
    [SerializeField]
    private float m_pressDuration = 0.3f;
    [SerializeField]
    private List<string> m_pressedAudio = new List<string>();
    [SerializeField]
    private List<string> m_unpressedAudio = new List<string>();
    [SerializeField]
    private ColorInfo.PipColor m_pipColor = ColorInfo.PipColor.White;
    [SerializeField]
    private UIWidget[] m_additionalColoredWidgets;
    [SerializeField]
    private SimpleSpriteAnim m_sheenAnimation;
    [SerializeField]
    private SimpleSpriteAnim m_pressAnimation;

    bool m_isTransitioning;

    DataRow m_data;

    
    // PositionChange Variables and Method
    [SerializeField]
    private bool m_changePosition;
    [SerializeField]
    private Transform m_pressedLocation;
    [SerializeField]
    private Transform m_positionFollower;
    [SerializeField]
    private Transform m_followerLocation;
    [SerializeField]
    private GameObject m_emptyPrefab;
    [SerializeField]
    private iTween.EaseType m_positionEaseType = iTween.EaseType.linear;
    
    Transform m_unpressedLocation;
    Hashtable m_positionTweenArgs = new Hashtable();

    IEnumerator UpdateFollowerPosition()
    {
        m_positionFollower.position = m_followerLocation.position;
        yield return null;
        StartCoroutine("UpdateFollowerPosition");
    }
    
    
    // SpriteChange Variables and Method
    [SerializeField]
    private bool m_changeSprite;
    [SerializeField]
    private string m_pressedSpriteName;
    
    string m_unpressedSpriteName;

    public void ChangeSprite(bool toStateB)
    {
        m_pressableButton.spriteName = toStateB ? m_pressedSpriteName : m_unpressedSpriteName;
    }
    
    // ColorChange Variables
    [SerializeField]
    private bool m_changeColor;
    [SerializeField]
    private Color m_pressedColor = Color.white;
    
    Color m_unpressedColor;
    

    // Shared Methods
    public void AddPressedAudio(string audioEvent)
    {
        m_pressedAudio.Add(audioEvent);
    }

    public void AddUnpressedAudio(string audioEvent)
    {
        m_unpressedAudio.Add(audioEvent);
    }

    public void SetPipColor(ColorInfo.PipColor newPipColor, bool changeSpriteColor)
    {
        m_pipColor = newPipColor;

        if (changeSpriteColor)
        {
            Color col = ColorInfo.GetColor(m_pipColor);

            m_pressableButton.color = col;

            foreach(UIWidget widget in m_additionalColoredWidgets)
            {
                widget.color = col;
            }
        }
    }

    public void SetData(DataRow newData)
    {
        m_data = newData;
        Debug.Log("Button.SetData: " + m_data);
    }

    public DataRow data
    {
        get
        {
            return m_data;
        }
    }
    
    public ColorInfo.PipColor pipColor
    {
        get
        {
            return m_pipColor;
        }
    }

    public int GetInt()
    {
        return m_int;
    }

    public void SetInt(int newInt)
    {
        m_int = newInt;
    }

    public string GetString()
    {
        return m_string;
    }

    public void SetString(string newString)
    {
        m_string = newString;
    }

    public void SetPipColor(ColorInfo.PipColor newPipColor)
    {
        m_pipColor = newPipColor;
    }

    void Start()
    {
        if (m_pressableButton != null)
        {
            Color col = ColorInfo.GetColor(m_pipColor);
            m_pressableButton.color = col;
            
            foreach (UIWidget widget in m_additionalColoredWidgets)
            {
                widget.color = col;
            }
            
            if (m_changePosition)
            {
                GameObject unpressedLocationGo = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, transform);
                m_unpressedLocation = unpressedLocationGo.transform;
                m_unpressedLocation.position = m_pressableButton.transform.position;
                
                m_positionTweenArgs.Add("time", m_pressDuration);
                m_positionTweenArgs.Add("easetype", m_positionEaseType);
            }
            
            if (m_changeSprite)
            {
                m_unpressedSpriteName = m_pressableButton.spriteName;
                
                if (System.String.IsNullOrEmpty(m_pressedSpriteName))
                {
                    m_pressedSpriteName = DataHelpers.GetLinkedSpriteName(m_unpressedSpriteName);
                }
            }
            
            if (m_changeColor)
            {
                m_unpressedColor = m_pressableButton.color;
                
                // If unpressedColor hasn't been set then we want to set it to be a bit darker than the pressed color
                if (m_pressedColor == Color.white)
                {
                    m_unpressedColor = m_pressedColor;
                }
                
                if (Mathf.Approximately(m_pressedColor.r, m_unpressedColor.r) && Mathf.Approximately(m_pressedColor.g, m_unpressedColor.g) && Mathf.Approximately(m_pressedColor.b, m_unpressedColor.b))
                {
                    m_pressedColor = m_unpressedColor;
                    
                    for (int i = 0; i < 3; ++i) // Only change 0(r), 1(g), 2(b). 3 is alpha.
                    {
                        m_pressedColor [i] = Mathf.Clamp01(m_pressedColor [i] - 0.2f);
                    }
                }
            }
            
            if (m_sheenAnimation != null)
            {
                m_sheenAnimation.OnAnimFinish += OnSheenFinish;
                m_sheenAnimation.PlayAnimation("ON");
                StartCoroutine(PlaySheen());
            }
        }
    }
    
    void OnSheenFinish(string animName)
    {
        StartCoroutine(PlaySheen());
    }
    
    IEnumerator PlaySheen()
    {
        yield return new WaitForSeconds(Random.Range(3f, 9f));
        m_sheenAnimation.PlayAnimation("ON");
    }
    
    void OnPress(bool pressed)
    {
        if (pressed)
        {
            StartCoroutine(Press());
        } 
        else if(m_autoReset)
        {
            StartCoroutine(Unpress());
        }
    }
    
    IEnumerator Press()
    {
        while (m_isTransitioning)
        {
            yield return null;
        }
        
        if (Pressing != null)
        {
            Pressing(this);
        }
        
        m_isTransitioning = true;
        collider.enabled = false;

        foreach (string audioEvent in m_pressedAudio)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
        
        if (m_changePosition)
        {
            if (m_positionFollower != null)
            {
                StartCoroutine("UpdateFollowerPosition");
            }

            m_positionTweenArgs ["position"] = m_pressedLocation;
            iTween.MoveTo(m_pressableButton.gameObject, m_positionTweenArgs);
        }
        
        if (m_changeSprite)
        {
            m_pressableButton.spriteName = m_pressedSpriteName;
        }
        
        if (m_changeColor)
        {
            m_pressableButton.color = m_pressedColor;
        }
        
        if (m_pressAnimation != null)
        {
            m_pressAnimation.PlayAnimation("ON");
        }
        
        yield return new WaitForSeconds(m_pressDuration);
        
        collider.enabled = true;
        m_isTransitioning = false;
    }
    
    public IEnumerator Unpress()
    {
        while (m_isTransitioning)
        {
            yield return null;
        }
        
        if (Unpressing != null)
        {
            Unpressing(this);
        }
        
        m_isTransitioning = true;
        collider.enabled = false;

        foreach(string audioEvent in m_unpressedAudio)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
        
        if (m_changePosition)
        {
            m_positionTweenArgs ["position"] = m_unpressedLocation;
            iTween.MoveTo(m_pressableButton.gameObject, m_positionTweenArgs);
        }

        if (m_changeColor)
        {
            m_pressableButton.color = m_unpressedColor;
        }
        
        yield return new WaitForSeconds(m_pressDuration);
        
        if (Unpressed != null)
        {
            Unpressed(this);
        }

        if (m_changeSprite)
        {
            m_pressableButton.spriteName = m_unpressedSpriteName;
        }

        if (m_changePosition && m_positionFollower != null)
        {
            StopCoroutine("UpdateFollowerPosition");
        }
        
        collider.enabled = true;
        m_isTransitioning = false;
    }

    public void EnableCollider(bool enable)
    {
        collider.enabled = enable;
    }
}