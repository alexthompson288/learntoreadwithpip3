using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class BingoKeywordsPlayer : GamePlayer {
	[SerializeField]
	private int m_playerIndex;
	[SerializeField]
	private GameObject m_bingoSlotPrefab;
	[SerializeField]
	private Transform[] m_locators;
	[SerializeField]
	private BingoButton m_bingoButton;

	int m_score;

	List<BingoSlot> m_spawnedSlots = new List<BingoSlot>();

	List<CharacterSelection> m_characterSelections = new List<CharacterSelection>();
	int m_selectedCharacter = -1;
	
	public override void RegisterCharacterSelection(CharacterSelection characterSelection)
	{
		m_characterSelections.Add(characterSelection);
	}
	
	public override void SelectCharacter(int characterIndex)
	{
		Debug.Log("SelectCharacter");
		SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
		m_selectedCharacter = characterIndex;
		Debug.Log("m_selectedCharacter: " + m_selectedCharacter);
		foreach (CharacterSelection cs in m_characterSelections)
		{
			cs.DeactivatePress(false);
		}
		BingoKeywordsCoordinator.Instance.CharacterSelected(characterIndex);
	}
	
	public void HideCharacter(int index)
	{
		foreach (CharacterSelection cs in m_characterSelections)
		{
			if (cs.GetCharacterIndex() == index)
			{
				cs.DeactivatePress(false);
			}
		}
	}
	
	public void HideAll()
	{
		foreach (CharacterSelection cs in m_characterSelections)
		{
			cs.DeactivatePress(true);
		}
	}

	public bool HasSelectedCharacter()
	{
		return (m_selectedCharacter != -1);
	}
	
	public void SpawnSlots(int numToSpawn, List<DataRow> wordsPool)
	{
		Debug.Log("wordsPool.Count: " + wordsPool.Count);

		HashSet<string> words = new HashSet<string>();

		while(words.Count < numToSpawn)
		{
			words.Add(wordsPool[Random.Range(0, wordsPool.Count)]["word"].ToString());
		}

		int i = 0;
		foreach(string word in words)
		{
			GameObject newGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_bingoSlotPrefab, m_locators[i]);

			BingoSlot newBingoSlot = newGo.GetComponent<BingoSlot>() as BingoSlot;
			newBingoSlot.SetUp(word);
			newBingoSlot.OnBingoClick += OnBingoClick;
			m_spawnedSlots.Add(newBingoSlot);

			++i;
			if(i >= numToSpawn)
			{
				break;
			}
		}

		m_bingoButton.On();
	}

	void OnBingoClick(BingoSlot clicked)
	{
		Debug.Log("OnBingoClick");
		if(!clicked.GetIsOn() && clicked.GetWord() == BingoKeywordsCoordinator.Instance.GetCurrentWord())
		{
			clicked.ChangeToOnSprite();
			++m_score;
		}
		else
		{
			Debug.Log("isOn: " + clicked.GetIsOn());
			Debug.Log("clicked.Word: " + clicked.GetWord());
			Debug.Log("currentWord: " + BingoKeywordsCoordinator.Instance.GetCurrentWord());
		}
	}

	public void CheckWin()
	{
		if(m_score == m_spawnedSlots.Count)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
			BingoKeywordsCoordinator.Instance.SetWinningPlayerIndex(m_playerIndex);
		}
	}
}
