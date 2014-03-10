using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SplatRatKeyPlayer : GamePlayer 
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
	
	List<SplattableRatKeyword> m_spawnedSplattables = new List<SplattableRatKeyword>();
	
	int m_score;

	[SerializeField]
	private UITexture m_frontTexture;
	[SerializeField]
	private UITexture m_rearTexture;
	
	public void SetTextures(Texture2D frontTex, Texture2D rearTex)
	{
		m_frontTexture.mainTexture = frontTex;
		m_rearTexture.mainTexture = rearTex;
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
        SplatRatKeyCoordinator.Instance.CharacterSelected(characterIndex);
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
	
	public IEnumerator DisplayLargeBlackboard(string word)
	{
		Debug.Log("DisplayInitialBlackBoard()");

		m_largeBlackboard.ShowImage(null, word, null, null);

		yield break;
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
			SplattableRatKeyword splattableRatLetter = newSplat.GetComponent<SplattableRatKeyword>() as SplattableRatKeyword;
			splattableRatLetter.SetUp(this, locator, lettersPool);
	        m_spawnedSplattables.Add(splattableRatLetter);
		}
	}
	
	public bool LetterClicked(string letter, GameObject locator)
    {
		if(letter == SplatRatKeyCoordinator.Instance.GetCurrentLetter())
		{
			// Start coroutine from player, not splattable, because splattables are destroyed at end of game
			StartCoroutine(SplatRatKeyCoordinator.Instance.PlayLetterSound()); 
			
            m_blackboard.Hide();

			++m_score;
			m_scoreBar.SetStarsCompleted(m_score);
	        m_scoreBar.SetScore(m_score);
			
			return true;
		}
		else
		{
			SplatRatKeyCoordinator.Instance.GiveHint(m_blackboard);
			
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
		
		foreach (SplattableRatKeyword splattable in m_spawnedSplattables)
        {
			StartCoroutine(splattable.DestroySplattable());
        }
        
		m_spawnedSplattables.Clear();
	}
}
