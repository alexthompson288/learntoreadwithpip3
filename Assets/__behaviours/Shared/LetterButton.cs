using UnityEngine;
using System.Collections;

public class LetterButton : MonoBehaviour 
{
	static LetterButton m_largeButton = null;
	static BoxCollider m_blocker;

	public delegate void Single(LetterButton behaviour);
	public event Single OnSingle;

	public delegate void Double(LetterButton behaviour);
	public event Double OnDouble;

	[SerializeField]
	private UILabel m_graphemeLabel;
	[SerializeField]
	private UILabel m_mnemonicLabel;
	[SerializeField]
	private UITexture m_mnemonicTexture;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private string[] m_backgroundNames;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private float m_betweenAudioDelay = 0.3f;
	[SerializeField]
	private string m_mainColorString = "[333333]";
	[SerializeField]
	private string m_highlightColorString = "[FF0000]";
	//[SerializeField]
	//private BoxCollider m_blocker;
	[SerializeField]
	private float m_tweenDuration = 0.5f;

	bool m_useBlocker = true;
	bool m_tweenPosition = true;

	DataRow m_data;

	[SerializeField] // TODO: Delete SerializeField, only needed for testing
	AudioClip m_phonemeAudio;
	[SerializeField] // TODO: Delete SerializeField, only needed for testing
	AudioClip m_mnemonicAudio;

	public delegate void MyMethod();

	MyMethod[] m_singleMethods = null;
	MyMethod[] m_doubleMethods = null;

	Vector3 m_defaultLocalPos;

	public void SetBackgroundSpriteName(string spriteName)
	{
		m_background.spriteName = spriteName;
	}

	public void RandomBackgroundSpriteName()
	{
		m_background.spriteName = m_backgroundNames[Random.Range(0, m_backgroundNames.Length)];
	}

	public void SetUseBlocker(bool useBlocker)
	{
		m_useBlocker = useBlocker;
	}

	public void SetTweenPosition(bool tweenPosition)
	{
		m_tweenPosition = tweenPosition;
	}

	void OnDrag(Vector2 delta)
	{
		if(m_largeButton != null)
		{
			m_largeButton.TweenDefault();
		}
	}

	public void SetUp(DataRow data, bool showMnemonicLabel = true, bool loadAudio = true)
	{
		m_data = data;

		string spriteName = AlphabetBookInformation.Instance.GetTexture(System.Convert.ToInt32(m_data["id"]));
		if(spriteName != null)
		{
			m_background.spriteName = spriteName;
		}

		string graphemeText = m_data["phoneme"].ToString();
		m_graphemeLabel.text = graphemeText;

		string mnemonicText = m_data["mneumonic"].ToString();

		string coloredMnemonicText = m_mainColorString + mnemonicText.Replace(graphemeText, m_highlightColorString + graphemeText + m_mainColorString);
		// do some mad logic to replace splits
		if (graphemeText.Contains("-") && graphemeText.Length == 3)
		{
			int startIndex = 0;
			while (coloredMnemonicText.IndexOf(graphemeText[0], startIndex) != -1)
			{
				startIndex = coloredMnemonicText.IndexOf(graphemeText[0], startIndex);
				if (startIndex < coloredMnemonicText.Length + 2)
				{
					if (coloredMnemonicText[startIndex + 2] == graphemeText[2])
					{
						coloredMnemonicText = coloredMnemonicText.Insert(startIndex + 3, m_mainColorString);
						coloredMnemonicText = coloredMnemonicText.Insert(startIndex + 2, m_highlightColorString);
						coloredMnemonicText = coloredMnemonicText.Insert(startIndex + 1, m_mainColorString);
						coloredMnemonicText = coloredMnemonicText.Insert(startIndex + 0, m_highlightColorString);
						startIndex += m_mainColorString.Length * 2 + m_highlightColorString.Length * 2;
					}
				}
				
				startIndex++;
			}
		}

		m_mnemonicLabel.text = coloredMnemonicText;

		if(mnemonicText.Length > 23)
		{
			m_mnemonicLabel.transform.parent.localScale = Vector3.one * 0.35f;
		}
		else if(mnemonicText.Length > 18)
		{
			m_mnemonicLabel.transform.parent.localScale = Vector3.one * 0.4f;
		}
		else if(mnemonicText.Length > 15)
		{
			m_mnemonicLabel.transform.parent.localScale = Vector3.one * 0.45f;
		}
		else if(mnemonicText.Length > 12)
		{
			m_mnemonicLabel.transform.parent.localScale = Vector3.one * 0.5f;
		}

		m_mnemonicLabel.enabled = showMnemonicLabel;

		if(!showMnemonicLabel)
		{
			Debug.Log("Moving graphemeLabel and mnemonicTexture to posY = 0");
			TransformHelpers.SetLocalPosY(m_graphemeLabel.transform.parent, 0);
			TransformHelpers.SetLocalPosY(m_mnemonicTexture.transform.parent, 0);
		}

		string imageFilename =
			string.Format("Images/mnemonics_images_png_250/{0}_{1}",
			              graphemeText,
			              mnemonicText.Replace(" ", "_"));
		
		Texture2D tex = (Texture2D)Resources.Load(imageFilename);
		if(tex != null)
		{
			m_mnemonicTexture.mainTexture = tex;
		}
		else
		{
			m_mnemonicTexture.enabled = false;
		}

		if(loadAudio)
		{
			m_phonemeAudio = AudioBankManager.Instance.GetAudioClip(m_data["grapheme"].ToString());
			m_mnemonicAudio = LoaderHelpers.LoadMnemonic(m_data);
		}
	}

