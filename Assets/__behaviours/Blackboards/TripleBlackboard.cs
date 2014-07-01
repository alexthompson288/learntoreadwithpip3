using UnityEngine;
using System.Collections;

public class TripleBlackboard : MonoBehaviour {

    [SerializeField]
    UITexture m_texture;
    [SerializeField]
    Transform m_popOffTransform;
    [SerializeField]
    private string m_mainColorString;
    [SerializeField]
    private string m_highlightColorString;
    [SerializeField]
    private UILabel m_label;
	[SerializeField]
	private bool m_showLabel;
	[SerializeField]
	private Vector2 m_widgetOffset;
	[SerializeField]
    private GameObject m_background;
	
	Vector3 m_initialWidgetPosition;
    Vector3 m_initialPopOff;

    private bool m_isShowing = false;
	private string m_letter;

	// Use this for initialization
	void Awake() 
    {
		m_initialWidgetPosition = m_label.transform.localPosition;
        m_initialPopOff = m_popOffTransform.transform.position;
	}

    public void ShowImage(Texture2D texture, string mnemonic, string colorReplace)
    {
		collider.enabled = true;
		
		m_letter = colorReplace;
		
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
		
		if(collider != null)
		{
			collider.enabled = true;
		}
		
        if (!m_showLabel)
        {
            m_label.enabled = false;
        }
        else
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
    }

    public void Hide()
    {
        collider.enabled = false;
		
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
		D.Log("Clicked: " + m_letter);
        //LetterBankGrid.Instance.LetterClicked(this);
    }
	
	public void ShakeFade()
    {
        iTween.ShakePosition(gameObject, Vector3.one * 0.01f, 0.5f);
        TweenAlpha.Begin(m_texture.gameObject, 0.5f, 0.6f);
        TweenColor.Begin(m_background.gameObject, 0.5f, Color.gray);
    }
	
	public void MoveWidgets()
	{
		Vector3 widgetOffset = new Vector3(m_widgetOffset.x, m_widgetOffset.y, 0);
		
		Vector3 localLabelPosition = m_label.transform.localPosition - widgetOffset;
		Vector3 worldLabelPosition = m_label.transform.parent.TransformPoint(localLabelPosition);
		
		Vector3 localTexturePosition = m_texture.transform.localPosition + widgetOffset;
		Vector3 worldTexturePosition = m_texture.transform.parent.TransformPoint(localTexturePosition);
		
		iTween.MoveTo(m_label.gameObject, worldLabelPosition, 1.5f);
		iTween.MoveTo(m_texture.gameObject, worldTexturePosition, 1.5f);
	}
	
	public string GetLetter()
	{
		return m_letter;
	}
}
