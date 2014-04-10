using UnityEngine;
using System.Collections;
using Wingrove;

public class TweenReceiver : MonoBehaviour 
{
	public delegate void TweenFinish(TweenReceiver tweenBehaviour);
	public event TweenFinish OnTweenFinish;

	[SerializeField]
	private Transform m_tweenToPosition;
	[SerializeField]
	private Camera m_myCamera;
	[SerializeField]
	private Camera m_otherCamera;
	[SerializeField]
	private GameObject m_tweenPrefab;
	[SerializeField]
	private float m_scaleDuration;
	[SerializeField]
	private float m_postScaleDelay;
	[SerializeField]
	private float m_moveDuration;
	[SerializeField]
	private float m_postMoveDelay;
	[SerializeField]
	private string m_appearSound = "SOMETHING_APPEARS";
	[SerializeField]
	private string m_disappearSound = "SOMETHING_DISAPPEARS";


	public void TweenObject(Transform fromPos)
	{
		GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_tweenPrefab, fromPos);

		iTween.ScaleFrom(newGo, Vector3.zero, m_scaleDuration);

		WingroveAudio.WingroveRoot.Instance.PostEvent(m_appearSound);

		StartCoroutine(TweenObjectCo(newGo));
	}
	
	IEnumerator TweenObjectCo(GameObject newGo)
	{
		yield return new WaitForSeconds(m_scaleDuration + m_postScaleDelay);

		Vector3 globalTweenPos = m_tweenToPosition.position;
		
		if(m_myCamera != null && m_otherCamera != null)
		{
			Vector3 camTweenPos = m_myCamera.WorldToScreenPoint(m_tweenToPosition.position);
			globalTweenPos = m_otherCamera.ScreenToWorldPoint(camTweenPos);
		}

		iTween.MoveTo(newGo, globalTweenPos, m_moveDuration);

		yield return new WaitForSeconds(m_moveDuration + m_postMoveDelay);

		iTween.ScaleTo(newGo, Vector3.zero, m_scaleDuration);

		WingroveAudio.WingroveRoot.Instance.PostEvent(m_disappearSound);

		yield return new WaitForSeconds(m_scaleDuration);

		Destroy(newGo);

		if(OnTweenFinish != null)
		{
			OnTweenFinish(this);
		}
	}
}
