using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class SplatRatGamePlayer : GamePlayer 
{
	[SerializeField]
    private GameObject m_splatPrefab;
	[SerializeField]
    private Blackboard m_blackboard;
	[SerializeField]
    private ProgressScoreBar m_scoreBar;
    [SerializeField]
    private Benny m_benny;
	
	[SerializeField]
	private GameObject[] m_locators;
	
	[SerializeField]
	private Blackboard m_largeBlackboard;

	[SerializeField]
	private UITexture m_frontTexture;
	[SerializeField]
	private UITexture m_rearTexture;

    public override void SelectCharacter(int characterIndex)
    {
        ////D.Log("SelectCharacter");
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;
        ////D.Log("m_selectedCharacter: " + m_selectedCharacter);
        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        SplatRatGameCoordinator.Instance.CharacterSelected(characterIndex);
    }

	public void SetTextures(Texture2D frontTex, Texture2D rearTex)
	{
		m_frontTexture.mainTexture = frontTex;
		m_rearTexture.mainTexture = rearTex;
	}
	
	List<SplattableRatLetter> m_spawnedSplattables = new List<SplattableRatLetter>();
	
	int m_score;
	
	void Start()
	{
		m_blackboard.MoveWidgets();
	}
	
    public IEnumerator SetUpBenny(DataRow currentData, bool play)
    {
        if (play)
        {
            m_benny.SetFirst(DataHelpers.GetLongAudio(currentData));
            yield return StartCoroutine(m_benny.PlayAudio());
        }

        m_benny.SetFirst(DataHelpers.GetShortAudio(currentData));

        yield break;
    }

	public IEnumerator DisplayLargeBlackboard(Texture2D texture, string letter, string colorReplace)
	{
		////D.Log("DisplayInitialBlackBoard()");

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
	
	public bool LetterClicked(string letter)
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
