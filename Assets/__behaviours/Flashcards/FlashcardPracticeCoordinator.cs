	using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlashcardPracticeCoordinator : MonoBehaviour 
{
	[SerializeField]
	private ClickEvent m_leftArrow;
	[SerializeField]
	private ClickEvent m_rightArrow;
	[SerializeField]
	private ClickEvent m_showPictureButton;
	[SerializeField]
	private ClickEvent m_showButtonsButton;

#if UNITY_EDITOR
[SerializeField]
	private bool m_guaranteeData;
#endif

	List<DataRow> m_wordPool = new List<DataRow>();

	int m_currentIndex = 0;

	bool m_pictureActive = false;
	bool m_buttonsActive = false;
	
	IEnumerator Start () 
	{
		m_leftArrow.OnSingleClick += OnLeftArrowClick;
		m_rightArrow.OnSingleClick += OnRightArrowClick;
		m_showPictureButton.OnSingleClick += TogglePicture;
		m_showButtonsButton.OnSingleClick += ToggleButtons;

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		m_wordPool = GameDataBridge.Instance.GetWords();

#if UNITY_EDITOR
		if(m_guaranteeData)
		{
			m_wordPool = GameDataBridge.Instance.GetSectionWords(1398).Rows;
		}
#endif

		StartCoroutine(UpdatePipPad());
	}

	IEnumerator UpdatePipPad()
	{
		string word = m_wordPool[m_currentIndex]["word"].ToString();

		Texture2D tex = Resources.Load<Texture2D>("Images/word_images_png_350/_" + word);
		float tweenDuration = 0.3f;
		if(tex != null)
		{
			TweenScale.Begin(m_showPictureButton.gameObject, tweenDuration, Vector3.one);
		}
		else
		{
			TweenScale.Begin(m_showPictureButton.gameObject, tweenDuration, Vector3.zero);
		}

		yield return new WaitForSeconds(0.5f);

		Debug.Log("UpdatePipPad()");


		PipPadBehaviour.Instance.Show(word);

		PipPadBehaviour.Instance.EnableButtons(false);
		PipPadBehaviour.Instance.EnableSayWholeWordButton(false);
		m_buttonsActive = false;

		EnableClickEventColliders(true);
	}

	void OnLeftArrowClick(ClickEvent clickBehaviour)
	{
		OnArrowClick(-1);
	}

	void OnRightArrowClick(ClickEvent clickBehaviour)
	{
		OnArrowClick(1);
	}

	void OnArrowClick(int direction)
	{
		m_currentIndex += direction;

		if(m_currentIndex < 0)
		{
			m_currentIndex = m_wordPool.Count - 1;
		}
		else if(m_currentIndex >= m_wordPool.Count)
		{
			m_currentIndex = 0;
		}

		EnableClickEventColliders(false);

		PipPadBehaviour.Instance.Hide();

		PipPadBehaviour.Instance.HideAllBlackboards();
		m_pictureActive = false;

		StartCoroutine(UpdatePipPad());
	}

	void ToggleButtons(ClickEvent clickBehaviour)
	{
		m_buttonsActive = !m_buttonsActive;

		PipPadBehaviour.Instance.EnableButtons(m_buttonsActive);
		PipPadBehaviour.Instance.EnableSayWholeWordButton(m_buttonsActive);

		string audioEvent = m_buttonsActive ? "SOMETHING_APPEARS" : "SOMETHING_DISAPPEARS";
		WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
	}

	void TogglePicture(ClickEvent clickBehaviour)
	{
		Debug.Log("TogglePicture()");

		m_pictureActive = !m_pictureActive;

		if(m_pictureActive)
		{
			PipPadBehaviour.Instance.ReShowWordImage();
		}
		else
		{
			PipPadBehaviour.Instance.HideAllBlackboards();
		}
	}

	void EnableClickEventColliders(bool enable)
	{
		m_leftArrow.EnableCollider(enable);
		m_rightArrow.EnableCollider(enable);
		m_showButtonsButton.EnableCollider(enable);
		m_showPictureButton.EnableCollider(enable);
	}
}
