using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System.Linq;

public class TTItemLoader : Singleton<TTItemLoader> 
{
	[SerializeField]
	private GameObject m_draggableItemPrefab;
	[SerializeField]
	private Transform m_cutOff;
	[SerializeField]
	private TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private List<Texture2D> m_letterTextures = new List<Texture2D>();

	List<TTDraggable> m_spawnedItems = new List<TTDraggable>();
	
	enum menus
	{
		none,
		mnemonics,
		food,
		stickers,
		backgrounds
	}

	menus m_currentMenu;

	int SortByName(Texture2D a, Texture2D b)
	{
		if (a.name.Length == b.name.Length)
		{
			return System.String.Compare(a.name, b.name);
		}
		else
		{
			return a.name.Length > b.name.Length ? -1 : 1;
		}
	}

	void Awake()
	{
		m_letterTextures.Sort(SortByName);
	}

	public void LoadStickers ()
	{
		if(m_currentMenu == menus.stickers)
		{
			Off ();
		}
		else
		{
			Dictionary<Texture2D, AudioClip> stickerItems = new Dictionary<Texture2D, AudioClip>();

			Object[] objs = Resources.LoadAll("tt_animatedStickers");
			foreach(Object obj in objs)
			{
				stickerItems.Add((Texture2D)obj, null);
			}

			StartCoroutine(LoadItems(stickerItems, OnStickerCreate));



			m_currentMenu = menus.stickers;
		}
	}

	
	void OnStickerCreate()
	{
		foreach(TTDraggable item in m_spawnedItems)
		{
			if(!TTInformation.Instance.IsItemUnlocked(ChooseUser.Instance.GetCurrentUser(), item.GetTextureName()))
			{
				item.Lock();
			}	
		}
	}
	
	public void LoadBackgrounds ()
	{
		if(m_currentMenu == menus.backgrounds)
		{
			Off ();
		}
		else
		{
			Dictionary<Texture2D, AudioClip> backgroundItems = new Dictionary<Texture2D, AudioClip>();
			Texture2D[] backgrounds = Resources.LoadAll<Texture2D>("tt_backgrounds");
			foreach(Texture2D background in backgrounds)
			{
				backgroundItems.Add(background, null);
			}

			StartCoroutine(LoadItems(backgroundItems, OnBackgroundCreate));



			m_currentMenu = menus.backgrounds;
		}
	}

	void OnBackgroundCreate()
	{
		foreach(TTDraggable item in m_spawnedItems)
		{
			item.MakeBackground();
			
			if(!TTInformation.Instance.IsItemUnlocked(ChooseUser.Instance.GetCurrentUser(), item.GetTextureName()))
			{
				item.Lock();
			}	
		}
	}

	public void LoadFood ()
	{
		if(m_currentMenu == menus.food)
		{
			Off ();
		}
		else
		{
			Object[] objs = Resources.LoadAll("tt_food");

			Dictionary<Texture2D, AudioClip> foodItems = new Dictionary<Texture2D, AudioClip>();

			foreach(Object obj in objs)
			{
				foodItems.Add((Texture2D)obj, null);
			}

			StartCoroutine(LoadItems(foodItems, OnFoodCreate));

			m_currentMenu = menus.food;


		}
	}
	
	void OnFoodCreate()
	{
		foreach(TTDraggable item in m_spawnedItems)
		{
			item.MakeFood();
		}
	}


	public int SortMnemonicData(DataRow rowA, DataRow rowB)
	{
		string a = rowA["phoneme"].ToString();
		string b = rowB["phoneme"].ToString();

		if(a[0] < b[0])
		{
			return -1;
		}
		else if(a[0] > b[0])
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}


