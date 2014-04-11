using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LearnPhonemeCoordinator : Singleton<LearnPhonemeCoordinator> 
{
	[SerializeField]
	private LearnPhonemeLetter m_learnPhonemeLetter;
	[SerializeField]
	private GameObject m_wordPrefab;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private UILabel m_graphemeLabel;
	[SerializeField]
	private UITexture m_mnemonic;
	[SerializeField]
	private LetterButton m_letterButton;


	List<LearnPhonemeWord>  m_words = new List<LearnPhonemeWord>();

	// Use this for initialization
	IEnumerator Start () 
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		int sectionId = SessionManager.Instance.GetCurrentSectionId();
		//int sectionId = 1446;

		Debug.Log("sectionId: " + sectionId);

		DataTable dtp = GameDataBridge.Instance.GetDatabase().
			ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);

		AudioClip phonemeAudio = null;

		if(dtp.Rows.Count > 0)
		{
			DataRow phonemeData = dtp.Rows[0];

			m_graphemeLabel.text = phonemeData["phoneme"].ToString();

			string imageFilename =
				string.Format("Images/mnemonics_images_png_250/{0}_{1}",
				              phonemeData["phoneme"],
				              phonemeData["mneumonic"].ToString().Replace(" ", "_"));

			Texture2D mnemonicTexture = (Texture2D)Resources.Load(imageFilename);
			if(mnemonicTexture != null)
			{
				m_mnemonic.mainTexture = mnemonicTexture;
			}
			
			phonemeAudio = AudioBankManager.Instance.GetAudioClip(phonemeData["grapheme"].ToString());
			AudioClip mnemonicAudio = LoaderHelpers.LoadMnemonic(phonemeData);

			m_learnPhonemeLetter.SetUp(phonemeAudio, mnemonicAudio);

			m_letterButton.SetUp(dtp.Rows[0], false);
			m_letterButton.SetUseBlocker(false);
			m_letterButton.SetTweenPosition(false);
			//m_letterButton.SetMethods( m_letterButton.PlayPhonemeAudio , new LetterButton.MyMethod[] { m_letterButton.PlayMnemonicAudio, m_letterButton.TweenLarge } );
			m_letterButton.SetMethods( m_letterButton.PlayPhonemeAudio , m_letterButton.PlayMnemonicAudio );
		}

		DataTable dtw = DataHelpers.GetSectionWords(sectionId);
		List<DataRow> wordData = new List<DataRow>();

		if(dtw.Rows.Count > 0)
		{
			wordData.AddRange(dtw.Rows);
			for(int i = 0; i < wordData.Count && i < m_locators.Length; ++i)
			{
				Debug.Log(wordData[i]["word"].ToString());
				GameObject newWord = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordPrefab, m_locators[i]);
				m_words.Add(newWord.GetComponent<LearnPhonemeWord>());
				newWord.GetComponent<LearnPhonemeWord>().SetUp(wordData[i], phonemeAudio);
			}
		}
		else
		{
			GameManager.Instance.CompleteGame();
		}
	}

	public void CheckForEnd()
	{
		bool finished = true;
		foreach(LearnPhonemeWord word in m_words)
		{
			if(!word.GetPressed())
			{
				finished = false;
				break;
			}
		}

		if(finished)
		{
			StartCoroutine(EndGame());
		}
	}

	IEnumerator EndGame()
	{
		m_letterButton.TweenLarge();

		m_letterButton.PlayAllAudio();

		yield return new WaitForSeconds(m_letterButton.GetAllAudioLength() + 0.8f);

		yield return StartCoroutine(CelebrationCoordinator.Instance.Trumpet());
		GameManager.Instance.CompleteGame();
	}
}
