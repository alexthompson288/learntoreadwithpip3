using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LetterRoomGrid : Singleton<LetterRoomGrid> 
{
	[SerializeField]
	private GameObject m_letterButtonPrefab;
	[SerializeField]
	private GameObject m_emptyPrefab;
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;

	List<GameObject> m_spawnedObjects = new List<GameObject>();

	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets ORDER BY number");

		List<DataRow> setData = dt.Rows;

		if(setData.Count > 0)
		{
			int objName = 0;
			foreach(DataRow set in setData)
			{
				List<DataRow> phonemes = GameDataBridge.Instance.GetSetPhonemes(set);

				foreach(DataRow phoneme in phonemes)
				{
					GameObject newPhoneme = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_letterButtonPrefab, transform);

					newPhoneme.name = objName.ToString();
					++objName;

					m_spawnedObjects.Add(newPhoneme);

					LetterButton letterBehaviour = newPhoneme.GetComponent<LetterButton>() as LetterButton;
					letterBehaviour.SetUp(phoneme);
					letterBehaviour.SetMethods(letterBehaviour.PlayPhonemeAudio, new LetterButton.MyMethod[] { letterBehaviour.PlayMnemonicAudio, letterBehaviour.TweenLarge } );

					UIDragPanelContents dragBehaviour = newPhoneme.AddComponent<UIDragPanelContents>() as UIDragPanelContents;
					dragBehaviour.draggablePanel = m_draggablePanel;
				}

				if(phonemes.Count > 0)
				{
					int remainder = phonemes.Count % m_grid.maxPerLine;
					
					if(remainder > 0)
					{
						int numEmpty = m_grid.maxPerLine - remainder;
						for(int i = 0; i < numEmpty; ++i)
						{
							GameObject newEmpty = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, transform);
							
							newEmpty.name = objName.ToString();
							++objName;
							
							m_spawnedObjects.Add(newEmpty);
						}
					}
				}
			}
		}

		yield return new WaitForSeconds(0.1f);

		m_grid.Reposition();
	}
}
