using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SplatRatGamePlayer : GamePlayer 
{
	[SerializeField]
	private int m_playerIndex;
	
	[SerializeField]
    private GameObject m_splatPrefab;
	[SerializeField]
    private Blackboard m_blackboard;
	[SerializeField]
    private ProgressScoreBar m_scoreBar;
	
	[SerializeField]
	private GameObject[] m_locators;
	
	[SerializeField]
	private Blackboard m_largeBlackboard;

	
	List<CharacterSelection> m_characterSelections = new List<CharacterSelection>();
	int m_selectedCharacter = -1;
	
	List<SplattableRatLetter> m_spawnedSplattables = new List<SplattableRatLetter>();
	
	int m_score;
	
	void Start()
	{
		m_blackboard.MoveWidgets();
	}
	
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
        SplatRatGameCoordinator.Instance.CharacterSelected(characterIndex);
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
	
	public IEnumerator DisplayLargeBlackboard(Texture2D texture, string letter, string colorReplace)
	{
		Debug.Log("DisplayInitialBlackBoard()");

        m_largeBlackboard.ShowImage(texture, letter, colorReplace);
		
		yield return new WaitForSeconds(1.0f);
		
		m_largeBlackboard.MoveWidgets();
	}
	
	public void HideLargeBlackboard()
	{
		m_largeBlackboard.Hide();
	}
	
	public void SetScoreBar(int targetScore)
	{
		m_scoreBar.SetStarsTarget(targetScore);
	}
	
	public int GetScore()
	{
		return m_score;
	}
	
	public void SpawnSplattables(List<DataRow> lettersPool)
	{
		foreach(GameObject locator in m_locators)
		{
			GameObject newSplat = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_splatPrefab, locator.transform);
			SplattableRatLetter splattableRatLetter = newSplat.GetComponent<SplattableRatLetter>() as SplattableRatLetter;
			splattableRatLetter.SetUp(this, locator, lettersPool);
	        m_spawnedSplattables.Add(splattableRatLetter);
		}
	}
	
	public bool LetterClicked(string letter, GameObject locator)
    {
		if(letter == SplatRatGameCoordinator.Instance.GetCurrentLetter())
		{
			// Start coroutine from player, not splattable, because splattables are destroyed at end of game
			StartCoroutine(SplatRatGameCoordinator.Instance.PlayLetterSound(false)); 
			
            m_blackboard.Hide();

			++m_score;
			m_scoreBar.SetStarsCompleted(m_score);
	        m_scoreBar.SetScore(m_score);
			
			return true;
		}
		else
		{
			SplatRatGameCoordinator.Instance.GiveHint(m_blackboard);
			
			return false;
		}
    }
	
	public void StopGame()
    {
        StopAllCoroutines();
        m_blackboard.Hide();
		DestroySplattables();
    }
	
	void DestroySplattables()
	{
		m_blackboard.Hide();
		
		foreach (SplattableRatLetter splattable in m_spawnedSplattables)
        {
			StartCoroutine(splattable.DestroySplattable());
        }
        
		m_spawnedSplattables.Clear();
	}
}
