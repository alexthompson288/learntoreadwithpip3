using UnityEngine;
using System.Collections;

public class SpellingPadPhoneme : MonoBehaviour {

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
	private UISprite m_background;
	[SerializeField]
	private string[] m_backgroundNames;

	
	float m_width;
	private GameObject m_linkedObject = null;
	
	bool m_isActive = false;
	private AudioClip m_audioClip;

	private string m_phoneme;

	private Collider m_other = null;

	void Awake()
	{
		if(m_backgroundNames.Length > 0)
		{
			m_background.spriteName = m_backgroundNames[Random.Range(0, m_backgroundNames.Length)];
		}
	}

	public void SetUpPhoneme(SpellingPadBehaviour.PhonemeBuildInfo pbi)
	{
		Resources.UnloadUnusedAssets();
		m_highlight.enabled = false;
		if (pbi.m_audioFilename != null)
		{
			m_audioClip = AudioBankManager.Instance.GetAudioClip(pbi.m_audioFilename);
			m_audioSource.clip = m_audioClip;
		}
		else
		{
			m_audioClip = null;
			m_audioSource.clip = null;
		}
		m_label.text = pbi.m_displayString;
		m_label.alpha = 0;

		int width = (int)(m_label.font.CalculatePrintedSize(m_label.text, false, UIFont.SymbolStyle.None).x*1.1f);
		if(width < 80)
		{
			width = 80;
		}
		m_highlight.width = width;
		m_background.width = width;
		BoxCollider col = collider as BoxCollider;
		col.size = new Vector3(width, col.size.y, col.size.z);

		if (pbi.m_displayString.Length == 1)
		{
			m_singleButton.SetActive(true);
			//m_width = 80;
			m_width = 100;
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

		m_phoneme = pbi.m_fullPhoneme;
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
		//Debug.Log("Activate(): " + m_phoneme);
		if (!m_isActive)
		{
			//Debug.Log("Playing: " + m_phoneme);
			if (m_linkedObject != null)
			{
				m_linkedObject.GetComponent<SpellingPadPhoneme>().ActivateFinal();
			}
			ActivateFinal();
			m_audioSource.Play();
		}
	}

	public void ActivateFinal()
	{
		m_isActive = true;
		StartCoroutine(HighlightAndPlay());
	}
	
	IEnumerator HighlightAndPlay()
	{
		//Debug.Log("SpellingPadPhoneme.HighlightAndPlay(): " + m_phoneme);

		foreach (AtoBPressButton abs in m_pressButtons)
		{
			abs.ManualActivate(true);
		}
		m_highlight.enabled = true;
		
		float t = 0;
		while(t < 0.5f)
		//while (t < 1.5f)
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

		//Debug.Log("Setting m_isActive false: " + m_phoneme);
	}

	public float GetLabelTransparency()
	{
		return m_label.alpha;
	}

	public void MakeLabelInvisible()
	{
		m_label.alpha = 0;
	}

	public void MakeLabelTransparent()
	{
		m_label.alpha = 0.4f;
	}

	public void MakeLabelVisible()
	{
		m_label.alpha = 1;
	}

	public float TweenBackgroundAlpha(float newAlpha)
	{
		TweenAlpha.Begin(m_background.gameObject, 0.2f, newAlpha);
		return 0.2f;
	}

	public void PlayAudio()
	{
		if(m_audioSource.clip != null)
		{
			m_audioSource.Play();
		}
	}

	public Collider GetOther()
	{
		return m_other;
	}

	void OnTriggerEnter(Collider other)
	{
		Debug.Log("OnTriggerEnter()");
		if(m_other == null)
		{
			m_other = other;
		}
		Debug.Log("m_other: " + m_other);
	}

	void OnTriggerExit(Collider other)
	{
		Debug.Log("OnTriggerExit()");
		if(other == m_other)
		{
			m_other = null;
		}
		Debug.Log("m_other: " + m_other);
	}

	public string GetPhoneme()
	{
		return m_phoneme;
	}

	public GameObject GetLabelGo()
	{
		return m_label.gameObject;
	}

	public void EnableButtons(bool enable)
	{
		m_singleButton.SetActive(false);
		m_doubleButton.SetActive(false);
		m_tripleButton.SetActive(false);
	}
}
