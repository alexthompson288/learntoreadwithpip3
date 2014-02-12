using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnlockItem : Singleton<UnlockItem> {
	[SerializeField]
	private UITexture[] m_itemTextures;
	[SerializeField]
	private TweenOnOffBehaviour m_tweenBehaviour;
	[SerializeField]
	private Collider m_blocker;
	[SerializeField]
	private Transform m_collectionRoomBoardOnPosition;
	[SerializeField]
	private Transform m_itemParent;
	

	public IEnumerator ShowItems() 
	{
		m_blocker.enabled = true;
		
		yield return new WaitForSeconds(3.5f);
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("CHOOSE_ITEM_INSTRUCTION");
		
		Dictionary<string, bool> unlockableItems = SessionInformation.Instance.GetUnlockableItems();
		Debug.Log("unlockableItems.Count : " + unlockableItems.Count);
		List<string> lockedItems = new List<string>();
		
		foreach(KeyValuePair<string, bool> kvp in unlockableItems)
		{
			if(!kvp.Value)
			{
				lockedItems.Add(kvp.Key);
			}
		}
		
		if(lockedItems.Count > 0)
		{
			Debug.Log("Finding shit");
			HashSet<string> imageNames = new HashSet<string>();
			while(imageNames.Count < m_itemTextures.Length)
			{
				imageNames.Add(lockedItems[Random.Range(0, lockedItems.Count)]);
			}
			
			foreach(string name in imageNames)
			{
				Debug.Log(name);
			}
			
			int index = 0;
			foreach(string imageName in imageNames)
			{
				Debug.Log("Loading: Images/collection_room_additional_images 1/" + imageName);
				
				m_itemTextures[index].mainTexture = 
					Resources.Load("Images/collection_room_additional_images 1/" + imageName) as Texture2D;
				
				//m_itemTextures[index].MakePixelPerfect();
				Debug.Log(m_itemTextures[index].mainTexture);
				
				++index;
			}	
		}
		else
		{
			Debug.Log("No shit to find");
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		m_tweenBehaviour.On();
	}
	

	
	public IEnumerator Off(GameObject chosenItem, Transform itemDefaultPosition)
	{
		Vector3 chosenItemLocalScale = chosenItem.transform.localScale;
		chosenItem.transform.parent = null;
		Debug.Log("chosenItemLocalScale: " + chosenItemLocalScale);
		iTween.MoveTo(chosenItem, m_collectionRoomBoardOnPosition.position, 0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_APPEAR");
		
		yield return new WaitForSeconds(1f);
		
		iTween.ScaleTo(chosenItem, Vector3.zero, 0.5f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
		m_blocker.enabled = false;
		m_tweenBehaviour.Off();
		
		yield return new WaitForSeconds(0.5f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_NEW_COLLECTION_ROOM_INSTRUCTION");
		
		yield return new WaitForSeconds(0.5f);
		chosenItem.transform.parent = m_itemParent;
		chosenItem.transform.position = itemDefaultPosition.position;
		TweenScale.Begin(chosenItem, 0f, chosenItemLocalScale); 
	}

}
