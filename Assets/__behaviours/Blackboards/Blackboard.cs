using UnityEngine;
using System.Collections;

public class Blackboard : MonoBehaviour 
{
	public delegate void BoardClick(Blackboard blackboard);
	public event BoardClick OnBoardClick;
	
    [SerializeField]
    UITexture m_texture;
    [SerializeField]
    Transform m_popOffTransform;
	[SerializeField]
    private GameObject m_background;
    [SerializeField]
    private string m_mainColorString;
    [SerializeField]
    private string m_highlightColorString;
    [SerializeField]
    private UILabel m_label;
	[SerializeField]
	private bool m_hideLabel;
	[SerializeField]
	private bool m_enableClick;
	[SerializeField]
	private bool m_hideOnClick;
	[SerializeField]
	private bool m_hideOnClickBlocker;
	[SerializeField]
	private Collider m_blocker;
	[SerializeField]
	private bool m_moveWidgetsOnAwake;
	[SerializeField]
	private Vector2 m_widgetOffset;
	
	bool m_movedWidgets = false;
	
	Vector3 m_initialWidgetPosition;
    Vector3 m_initialPopOff;

    private bool m_isShowing = false;
	
	private string m_letter;

	// Use this for initialization
	void Awake() 
    {
		m_initialWidgetPosition = m_label.transform.localPosition;
        m_initialPopOff = m_popOffTransform.transform.position;
		
		if(m_moveWidgetsOnAwake)
		{
			MoveWidgetsInstantly();
		}
	}

    public void ShowImage(Texture2D texture, string mnemonic, string colorReplace, string audioWord = null)
    {
		m_letter = colorReplace;
		
		if(m_enableClick)
		{
			collider.enabled = true;
		}
		
		if(m_hideOnClickBlocker)
		{
			m_blocker.enabled = true;
		}
		
		m_texture.alpha = 1.0f;
        if (m_background != null)
        {
            m_background.GetComponent<UIWidget>().color = Color.white;
        }
		
        m_texture.mainTexture = texture;
        m_texture.enabled = (texture != null);
        iTween.MoveTo(m_popOffTransform.gameObject, transform.position, 1.5f);
        if (!m_isShowing)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
        }
        m_isShowing = true;
		
        if (m_hideLabel)
        {
            m_label.enabled = false;
        }
        else if(colorReplace != null)
        {
            m_label.enabled = true;
            string finalletter = m_mainColorString + mnemonic.Replace(colorReplace, m_highlightColorString + colorReplace + m_mainColorString);
            // do some mad logic to replace splits
            if (colorReplace.Contains("-") && colorReplace.Length == 3)
            {
                int startIndex = 0;
                while (finalletter.IndexOf(colorReplace[0], startIndex) != -1)
                {
                    startIndex = finalletter.IndexOf(colorReplace[0], startIndex);
                    if (startIndex < finalletter.Length + 2)
                    {
                        if (finalletter[startIndex + 2] == colorReplace[2])
                        {
                            finalletter = finalletter.Insert(startIndex + 3, m_mainColorString);
                            finalletter = finalletter.Insert(startIndex + 2, m_highlightColorString);
                            finalletter = finalletter.Insert(startIndex + 1, m_mainColorString);
                            finalletter = finalletter.Insert(startIndex + 0, m_highlightColorString);
                            startIndex += m_mainColorString.Length * 2 + m_highlightColorString.Length * 2;
                        }
                    }

                    startIndex++;
                }
            }
            m_label.text = finalletter;
        }
		else
		{
			m_label.text = mnemonic;
		}
    }

    public void Hide()
    {
		if(collider != null)
		{
        	collider.enabled = false;
		}
		
		if(m_blocker != null)
		{
			m_blocker.enabled = false;
		}
			
        if (m_isShowing)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
        }
        m_isShowing = false;
        iTween.MoveTo(m_popOffTransform.gameObject, m_initialPopOff, 1.5f);
        Resources.UnloadUnusedAssets();
    }

    void OnClick()
    {
		if(OnBoardClick != null)
		{
			OnBoardClick(this);
		}
		
		if(m_hideOnClick)
		{
			Hide();
		}
    }
	
	public void ShakeFade()
    {
        iTween.ShakePosition(gameObject, Vector3.one * 0.01f, 0.5f);
        TweenAlpha.Begin(m_texture.gameObject, 0.5f, 0.6f);
        TweenColor.Begin(m_background.gameObject, 0.5f, Color.gray);
    }

	void MoveWidgetsInstantly()
	{
		if(!m_movedWidgets)
		{
			Vector3 widgetOffset = new Vector3(m_widgetOffset.x, m_widgetOffset.y, 0);
			
			Vector3 localLabelPosition = m_label.transform.localPosition - widgetOffset;
			Vector3 worldLabelPosition = m_label.transform.parent.TransformPoint(localLabelPosition);
			
			Vector3 localTexturePosition = m_texture.transform.localPosition + widgetOffset;
			Vector3 worldTexturePosition = m_texture.transform.parent.TransformPoint(localTexturePosition);
			
			iTween.MoveTo(m_label.gameObject, worldLabelPosition, 0);
			iTween.MoveTo(m_texture.gameObject, worldTexturePosition, 0);
			
			m_movedWidgets = true;
		}
	}
	
	public void MoveWidgets()
	{
		if(!m_movedWidgets)
		{
			Vector3 widgetOffset = new Vector3(m_widgetOffset.x, m_widgetOffset.y, 0);
			
			Vector3 localLabelPosition = m_label.transform.localPosition - widgetOffset;
			Vector3 worldLabelPosition = m_label.transform.parent.TransformPoint(localLabelPosition);
			
			Vector3 localTexturePosition = m_texture.transform.localPosition + widgetOffset;
			Vector3 worldTexturePosition = m_texture.transform.parent.TransformPoint(localTexturePosition);
			
			iTween.MoveTo(m_label.gameObject, worldLabelPosition, 1.5f);
			iTween.MoveTo(m_texture.gameObject, worldTexturePosition, 1.5f);
			
			m_movedWidgets = true;
		}
	}
	
	public void ResetWidgets()
	{
		m_label.transform.localPosition = m_initialWidgetPosition;
		m_texture.transform.localPosition = m_initialWidgetPosition;
		
		m_movedWidgets = false;
	}
	
	public string GetLetter()
	{
		return m_letter;
	}
	
	public bool GetIsShowing()
	{
		return m_isShowing;
	}
}
