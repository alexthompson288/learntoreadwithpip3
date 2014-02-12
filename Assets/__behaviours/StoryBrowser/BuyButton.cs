using UnityEngine;
using System.Collections;

public class BuyButton : MonoBehaviour 
{
	void OnClick()
	{
		Debug.Log("BuyButton.OnClick()");
		StartCoroutine(SmallTween());

		NewStoryBrowserBookButton currentBook = InfoPanelBox.Instance.GetCurrentBook();

		if(currentBook != null)
		{
			currentBook.Purchase();
		}
		else
		{
			Debug.Log("InfoPanel has no current book");
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