	void OnBlockerClick(ClickEvent blockerBehaviour)
	{
		TweenDefault();
	}

	void OnClick()
	{
		if(m_largeButton == null)
		{
			StartCoroutine(OnClickCo());
		}
		else
		{
			m_largeButton.TweenDefault();
		}
	}

	IEnumerator OnClickCo()
	{
		yield return new WaitForSeconds(0.3f);
		if(OnSingle != null)
		{
			OnSingle(this);
		}

		if(m_singleMethods != null)
		{
			for(int i = 0; i < m_singleMethods.Length; ++i)
			{
				m_singleMethods[i]();
			}
		}
	}

	void OnDoubleClick()
	{
		if(m_largeButton == null)
		{
			if(OnDouble != null || m_doubleMethods != null)
			{
				StopAllCoroutines();
			}
			
			if(OnDouble != null)
			{
				OnDouble(this);
			}
			
			if(m_doubleMethods != null)
			{
				for(int i = 0; i < m_doubleMethods.Length; ++i)
				{
					m_doubleMethods[i]();
				}
			}
		}
		else
		{
			m_largeButton.TweenDefault();
		}
	}

	public void TweenLarge()
	{
		if(TransformHelpers.ApproxLocalScale2D(transform, Vector3.one))
		{
			m_largeButton = this;

			//m_blocker.transform.position = Vector3.zero;

			m_background.depth += 100;
			m_graphemeLabel.depth += 100;
			m_mnemonicLabel.depth += 100;
			m_mnemonicTexture.depth += 100;

			m_defaultLocalPos = transform.localPosition;

			TweenScale.Begin(gameObject, m_tweenDuration, Vector3.one * 2);

			if(m_tweenPosition)
			{
				iTween.MoveTo(gameObject, Vector3.zero, m_tweenDuration);
			}

			//m_blocker.enabled = m_useBlocker;
		}
	}

	public void TweenDefault()
	{
		if(m_background.depth >= 100)
		{
			m_background.depth -= 100;
			m_graphemeLabel.depth -= 100;
			m_mnemonicLabel.depth -= 100;
			m_mnemonicTexture.depth -= 100;
		}

		TweenScale.Begin(gameObject, m_tweenDuration, Vector3.one);
		TweenPosition.Begin(gameObject, m_tweenDuration, m_defaultLocalPos);

		//m_blocker.enabled = false;

		if(m_largeButton == this)
		{
			m_largeButton = null;
		}
	}

	public void PlayPhonemeAudio()
	{
		if(m_phonemeAudio != null)
		{
			m_audioSource.clip = m_phonemeAudio;
			m_audioSource.Play();
		}
	}

	public void PlayMnemonicAudio()
	{
		if(m_mnemonicAudio != null)
		{
			m_audioSource.clip = m_mnemonicAudio;
			m_audioSource.Play();
		}
	}

	public void PlayAllAudio()
	{
		StartCoroutine(PlayAllAudioCo());
	}

	IEnumerator PlayAllAudioCo()
	{
		PlayPhonemeAudio();

		float delay = 0;

		if(m_phonemeAudio != null)
		{
			delay += m_phonemeAudio.length;
			delay += 0.2f;
		}

		yield return new WaitForSeconds(delay);

		PlayMnemonicAudio();
	}

	public float GetPhonemeAudioLength()
	{
		float length = 0;

		if(m_phonemeAudio != null)
		{
			length += m_phonemeAudio.length;
		}

		return length;
	}

	public float GetMnemonicAudioLength()
	{
		float length = 0;
		
		if(m_mnemonicAudio != null)
		{
			length += m_mnemonicAudio.length;
		}
		
		return length;
	}

	public float GetAllAudioLength()
	{
		float length = 0;
		
		if(m_phonemeAudio != null)
		{
			length += m_phonemeAudio.length;
			length += m_betweenAudioDelay;
		}

		if(m_mnemonicAudio != null)
		{
			length += m_mnemonicAudio.length;
		}
		
		return length;
	}

	public void SetMethods(MyMethod[] singleMethods, MyMethod[] doubleMethods = null)
	{
		m_singleMethods = singleMethods;
		m_doubleMethods = doubleMethods;
	}
	
	public void SetMethods(MyMethod singleMethod, MyMethod doubleMethod = null)
	{
		m_singleMethods = new MyMethod[1];
		m_singleMethods[0] = singleMethod;
		
		m_doubleMethods = new MyMethod[1];
		m_doubleMethods[0] = doubleMethod;
	}

	public void SetMethods(MyMethod singleMethod, MyMethod[] doubleMethods = null)
	{
		m_singleMethods = new MyMethod[1];
		m_singleMethods[0] = singleMethod;

		m_doubleMethods = doubleMethods;
	}

	public void SetMethods(MyMethod[] singleMethods, MyMethod doubleMethod)
	{
		m_singleMethods = singleMethods;

		m_doubleMethods = new MyMethod[1];
		m_doubleMethods[0] = doubleMethod;
	}

	public void SetDoubleMethod(MyMethod doubleMethod)
	{
		m_doubleMethods = new MyMethod[1];
		m_doubleMethods[0] = doubleMethod;
	}

	public DataRow GetData()
	{
		return m_data;
	}
}
