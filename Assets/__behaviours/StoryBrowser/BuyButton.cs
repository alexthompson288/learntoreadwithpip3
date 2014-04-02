using UnityEngine;
using System.Collections;

// TODO: Find out whether this script has been completely deprecated
public class BuyButton : MonoBehaviour 
{
	void OnClick()
	{
		Debug.Log("BuyButton.OnClick()");
		StartCoroutine(SmallTween());

		if(InfoPanelBox.Instance.GetCurrentBook() != null)
		{
			ParentGate.Instance.OnParentGateAnswer += OnParentGateAnswer;
			ParentGate.Instance.On();
		}
		else
		{
			Debug.Log("InfoPanel has no current book");
		}
	}

	void OnParentGateAnswer(bool isCorrect)
	{
		ParentGate.Instance.OnParentGateAnswer -= OnParentGateAnswer;

		NewStoryBrowserBookButton currentBook = InfoPanelBox.Instance.GetCurrentBook();
		if(isCorrect && currentBook != null)
		{
			Debug.Log("Purchasing book");
			currentBook.Buy();
			gameObject.SetActive(false);
		}
	}

	IEnumerator SmallTween()
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("FRUITMACHINE_LEVER_DOWN");
		Vector3 originalPos = transform.localPosition;
		Vector3 newPos = originalPos;
		newPos.y -= 15;
		float tweenDuration = 0.15f;
		TweenPosition.Begin(gameObject, tweenDuration, newPos);
		yield return new WaitForSeconds(tweenDuration);
		TweenPosition.Begin(gameObject, tweenDuration, originalPos);
	}
}
