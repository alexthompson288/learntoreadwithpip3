using UnityEngine;
using System.Collections;

public class PipPadPhoneme : MonoBehaviour 
{
    [SerializeField]
    private GameObject m_singleButton;
    [SerializeField]
    private GameObject m_doubleButton;
    [SerializeField]
    private GameObject m_tripleButton;
    [SerializeField]
    private AtoBPressButton[] m_pressButtons;
    [SerializeField]
    private UILabel m_label;
    [SerializeField]
    private AudioSource m_audioSource;
    [SerializeField]
    private UISprite m_highlight; 
	[SerializeField]
	private BoxCollider m_letterCollider;
	[SerializeField]
	private BoxCollider m_dragCollider;

    float m_width;
    private GameObject m_linkedObject = null;

    bool m_isActive = false;
    private AudioClip m_audioClip;
    private string m_mnemonic;
    private Texture2D m_texture;
    private string m_phoneme;

	private bool m_isSecondInSplitDigraph;

	public void SetUpPhoneme(PipPadBehaviour.PhonemeBuildInfo pbi, bool noButton, bool isSecondInSplitDigraph = false)
    {
		try
		{
	        Resources.UnloadUnusedAssets();
	        m_highlight.enabled = false;
	        if (pbi.m_audioFilename != null)
	        {
				////D.Log("audioFilename: " + pbi.m_audioFilename);
	            m_audioClip = AudioBankManager.Instance.GetAudioClip(pbi.m_audioFilename);
				////D.Log("m_audioClip: " + m_audioClip);
	            m_audioSource.clip = m_audioClip;
	        }
	        else
	        {
	            m_audioClip = null;
	            m_audioSource.clip = null;
	        }
			m_isSecondInSplitDigraph = isSecondInSplitDigraph;
	        m_texture = (Texture2D)Resources.Load(pbi.m_imageFilename);
	        m_label.text = pbi.m_displayString;
	        m_phoneme = pbi.m_fullPhoneme;
	        m_highlight.width = (int)(m_label.printedSize.x*1.1f);

            m_letterCollider.size = new Vector3(m_label.printedSize.x*1.1f, m_letterCollider.size.y, m_letterCollider.size.z);
            m_dragCollider.size = new Vector3(m_label.printedSize.x*1.1f, m_dragCollider.size.y, m_dragCollider.size.z);

	        m_mnemonic = pbi.m_mnemonic;
	        if (!noButton)
	        {
	            if (pbi.m_displayString.Length == 1)
	            {
	                m_singleButton.SetActive(true);
	                m_width = 80;
	            }
	            else if (pbi.m_displayString.Length == 2)
	            {
	                m_doubleButton.SetActive(true);
	                m_width = 128;
	            }
	            else
	            {
	                m_tripleButton.SetActive(true);
	                m_width = 170;
	            }
	        }
		}
		catch
		{
			//D.Log("SetUpPhoneme.SetUpPhoneme - catch executing");
		}
    }

    void OnDestroy()
    {
        m_audioClip = null;
        m_audioSource.clip = null;
        Resources.UnloadUnusedAssets();
    }

    public void Link(GameObject other)
    {
        m_linkedObject = other;
    }

    public float GetWidth()
    {
        return m_width;
    }

    public void Activate()
    {
        if (!m_isActive)
        {
            if (m_linkedObject != null)
            {
                m_linkedObject.GetComponent<PipPadPhoneme>().ActivateFinal();
            }
            ActivateFinal();
            m_audioSource.Play();
            if (m_texture != null)
            {
                //PipPadBehaviour.Instance.ShowPhonemeImage(m_texture, m_mnemonic, m_phoneme);
            }
        }
    }

    public void ActivateFinal()
    {
        m_isActive = true;
        StartCoroutine(HighlightAndPlay());
    }

    IEnumerator HighlightAndPlay()
    {
        foreach (AtoBPressButton abs in m_pressButtons)
        {
            abs.ManualActivate(true);
        }
        m_highlight.enabled = true;

        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            m_highlight.enabled = true;
            m_isActive = true;
            foreach (AtoBPressButton abs in m_pressButtons)
            {
                abs.ManualActivate(true);
            }
            yield return null;
        }

        foreach (AtoBPressButton abs in m_pressButtons)
        {
            abs.ManualActivate(false);
        }
        m_highlight.enabled = false;
        m_isActive = false;
    }

	public bool GetIsSecondInSplitDigraph()
	{
		return m_isSecondInSplitDigraph;
	}

	/*
	public void EnableButtons(bool active)
	{
		if(m_label.text.Length == 1 || !active)
		{
			m_singleButton.SetActive(active);
		}

		if(m_label.text.Length == 2 || !active)
		{
			m_doubleButton.SetActive(active);
		}

		if(m_label.text.Length == 2 || !active)
		{
			m_tripleButton.SetActive(active);
		}

		m_letterCollider.gameObject.SetActive(active);
	}
	*/

	public void EnableButtons(bool enable)
	{
		EnableSubButtons(enable);
		EnableButtonSprites(enable);
	}

	public void EnableSubButtons(bool enable)
	{
		PipPadPhonemeSubButton[] subButtons = Object.FindObjectsOfType(typeof(PipPadPhonemeSubButton)) as PipPadPhonemeSubButton[];

		foreach(PipPadPhonemeSubButton subButton in subButtons)
		{
			subButton.collider.enabled = enable;
		}
	}

	public void EnableButtonSprites(bool enable)
	{
		UISprite sprite = m_singleButton.GetComponentInChildren<UISprite>() as UISprite;

		if(sprite == null)
		{
			sprite = m_doubleButton.GetComponentInChildren<UISprite>() as UISprite;
		}

		if(sprite == null)
		{
			sprite = m_tripleButton.GetComponentInChildren<UISprite>() as UISprite;
		}

		if(sprite != null)
		{
			StartCoroutine(EnableIndividualButtonSprite(enable, sprite));
		}
	}

	IEnumerator EnableIndividualButtonSprite(bool enable, UISprite sprite)
	{
		if(enable)
		{
			sprite.enabled = enable;
		}

		float tweenDuration = 0.1f;
		Vector3 localScale = enable ? Vector3.one : Vector3.zero;
		//iTween.ScaleTo(sprite.gameObject, localScale, tweenDuration);
		TweenScale.Begin(sprite.gameObject, tweenDuration, localScale);

		yield return new WaitForSeconds(tweenDuration + 0.1f);


		sprite.enabled = enable;
	}

	public void EnableLetterCollider(bool active)
	{
		m_letterCollider.gameObject.SetActive(active);
	}

	public void EnableDragCollider(bool active)
	{
		m_dragCollider.gameObject.SetActive(active);
	}

	public void EnableLabel(bool active)
	{
		m_label.gameObject.SetActive(active);
	}

	public Vector3 GetButtonPos()
	{
		return m_singleButton.transform.position;
	}

	public string GetText()
	{
		return m_label.text;
	}

	public float GetClipLength()
	{
		if(m_audioClip != null)
		{
			return m_audioClip.length;
		}
		else
		{
			return 0;
		}
	}
}
