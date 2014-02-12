using UnityEngine;
using System.Collections;

// TODO: Deprecate this class. Put a scipt on each of the boards that deals with particle effects 
//       and wobbling. Call functions in these new scripts from the coordinator      
public class LetterBankParticleSystem : MonoBehaviour {
	[SerializeField]
	private Camera m_mainCamera;
	[SerializeField]
	private Camera m_particleCamera;
	[SerializeField]
	private Transform m_collectStarsBoardPosition;
	[SerializeField]
	private Transform m_collectionRoomBoardPosition;
	[SerializeField]
	private WobbleGUIElement m_collectionRoomWobble;
	[SerializeField]
	private WobbleGUIElement m_collectStarsWobble;
	
	int m_lastNumCoins;
	bool m_onCollectionRoomBoard;
	
	IEnumerator Start()
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		yield return new WaitForSeconds(2f);
		
		Vector3 newPosition = Vector3.zero;
		int numCoins = SessionInformation.Instance.GetCoins();
		if(numCoins > 0)
		{
			newPosition = m_collectionRoomBoardPosition.position;
			
			m_collectionRoomWobble.enabled = true;
			m_collectStarsWobble.enabled = false;
			
			m_onCollectionRoomBoard = true;
		}
		else
		{
			newPosition = m_collectStarsBoardPosition.position;
			
			m_collectionRoomWobble.enabled = false;
			m_collectStarsWobble.enabled = true;
			
			m_onCollectionRoomBoard = false;
		}
		
		iTween.MoveTo(gameObject, newPosition, 0.5f);
		
		m_lastNumCoins = SessionInformation.Instance.GetCoins();
	}
	
	// Update is called once per frame
	void Update () 
	{
		int numCoins = SessionInformation.Instance.GetCoins();
		if(numCoins != m_lastNumCoins)
		{
			if(numCoins == 0 && m_onCollectionRoomBoard)
			{
				Vector3 newPosition = m_collectStarsBoardPosition.position;
				iTween.MoveTo(gameObject, newPosition, 0.5f);
				m_onCollectionRoomBoard = false;
			}
			else if(numCoins > 0 && !m_onCollectionRoomBoard) 
			{
				Vector3 newPosition = m_collectionRoomBoardPosition.position;
				iTween.MoveTo(gameObject, newPosition, 1f);
				m_onCollectionRoomBoard = true;
			}
		}
		
		m_lastNumCoins = SessionInformation.Instance.GetCoins();
	}
}
