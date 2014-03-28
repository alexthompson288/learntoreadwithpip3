using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LetterCollectionRoomItemList : MonoBehaviour {

    [SerializeField]
    private GameObject m_draggableItemPrefab;
    [SerializeField]
    private Transform m_cutOff;
    [SerializeField]
    private Texture2D[] m_additionalTextures;


    List<string> m_lettersToUse = new List<string>();

	// Use this for initialization
	IEnumerator Start () 
    {
        yield return StartCoroutine(GameDataBridge.WaitForDatabase());

        for (char i = 'a'; i <= 'z'; ++i)
        {
            string result = i == 'q' ? "qu" : i.ToString();
            m_lettersToUse.Add(result);
        }
		
		AddStickersFromLetters();
		
        AddAdditionalTextures();

        GetComponent<UIGrid>().Reposition();
	}

    int SortByName(Texture2D a, Texture2D b)
    {
        if (a.name.Length == b.name.Length)
        {
            return string.Compare(a.name, b.name);
        }
        else
        {
            return a.name.Length > b.name.Length ? -1 : 1;
        }
    }
	
	/*
    void AddAdditionalTextures()
    {
        List<Texture2D> sortedList = new List<Texture2D>();
        sortedList.AddRange(m_additionalTextures);
        sortedList.Sort(SortByName);
        foreach (Texture2D texture in sortedList)
        {
            GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
                m_draggableItemPrefab, transform);

            DataTable dtp = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where phoneme='" + texture.name + "'");
            AudioClip audio = null;
            if (dtp.Rows.Count > 0)
            {
                DataRow myPh = dtp.Rows[0];

                string audioFilename = string.Format("{0}",
                        myPh["grapheme"]);

                audio = AudioBankManager.Instance.GetAudioClip(audioFilename);
            }

            CollectionRoomDraggable draggable = newItem.GetComponent<CollectionRoomDraggable>();
            draggable.SetUp(texture.name, texture, null, m_cutOff, audio);
        }
    }
    */
	
	void AddAdditionalTextures()
    {
		List<Texture2D> sortedList = new List<Texture2D>();
		
        Dictionary<string, bool> unlockableItems = SessionInformation.Instance.GetUnlockableItems();
		int i = 0;
		Debug.Log("Adding additional items");
        foreach (KeyValuePair<string, bool> kvp in unlockableItems)
        {
			if(kvp.Value)
			{
				sortedList.Add(Resources.Load("Images/collection_room_additional_images 1/" + kvp.Key) as Texture2D);
				Debug.Log(kvp.Key + " - " + i + ": " + sortedList[sortedList.Count - 1]);
			}
			++i;
		}
		
		Debug.Log("sortedList count before adding adding m_additionalTextures: " + sortedList.Count);
			
        sortedList.AddRange(m_additionalTextures); // Add the letters
        sortedList.Sort(SortByName);
		
		Debug.Log("sortedList count after adding adding m_additionalTextures: " + sortedList.Count);
			
		
        foreach (Texture2D texture in sortedList)
        {
            GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
                m_draggableItemPrefab, transform);

            DataTable dtp = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where phoneme='" + texture.name + "'");
            AudioClip audio = null;
            if (dtp.Rows.Count > 0)
            {
                DataRow myPh = dtp.Rows[0];

                string audioFilename = string.Format("{0}",
                        myPh["grapheme"]);

                audio = AudioBankManager.Instance.GetAudioClip(audioFilename);
            }

            CollectionRoomDraggable draggable = newItem.GetComponent<CollectionRoomDraggable>();
            draggable.SetUp(texture.name, texture, null, m_cutOff, audio);
        }
    }
	
	void AddStickersFromLetters()
    {
        foreach (string letterToUse in m_lettersToUse)
        {
            DataTable dtp = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where phoneme='" + letterToUse + "'");
            if ( dtp.Rows.Count > 0 )
            {
                DataRow myPh = dtp.Rows[0];
				
				if(SessionInformation.Instance.IsLetterUnlocked(myPh["phoneme"].ToString()))
				{
	                string imageFilename =
	                        string.Format("Images/mnemonics_images_png_250/{0}_{1}",
	                        myPh["phoneme"],
	                        myPh["mneumonic"].ToString().Replace(" ", "_"));
	
	                Texture2D loadedTexture = (Texture2D)Resources.Load(imageFilename);
	                AudioClip loadedAudio = LoaderHelpers.LoadMnemonic(myPh);
	
	                GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
	                    m_draggableItemPrefab, transform);
	
	                CollectionRoomDraggable draggable = newItem.GetComponent<CollectionRoomDraggable>();
	                draggable.SetUp(imageFilename, loadedTexture, letterToUse, m_cutOff, loadedAudio);
				}
            }
        }
    }
	
	/*
    void AddStickersFromLetters(int sectionId)
    {
        foreach (string letterToUse in m_lettersToUse)
        {
            DataTable dtp = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where phoneme='" + letterToUse + "'");
            if ( dtp.Rows.Count > 0 )
            {
                DataRow myPh = dtp.Rows[0];
				
				if(SessionInformation.Instance.IsLetterUnlocked(myPh["phoneme"].ToString()))
				{
	                string imageFilename =
	                        string.Format("Images/mnemonics_images_png_250/{0}_{1}",
	                        myPh["phoneme"],
	                        myPh["mneumonic"].ToString().Replace(" ", "_"));
	
	                Texture2D loadedTexture = (Texture2D)Resources.Load(imageFilename);
	                AudioClip loadedAudio = LoaderHelpers.LoadMnemonic(myPh);
	
	                GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
	                    m_draggableItemPrefab, transform);
	
	                CollectionRoomDraggable draggable = newItem.GetComponent<CollectionRoomDraggable>();
	                draggable.SetUp(imageFilename, loadedTexture, letterToUse, m_cutOff, loadedAudio);
				}
            }
        }
    }
    */

    void AddStickersFromWords(int sectionId)
    {
        DataTable dt = DataHelpers.GetSectionWords(sectionId);
        HashSet<string> doneWords = new HashSet<string>();
        foreach (DataRow resultRow in dt.Rows)
        {
            string word = resultRow["word"].ToString();
            if (!doneWords.Contains(word))
            {
                //if (SessionInformation.Instance.IsWordUnlocked(word))
                //{

                    string imageFilename = "Images/word_images_png_350/_" + word;

                    Texture2D loadedTexture = (Texture2D)Resources.Load(imageFilename);
                    string audioFilename = string.Format("{0}",
                            word);
                    AudioClip loadedAudio = LoaderHelpers.LoadAudioForWord(word);

                    GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
            m_draggableItemPrefab, transform);

                    CollectionRoomDraggable draggable = newItem.GetComponent<CollectionRoomDraggable>();
                    draggable.SetUp(imageFilename, loadedTexture, null, m_cutOff, loadedAudio);
                //}
            }
        }
    }
}
