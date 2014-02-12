using UnityEngine;
using System.Collections;

public class UnlockableItem : MonoBehaviour 
{
	[SerializeField]
	private Transform m_defaultPosition;
	
	void OnClick () 
	{
		SessionInformation.Instance.UnlockItem(GetComponent<UITexture>().mainTexture.name);
		StartCoroutine(UnlockItem.Instance.Off(gameObject, m_defaultPosition));
	}
}
