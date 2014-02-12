using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Wingrove;

public class TTGoldCoinBar : Singleton<TTGoldCoinBar> {

	[SerializeField]
	private GameObject m_coinPrefab;
	[SerializeField]
	private Transform[] m_coinPositions;
	
	List<GameObject> m_spawnedCoins = new List<GameObject>();
	int m_numCoins;
	
	Vector3 m_itemPosition;
	
	int CoinPositionComparison(Transform a, Transform b)
	{
		if(a.position.x < b.position.x)
		{
			return -1;
		}
		else if(a.position.x > b.position.x)
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
		
		int numCoins = TTInformation.Instance.GetGoldCoins();
		
		yield return new WaitForSeconds(0.5f);
		
		if(numCoins == 0)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("PLAY_GAMES_WIN_COINS_INSTRUCTION");
			//WingroveAudio.WingroveRoot.Instance.PostEvent("TT_PLAY_GAMES_TO_BUY_FOOD");
		}
		else if(TTInformation.Instance.GetMagic() < 100)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("TT_BUY_FOOD_TO_FEED_TROLL");
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

	public void SpendCoin(Vector3 position, int amount)
	{
		StartCoroutine(SpendCoinCo(position, amount));
	}

	IEnumerator SpendCoinCo(Vector3 position, int amount)
	{
		TTInformation.Instance.SetGoldCoins(TTInformation.Instance.GetGoldCoins() - amount);

		for(int i = 0; i < amount; ++i)
		{
			int index = m_spawnedCoins.Count - 1;
			StartCoroutine(m_spawnedCoins[index].GetComponent<GoldCoin>().TweenToPosition(position));
			m_spawnedCoins.RemoveAt(index);
		}

		yield return new WaitForSeconds(0.5f);

		Debug.Log("GoldCoins = " + TTInformation.Instance.GetGoldCoins());

		while(TTInformation.Instance.GetGoldCoins() > m_spawnedCoins.Count && m_spawnedCoins.Count < m_coinPositions.Length)
		{
			SpawnCoin(m_spawnedCoins.Count);
		}
	}
	
	public IEnumerator SpendCoin()
	{
		yield return new WaitForSeconds(1.5f);
		
		TTInformation.Instance.SetGoldCoins(TTInformation.Instance.GetGoldCoins() - 1);
		
		int index = m_spawnedCoins.Count - 1;
		
		StartCoroutine(m_spawnedCoins[index].GetComponent<GoldCoin>().TweenToPosition(m_itemPosition));
		m_spawnedCoins.RemoveAt(index);
		
		if(TTInformation.Instance.GetGoldCoins() > m_spawnedCoins.Count)
		{
			SpawnCoin(index);
		}
	}
	
	public void SetItemPosition(Vector3 newPosition)
	{
		m_itemPosition = newPosition;
	}

#if UNITY_EDITOR
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.K))
		{
			DestroyCoins();

			int numCoins = 10;

			TTInformation.Instance.SetGoldCoins(numCoins);
			
			for (int index = 0; index < numCoins && index < m_coinPositions.Length; ++index)
			{
				SpawnCoin(index);
			}
		}
	}

	void DestroyCoins()
	{
		for(int i = 0; i < m_spawnedCoins.Count; ++i)
		{
			Destroy(m_spawnedCoins[i]);
		}
		m_spawnedCoins.Clear();
	}
#endif
}
