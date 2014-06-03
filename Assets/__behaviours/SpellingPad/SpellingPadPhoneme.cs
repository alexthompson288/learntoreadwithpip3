using UnityEngine;
using System.Collections;

public class SpellingPadPhoneme : MonoBehaviour 
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
	
	float m_width;
	private GameObject m_linkedObject = null;
	
	bool m_isActive = false;
	private AudioClip m_audioClip;

	private string m_phoneme;

	private Collider m_other = null;

    private int m_positionIndex;
    public int positionIndex
    {
        get
        {
            return m_positionIndex;
        }
    }

    public enum State
    {
        Unanswered,
        Hint,
        Answered
    }

    private State m_state;
    public State state
    {
        get
        {
            return m_state;
        }
    }

	public void SetUp(SpellingPadBehaviour.PhonemeBuildInfo pbi)
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
		BoxCollider col = collider as BoxCollider;
		col.size = new Vector3(width, col.size.y, col.size.z);

		if (pbi.m_displayString.Length == 1)
		{
			m_singleButton.SetActive(true);
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
        m_positionIndex = pbi.m_positionIndex;

        //Debug.Log(System.String.Format("Phoneme: {0} - {1}", m_phoneme, m_positionIndex));
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
		foreach (AtoBPressButton abs in m_pressButtons)
		{
			abs.ManualActivate(true);
		}
		m_highlight.enabled = true;
		
		float t = 0;
		while(t < 0.5f)
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

	public void ChangeState(State newState, bool useLock = false)
    {
        // if useLock is true, m_state will not change to a lower state 
        if (!useLock || newState > m_state)
        {
            m_state = newState;

            float alphaTweenDuration = 0.25f;

            switch(m_state)
            {
                case State.Unanswered:
                    TweenAlpha.Begin(m_label.gameObject, alphaTweenDuration, 0);
                    break;
                case State.Hint:
                    TweenAlpha.Begin(m_label.gameObject, alphaTweenDuration, 0.5f);
                    break;
                case State.Answered:
                    TweenAlpha.Begin(m_label.gameObject, alphaTweenDuration, 1);
                    break;
            }
        }
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

    public void EnableTrigger(bool enable)
    {
        collider.enabled = false;
    }

	void OnTriggerEnter(Collider other)
	{
		if(m_other == null)
		{
			m_other = other;
		}
		Debug.Log("Enter - m_other: " + m_other);
	}

	void OnTriggerExit(Collider other)
	{
		if(other == m_other)
		{
			m_other = null;
		}
		Debug.Log("Enter - m_other: " + m_other);
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