	public void LoadMnemonics () 
	{
		if(m_currentMenu == menus.mnemonics)
		{
			Off ();
		}
		else
		{
			/*
			List<string> lettersToUse = new List<string>();
			for (char i = 'a'; i <= 'z'; ++i)
			{
				string result = i == 'q' ? "qu" : i.ToString();
				lettersToUse.Add(result);
			}
			*/

			HashSet<DataRow> mnemonicSet = new HashSet<DataRow>();
			//SortedSet<DataRow> mnemonics = new SortedSet<DataRow>(new PhonemeSorter());

			int[] sectionIds = new int[] { 1405, 1406, 1407 };

			for(int i = 0; i < sectionIds.Length; ++i)
			{
				int sectionId = sectionIds[i];
				DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from data_phonemes INNER JOIN phonemes ON phoneme_id=phonemes.id WHERE section_id=" + sectionId);
				
				foreach(DataRow row in dt.Rows)
				{
					mnemonicSet.Add(row);
				}
			}

			List<DataRow> mnemonics = mnemonicSet.ToList();

			mnemonics.Sort(SortMnemonicData);


			Dictionary<Texture2D, AudioClip> mnemonicItems = new Dictionary<Texture2D, AudioClip>();

			int letterTextureIndex = 0;
			foreach(DataRow myPh in mnemonics)
			{
				string imageFilename =
					string.Format("Images/mnemonics_images_png_250/{0}_{1}",
						           myPh["phoneme"],
						           myPh["mneumonic"].ToString().Replace(" ", "_"));
					
				Texture2D mnemonicTexture = (Texture2D)Resources.Load(imageFilename);
				AudioClip mnemonicAudio = LoaderHelpers.LoadMnemonic(myPh);
					
				if(mnemonicTexture != null)
				{
					mnemonicItems.Add(mnemonicTexture, mnemonicAudio);
				}
				else
				{
					Debug.Log("Could not find " + imageFilename);
				}
					
					
				AudioClip phonemeAudio = AudioBankManager.Instance.GetAudioClip(myPh["grapheme"].ToString());
				mnemonicItems.Add(m_letterTextures[letterTextureIndex], phonemeAudio);
				++letterTextureIndex;
			}

			/*
			//for(int i = 0; i < lettersToUse.Count; ++i)
			foreach(DataRow row in mnemonics)
			{
				DataTable dtp = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonemes where phoneme='" + lettersToUse[i] + "'");
				if ( dtp.Rows.Count > 0 )
				{
					DataRow myPh = dtp.Rows[0];
					
					string imageFilename =
						string.Format("Images/mnemonics_images_png_250/{0}_{1}",
						              myPh["phoneme"],
						              myPh["mneumonic"].ToString().Replace(" ", "_"));
					
					Texture2D mnemonicTexture = (Texture2D)Resources.Load(imageFilename);
					AudioClip mnemonicAudio = LoaderHelpers.LoadMnemonic(myPh);

					if(mnemonicTexture != null)
					{
						mnemonicItems.Add(mnemonicTexture, mnemonicAudio);
					}
					else
					{
						Debug.Log("Could not find " + imageFilename);
					}


					AudioClip phonemeAudio = AudioBankManager.Instance.GetAudioClip(myPh["grapheme"].ToString());
					mnemonicItems.Add(m_letterTextures[i], phonemeAudio);
				}
			}
			*/

			StartCoroutine(LoadItems(mnemonicItems));

			m_currentMenu = menus.mnemonics;
		}
	}

	IEnumerator LoadItems (Dictionary<Texture2D, AudioClip> items, System.Action ac = null)
	{
		if(m_currentMenu != menus.none)
		{
			m_tweenBehaviour.Off();
			yield return new WaitForSeconds(m_tweenBehaviour.GetDuration() + m_tweenBehaviour.GetDelayOff());
		}

		DestroyItems();

		foreach(KeyValuePair<Texture2D, AudioClip> kvp in items)
		{
			GameObject newItem = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
				m_draggableItemPrefab, transform);

			TTDraggable draggable = newItem.GetComponent<TTDraggable>();
			draggable.SetUp(kvp.Key.name, kvp.Key, null, m_cutOff, kvp.Value);
			m_spawnedItems.Add(draggable);
		}

		if(ac != null)
		{
			ac();
		}

		m_grid.Reposition();

		m_tweenBehaviour.On();

		yield break;
	}

	void DestroyItems()
	{
		for(int i = 0; i < m_spawnedItems.Count; ++i)
		{
			Destroy(m_spawnedItems[i].gameObject);
		}
		m_spawnedItems.Clear();
		m_grid.Reposition();

		Resources.UnloadUnusedAssets();
	}

	void Off()
	{
		m_tweenBehaviour.Off();
		m_currentMenu = menus.none;
	}
}
