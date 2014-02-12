using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class GoldCoinBar : Singleton<GoldCoinBar> {

    [SerializeField]
    private GameObject m_coinPrefab;
	[SerializeField]
	private Transform[] m_coinPositions;

    List<GameObject> m_spawnedCoins = new List<GameObject>();
	int m_numCoins;
	
	Vector3 m_letterPosition;
	
	int CoinPositionComparison(Transform a, Transform b)
	{
		if(a.name[0] < b.name[0])
		{
			return -1;
		}
		else if(a.name[0] > b.name[0])
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}
	
	IEnumerator Start ()
	{
		Array.Sort(m_coinPositions, CoinPositionComparison);
		
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		int numCoins = SessionInformation.Instance.GetCoins();
		
		yield return new WaitForSeconds(0.5f);
		
		if(numCoins == 0)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("PLAY_GAMES_WIN_COINS_INSTRUCTION");
		}
		else
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("UNLOCK_GREEN_LETTERS_INSTRUCTION");
		}
		
		for (int index = 0; index < numCoins && index < m_coinPositions.Length; ++index)
        {
            SpawnCoin(index);
        }
	}
	
	void SpawnCoin(int index)
	{
		GameObject newCoin = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_coinPrefab, m_coinPositions[index]);
        m_spawnedCoins.Add(newCoin);
	}
	
	public IEnumerator SpendCoin()
	{
		yield return new WaitForSeconds(1.5f);
		
		SessionInformation.Instance.SetCoins(SessionInformation.Instance.GetCoins() - 1);
		
		int index = m_spawnedCoins.Count - 1;

		StartCoroutine(m_spawnedCoins[index].GetComponent<GoldCoin>().TweenToPosition(m_letterPosition));
		m_spawnedCoins.RemoveAt(index);
		
		if(SessionInformation.Instance.GetCoins() > m_spawnedCoins.Count)
		{
			SpawnCoin(index);
		}
	}
	
	public void SetCurrentLetterPosition(Vector3 newPosition)
	{
		m_letterPosition = newPosition;
	}
}
