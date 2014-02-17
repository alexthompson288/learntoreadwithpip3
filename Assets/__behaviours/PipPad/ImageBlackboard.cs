using UnityEngine;
using System.Collections;

public class ImageBlackboard : MonoBehaviour {

    [SerializeField]
    UITexture m_texture;
    [SerializeField]
    Transform m_popOffTransform;
    [SerializeField]
    private string m_mainColorString;
    [SerializeField]
    private string m_highlightColorString;
    [SerializeField]
    private UILabel m_wordLabel;
	[SerializeField]
	private Vector2 m_widgetOffset;
    [SerializeField]
    private GameObject m_background;
	[SerializeField]
	private bool m_hideOnClick;
	
	Vector3 m_initialWidgetPosition;
    Vector3 m_initialPopOff;

    private bool m_isShowing = false;
    string m_audioWord;
	
	bool m_movedWidgets = false;

	// Use this for initialization
	void Awake() 
    {
		m_initialWidgetPosition = m_wordLabel.transform.localPosition;
        m_initialPopOff = m_popOffTransform.transform.position;
	}

    public void ShowImage(Texture2D texture, string word, string colorReplace, string audioWord)
    {
        m_audioWord = audioWord;
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
        if (word == null)
        {
            m_wordLabel.enabled = false;
        }
        else
        {
            m_wordLabel.enabled = true;
            string finalWord = m_mainColorString + word.Replace(colorReplace, m_highlightColorString + colorReplace + m_mainColorString);
            // do some mad logic to replace splits
            if (colorReplace.Contains("-") && colorReplace.Length == 3)
            {
                int startIndex = 0;
                while (finalWord.IndexOf(colorReplace[0], startIndex) != -1)
                {
                    startIndex = finalWord.IndexOf(colorReplace[0], startIndex);
                    if (startIndex < finalWord.Length + 2)
                    {
                        if (finalWord[startIndex + 2] == colorReplace[2])
                        {
                            finalWord = finalWord.Insert(startIndex + 3, m_mainColorString);
                            finalWord = finalWord.Insert(startIndex + 2, m_highlightColorString);
                            finalWord = finalWord.Insert(startIndex + 1, m_mainColorString);
                            finalWord = finalWord.Insert(startIndex + 0, m_highlightColorString);
                            startIndex += m_mainColorString.Length * 2 + m_highlightColorString.Length * 2;
                        }
                    }

                    startIndex++;
                }
            }
            m_wordLabel.text = finalWord;
        }
    }

    public void Hide()
    {
        if (collider != null)
        {
            collider.enabled = false;
        }
        if (m_isShowing)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
        }
        m_isShowing = false;
        iTween.MoveTo(m_popOffTransform.gameObject, m_initialPopOff, 1.5f);
        Resources.UnloadUnusedAssets();
    }

    public void EnableClick()
    {
        collider.enabled = true;
    }

    void OnClick()
    {
		if(m_hideOnClick)
		{
			Hide();
		}
		else
		{
	        PipPadBehaviour.Instance.BlackBoardClicked(this);
	        if ( PictureGameCoordinator.Instance != null )
	        {
	            PictureGameCoordinator.Instance.SpeakWord(m_audioWord);
	        }
		}
    }

    public void ShakeFade()
    {
        iTween.ShakePosition(gameObject, Vector3.one * 0.01f, 0.5f);
        TweenAlpha.Begin(m_texture.gameObject, 0.5f, 0.6f);
        TweenColor.Begin(m_background.gameObject, 0.5f, Color.gray);
    }
	
	public void MoveWidgets()
	{
		if(m_movedWidgets)
		{
			return;
		}
		
		Vector3 widgetOffset = new Vector3(m_widgetOffset.x, m_widgetOffset.y, 0);
		
		Vector3 localLabelPosition = m_wordLabel.transform.localPosition - widgetOffset;
		Vector3 worldLabelPosition = m_wordLabel.transform.parent.TransformPoint(localLabelPosition);
		
		Vector3 localTexturePosition = m_texture.transform.localPosition + widgetOffset;
		Vector3 worldTexturePosition = m_texture.transform.parent.TransformPoint(localTexturePosition);
		
		iTween.MoveTo(m_wordLabel.gameObject, worldLabelPosition, 1.5f);
		iTween.MoveTo(m_texture.gameObject, worldTexturePosition, 1.5f);
		
		m_movedWidgets = true;
	}
	
	public void ResetWidgets()
	{
		m_wordLabel.transform.localPosition = m_initialWidgetPosition;
		m_texture.transform.localPosition = m_initialWidgetPosition;
		
		m_movedWidgets = false;
	}

	public string GetImageName()
	{
		return m_texture.mainTexture.name;
	}
}
