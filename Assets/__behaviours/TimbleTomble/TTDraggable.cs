using UnityEngine;
using System.Collections;
using System.IO;

public class TTDraggable : MonoBehaviour 
{	
	[SerializeField]
	private UITexture m_texture;
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private GameObject m_lock;
	[SerializeField]
	private GameObject[] m_coins;
	
	private GameObject m_instantiatedSprite;
	private Vector2 m_totalDrag;
	private string m_textureName;
	
	private UIDraggablePanel m_dragPanel;
	private Transform m_cutOff;
	private AudioClip m_clip;
	
	private bool m_isAnimated = false;
	private bool m_isBackground = false;

	private int m_price = 1;
	
	public enum itemStates
	{
		unlocked,
		locked,
		food
	}

	itemStates m_itemState = itemStates.unlocked;

	public void SetUp(string textureName, Texture2D texture, string label, Transform cutOff, AudioClip audioClip)
	{
		m_cutOff = cutOff;
		m_dragPanel = transform.parent.GetComponent<UIDraggablePanel>();
		m_label.text = label == null ? "" : label;
		m_texture.mainTexture = texture;
		m_textureName = textureName;
		m_clip = audioClip;

		StartCoroutine(SetFromDatabase(textureName));
	}

	IEnumerator SetFromDatabase(string name)
	{
		yield return StartCoroutine(GameDataBridge.WaitForDatabase());
		
		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from tombleitems WHERE name='" + name + "'");

		if(dt.Rows.Count > 0)
		{
			DataRow item = dt.Rows[0];
			m_price = System.Convert.ToInt32(item["price"]);
			m_isAnimated = (item["is_animated"].ToString() == "t");
		}
		else
		{
			m_price = 1;
			m_isAnimated = false;
		}

		if(m_itemState != itemStates.unlocked)
		{
			System.Array.Sort(m_coins, SortCoins);
			for(int i = 0; i < m_coins.Length && i < m_price; ++i)
			{
				m_coins[i].SetActive(true);
			}
		}
	}

	int SortCoins(GameObject a, GameObject b)
	{
		if(a.transform.position.x < b.transform.position.x)
		{
			return -1;
		}
		else if(a.transform.position.x > b.transform.position.x)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}

	public int GetPrice()
	{
		return m_price;
	}

	// Unlocks items
	void OnClick()
	{
		if(m_itemState == itemStates.locked && m_price <= TTInformation.Instance.GetGoldCoins())
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
			TTInformation.Instance.UnlockItem(m_textureName);
			TTGoldCoinBar.Instance.SpendCoin(transform.position, m_price);
			Unlock();
		}
		else if(m_itemState == itemStates.unlocked && m_isBackground)
		{
			TTCoordinator.Instance.SetBackground((Texture2D)m_texture.mainTexture);
		}
	}

	// TODO: You should be able to change the background by dragging and then a copy of the background will move out with mouse. 
	//       When the dragged background is dropped, it tweens position and scale to replace current background

	//  Manages calls that create and destroy sprites from menu onto Timble Tomble canvas
	void OnDrag(Vector2 drag)
	{
		if((m_itemState == itemStates.unlocked && !m_isBackground) || (m_itemState == itemStates.food && m_price <= TTInformation.Instance.GetGoldCoins()))
		{
			Ray camPos = UICamera.currentCamera.ScreenPointToRay(
				new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
			m_totalDrag += drag;
			if (camPos.origin.y > m_cutOff.transform.position.y)
			{
				m_dragPanel.enabled = false;
				if (m_instantiatedSprite == null)
				{
					m_instantiatedSprite = TTCoordinator.Instance.AddAnimatedSprite(m_textureName, transform.position);

					if(m_instantiatedSprite == null)
					{
						m_instantiatedSprite = TTCoordinator.Instance.AddPoppedSprite(
							m_textureName, (Texture2D)m_texture.mainTexture,
							new Vector3(camPos.origin.x,camPos.origin.y,0),
							m_price);
					}

					WingroveAudio.WingroveRoot.Instance.PostEvent("PICK_UP_STICKER");
				}
			}
			else
			{
				m_dragPanel.enabled = true;
				if (m_instantiatedSprite != null)
				{
					TTCoordinator.Instance.DestroyPoppedSprite(m_instantiatedSprite);
					m_instantiatedSprite = null;
				}
			}
			
			if (m_instantiatedSprite != null)
			{
				m_instantiatedSprite.transform.position = new Vector3(camPos.origin.x,camPos.origin.y,0);
			}
		}
	}
	
	void OnPress(bool press)
	{
		if (!press)
		{
			if (m_instantiatedSprite != null)
			{
				WingroveAudio.WingroveRoot.Instance.PostEvent("DROP_STICKER");

				if(m_itemState == itemStates.food)
				{
					TTCoordinator.Instance.FeedTroll(m_instantiatedSprite, transform);
				}

				TTPlacedAnimated placedAnimated = m_instantiatedSprite.GetComponent<TTPlacedAnimated>() as TTPlacedAnimated;
				if(placedAnimated != null)
				{
					placedAnimated.CreatePath();
				}
			}
			m_instantiatedSprite = null;
			m_dragPanel.enabled = true;
		}
		else
		{
			TTCoordinator.Instance.PlayClip(m_clip);
		}
	}

	public void Lock()
	{
		m_itemState = itemStates.locked;
		m_lock.SetActive(true);
	}

	public void Unlock()
	{
		m_itemState = itemStates.unlocked;
		m_lock.SetActive(false);
	}

	public void MakeFood()
	{
		m_itemState = itemStates.food;
	}

	public void MakeBackground()
	{
		m_isBackground = true;
	}

	public string GetTextureName()
	{
		return m_textureName;
	}	
}
