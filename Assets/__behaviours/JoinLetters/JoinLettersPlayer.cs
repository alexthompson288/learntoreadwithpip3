using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class JoinLettersPlayer : GamePlayer
{
	[SerializeField]
	private int m_playerIndex;
    [SerializeField]
    private GameObject m_pictureBoxPrefab;
    [SerializeField]
    private GameObject m_wordBoxPrefab;
    [SerializeField]
    private Transform m_lowCorner;
    [SerializeField]
    private Transform m_highCorner;
    [SerializeField]
    private ProgressScoreBar m_progressScoreBar;
    [SerializeField]
    private Transform m_spawnTransform;
    [SerializeField]
    private Transform m_leftOff;
    [SerializeField]
    private Transform m_rightOff;
    [SerializeField]
    private CharacterPopper m_characterPopper;
	[SerializeField]
    private Blackboard m_blackboard;
	List<CharacterSelection> m_characterSelections = new List<CharacterSelection>();
	[SerializeField] private int m_selectedCharacter = -1;
	[SerializeField]
	private BennyAudio m_bennyTheBook;

    List<DataRow> m_lettersPool = new List<DataRow>();
	Dictionary<DataRow, Texture2D> m_phonemeImages = new Dictionary<DataRow, Texture2D>();

    List<GameObject> m_spawnedObjects = new List<GameObject>();

    int m_score;
	
	int m_panelDepthIncrement = 1;
	
	public override void RegisterCharacterSelection(CharacterSelection characterSelection)
    {
        m_characterSelections.Add(characterSelection);
    }

    public override void SelectCharacter(int characterIndex)
    {
        SessionInformation.Instance.SetPlayerIndex(m_playerIndex, characterIndex);
        m_selectedCharacter = characterIndex;

        foreach (CharacterSelection cs in m_characterSelections)
        {
            cs.DeactivatePress(false);
        }
        JoinLettersCoordinator.Instance.CharacterSelected(characterIndex);
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

    // Use this for initialization
    public void SetUp (List<DataRow> lettersPool)
    {
		m_lettersPool = lettersPool;

        foreach (DataRow myPh in m_lettersPool)
        {
            string imageFilename =
                            string.Format("Images/mnemonics_images_png_250/{0}_{1}",
                            myPh["phoneme"],
                            myPh["mneumonic"].ToString().Replace(" ", "_"));

            m_phonemeImages[myPh] = (Texture2D)Resources.Load(imageFilename);
        }

        m_progressScoreBar.SetStarsTarget(JoinLettersCoordinator.Instance.GetTargetScore());
		
		m_blackboard.MoveWidgets();
		
		m_bennyTheBook.SetInstruction("MATCH_LETTERS_INSTRUCTION");
    }

    public IEnumerator SetUpNext()
    {
		m_panelDepthIncrement = 1;
		
		Dictionary<string, DataRow> letters = new Dictionary<string, DataRow>();
		
		int pairsToShowAtOnce = JoinLettersCoordinator.Instance.GetPairsToShowAtOnce();
		
        while (letters.Count < pairsToShowAtOnce)
        {
			DataRow letterData = m_lettersPool[Random.Range(0, m_lettersPool.Count)];
			letters[letterData["phoneme"].ToString()] = letterData;
            yield return null;
        }
        
		/*
        List<Vector3> positions = new List<Vector3>();
        Vector3 delta = m_highCorner.transform.localPosition - m_lowCorner.transform.localPosition;
        
        for (int index = 0; index < pairsToShowAtOnce * 2; ++index)
        {
            int x = index % pairsToShowAtOnce;
            int y = index / pairsToShowAtOnce;
            positions.Add(
                m_lowCorner.transform.localPosition +
                new Vector3((delta.x / pairsToShowAtOnce) * (x + 0.5f),
                    (delta.y / 2) * (y + 0.5f), 0)
                    + new Vector3(Random.Range(-delta.x / (pairsToShowAtOnce*20.0f), delta.x / (pairsToShowAtOnce*20.0f)),
                        Random.Range(-delta.y / 5, delta.y / 5),
                        0)
                        );
        }
		
		foreach(KeyValuePair<string, DataRow> letter in letters)
        {
            GameObject newText = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordBoxPrefab,
                m_spawnTransform);
           
            while(true)
			{
				Vector3 newPosition = new Vector3(Random.Range(m_lowCorner.position.x, m_highCorner.position.x), Random.Range(m_lowCorner.position.y, m_highCorner.position.y),
															newText.transform.position.z);
				
				bool legalPosition = false;
				
				for(int index = 0; index < m_spawnedObjects.Count; ++index)
				{
					if(m_spawnedObjects[index].collider.bounds.Contains(newPosition))
					{
						legalPosition = true;
					}
				}
				
				if(legalPosition)
				{
					newText.transform.position = newPosition;
					break;
				}
			}
			
			newText.transform.localScale = Vector3.zero;
			
			m_spawnedObjects.Add(newText);
			
			
			GameObject newImage = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pictureBoxPrefab,
    m_spawnTransform);

            while(true)
			{
				Vector3 newPosition = new Vector3(Random.Range(m_lowCorner.position.x, m_highCorner.position.x), Random.Range(m_lowCorner.position.y, m_highCorner.position.y),
															newText.transform.position.z);
				
				bool legalPosition = false;
				
				for(int index = 0; index < m_spawnedObjects.Count; ++index)
				{
					if(m_spawnedObjects[index].collider.bounds.Contains(newPosition))
					{
						legalPosition = true;
					}
				}
				
				if(legalPosition)
				{
					newImage.transform.position = newPosition;
					break;
				}
			}
			
			Texture2D texture = m_phonemeImages[letter.Value];
			
			newText.GetComponent<LetterPictureJoinableDrag>().SetUp(this, letter.Value, texture);
            newImage.GetComponent<LetterPictureJoinableDrag>().SetUp(this, letter.Value, texture);
 
            newImage.transform.localScale = Vector3.zero;
            
            m_spawnedObjects.Add(newImage);
        }
		*/
		
		List<Vector3> positions = new List<Vector3>();
        Vector3 delta = m_highCorner.transform.localPosition - m_lowCorner.transform.localPosition;
        
        for (int index = 0; index < pairsToShowAtOnce * 2; ++index)
        {
            int x = index % pairsToShowAtOnce;
            int y = index / pairsToShowAtOnce;
            positions.Add(
                m_lowCorner.transform.localPosition +
                new Vector3((delta.x / pairsToShowAtOnce) * (x + 0.5f),
                    (delta.y / 2) * (y + 0.5f), 0)
                    + new Vector3(Random.Range(-delta.x / (pairsToShowAtOnce*20.0f), delta.x / (pairsToShowAtOnce*20.0f)),
                        Random.Range(-delta.y / 5, delta.y / 5),
                        0)
                        );
        }
        
		foreach(KeyValuePair<string, DataRow> letter in letters)
        {
            GameObject newText = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_wordBoxPrefab,
                m_spawnTransform);
            GameObject newImage = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pictureBoxPrefab,
    m_spawnTransform);
			
			newText.transform.localScale = Vector3.zero;
            newImage.transform.localScale = Vector3.zero;

            int posIndex = Random.Range(0, positions.Count);
            Vector3 posA = positions[posIndex];
            positions.RemoveAt(posIndex);

            newText.transform.localPosition = posA;

            posIndex = Random.Range(0, positions.Count);
            Vector3 posB = positions[posIndex];
            positions.RemoveAt(posIndex);

            newImage.transform.localPosition = posB;
			
			Texture2D texture = m_phonemeImages[letter.Value];
			
			newText.GetComponent<LetterPictureJoinableDrag>().SetUp(this, letter.Value, texture);
            newImage.GetComponent<LetterPictureJoinableDrag>().SetUp(this, letter.Value, texture);

            m_spawnedObjects.Add(newText);
            m_spawnedObjects.Add(newImage);
        }
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");

        yield break;
    }

    IEnumerator AddPoint()
    {
		m_spawnedObjects.Clear();

        yield return new WaitForSeconds(2.0f);
		
		m_characterPopper.PopCharacter();

        WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT");

        m_score++;
        m_progressScoreBar.SetScore(m_score);
        m_progressScoreBar.SetStarsCompleted(m_score);

        if (m_score == JoinLettersCoordinator.Instance.GetTargetScore())
		{
			JoinLettersCoordinator.Instance.IncrementNumFinishedPlayers();
		}
		else
		{
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(SetUpNext());
        }
    }

    public void Connect(LetterPictureJoinableDrag a, LetterPictureJoinableDrag b)
    {
        if (a != b)
        {
            if (a.IsPicture() != b.IsPicture())
            {
				LetterPictureJoinableDrag letterObject = a.IsPicture() ? b : a;
				
				if(SessionInformation.Instance.GetNumPlayers() == 1)
				{
					JoinLettersCoordinator.Instance.PlayLetterSound(letterObject.GetLetterData());
				}
				
                if (a.GetLetterData() == b.GetLetterData())
                {                    
					m_blackboard.Hide();
					StartCoroutine(movePictures(a, b));
                }
                else
                {
					DisplayHint(m_phonemeImages[letterObject.GetLetterData()], letterObject.GetWord(), letterObject.GetWord());
                }
            }
        }
    }
	
	void DisplayHint(Texture2D texture, string word, string colorReplace)
	{
        m_blackboard.ShowImage(texture, word, colorReplace);
	}
	
	IEnumerator movePictures(LetterPictureJoinableDrag a, LetterPictureJoinableDrag b)
	{
		a.GetComponent<UIPanel>().depth += m_panelDepthIncrement;
		b.GetComponent<UIPanel>().depth += m_panelDepthIncrement;
		++m_panelDepthIncrement;
		
		a.Off(a.transform.position.x < b.transform.position.x ? m_leftOff : m_rightOff);
        b.Off(a.transform.position.x < b.transform.position.x ? m_rightOff : m_leftOff);
        m_spawnedObjects.Remove(a.gameObject);
        m_spawnedObjects.Remove(b.gameObject);

        if (m_spawnedObjects.Count == 0)
        {                        
            StartCoroutine(AddPoint());
        }
		
		yield return new WaitForSeconds(2.5f);
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("VOCAL_CORRECT_PLUS");
	}
	
	public void DestroyJoinables()
	{
		foreach(GameObject spawnedObject in m_spawnedObjects)
		{
			if(spawnedObject != null)
			{
				spawnedObject.GetComponent<LetterPictureJoinableDrag>().DestroyJoinable();
			}
		}
	}
	
	public int GetScore()
	{
		return m_score;
	}
}
