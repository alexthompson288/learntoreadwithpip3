using UnityEngine;
using System.Collections;
using Wingrove;

public class TimbleTombleSign : Singleton<TimbleTombleSign> 
{
	[SerializeField]
	private GameObject m_coinPrefab;
	[SerializeField]
	private Transform m_coinTweenPos;
	[SerializeField]
	private Camera m_myCamera;
	[SerializeField]
	private Camera m_otherCamera;
	
	public void TweenCoin(Transform fromPos)
	{
		GameObject newCoin = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_coinPrefab, fromPos);
	
		StartCoroutine(TweenCoinCo(newCoin.GetComponent<GoldCoin>() as GoldCoin));
	}

	IEnumerator TweenCoinCo(GoldCoin coin)
	{
		yield return new WaitForSeconds(coin.GetScaleTweenDuration() + 0.2f);

		//coin.transform.parent = transform;

		if(m_myCamera != null && m_otherCamera != null)
		{
			Vector3 camCoinTweenPos = m_myCamera.WorldToScreenPoint(m_coinTweenPos.transform.position);
			Vector3 globalCoinTweenPos = m_otherCamera.ScreenToWorldPoint(camCoinTweenPos);

			StartCoroutine(coin.TweenToPosition(globalCoinTweenPos));
		}
		else
		{
			StartCoroutine(coin.TweenToPosition(m_coinTweenPos.position));
		}

		yield return new WaitForSeconds(coin.GetTotalTweenDuration());

		JourneyCoordinator.Instance.SetActionComplete("CoinTween");
	}
}
 