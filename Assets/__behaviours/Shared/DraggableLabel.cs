using UnityEngine;
using System.Collections;

// Conflict resolved. Merged SetUp(), now has parameters for m_audioClip and changeBackground
public class DraggableLabel : MonoBehaviour 
{
	public delegate void NoDragClick (DraggableLabel draggableBehaviour);
	public event NoDragClick OnNoDragClick;

	public event Release OnRelease;
	public delegate void Release(DraggableLabel draggable);
	
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private AudioSource m_audioSource;
	[SerializeField]
	private BoxCollider m_collider;
	[SerializeField]
	private UITexture m_foregroundTexture;
	[SerializeField]
	private UITexture m_backgroundTexture;
	[SerializeField]
	private UISprite m_backgroundSprite;
	[SerializeField]
	private Texture2D[] m_offTextures;
	[SerializeField]
	private Texture2D[] m_onTextures;
	[SerializeField]
	private string[] m_offSpriteNames;
	[SerializeField]
	private string [] m_onSpriteNames;
	[SerializeField]
	private bool m_linkOnOffIndex = true;
	[SerializeField]
	private float m_tweenDuration = 0.5f;
	[SerializeField]
	private bool m_canDrag = true;

	int m_textureIndex;
	
    Vector3 m_dragOffset;
	Vector3 m_startPosition;
	
	bool m_hasDragged = false;
	
	string m_text;

	DataRow m_data = null;


    void Start()
    {
        transform.localScale = Vector3.one;
        iTween.ScaleFrom(gameObject, Vector3.zero, 0.5f);
		
		if(m_backgroundTexture != null && m_offTextures.Length > 0)
		{
			m_backgroundTexture.mainTexture = m_offTextures[Random.Range(0, m_offTextures.Length)];
		}
		
		if(m_backgroundSprite != null)
		{
			if(m_offSpriteNames.Length > 0)
			{
				m_backgroundSprite.spriteName = m_offSpriteNames[Random.Range(0, m_offSpriteNames.Length)];
			}
			else
			{
				m_backgroundSprite.spriteName = EnviroLoader.Instance.GetContainerOffName();
			}
		}
    }

	public void SetUp(string text, AudioClip audioClip = null, bool changeBackgroundWidth = false, DataRow data = null, Texture2D tex = null)
	{
		m_text = text;

		if(m_label != null)
		{
			m_label.text = m_text;
		}
		
		m_audioSource.clip = audioClip;

		m_data = data;

		if(m_foregroundTexture != null)
		{
			m_foregroundTexture.mainTexture = tex;
		}

		if(changeBackgroundWidth)
		{
			//Debug.Log("Changing bg width");
			int newWidth = (int)(m_label.font.CalculatePrintedSize(m_label.text, false, UIFont.SymbolStyle.None).x*1.3f);

			/*
			if(m_backgroundSprite != null)
			{
				Debug.Log("currentBgWidth: " + m_backgroundSprite.width);
			}

			Debug.Log("newWidth: " + newWidth);
			*/

			if(m_backgroundTexture != null && newWidth > m_backgroundTexture.width)
			{
				m_backgroundTexture.width = newWidth;
			}

			if(m_backgroundSprite != null && newWidth > m_backgroundSprite.width)
			{
				m_backgroundSprite.width = newWidth;
			}
		}

		if(m_collider != null)
		{
			if(m_backgroundSprite != null)
			{
				m_collider.size = m_backgroundSprite.localSize;
			}
			else if(m_backgroundTexture != null)
			{
				m_collider.size = m_backgroundTexture.localSize;
			}
		}
	}

    void OnPress(bool press)
    {
    	if(m_canDrag)
    	{
	    	if (press)
	    	{
				iTween.Stop(gameObject);

				m_startPosition = transform.position;
	        	Ray camPos = UICamera.currentCamera.ScreenPointToRay(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
	        	m_dragOffset = new Vector3(camPos.origin.x, camPos.origin.y, 0) - transform.position;
				
				m_hasDragged = false;
	     	}
	     	else
	     	{
				if(OnRelease != null)
				{
					OnRelease(this);
				}
         	}
         }
    }
	
	void OnClick()
	{
		if(!m_hasDragged)
		{
			if(m_audioSource.clip != null)
			{
				m_audioSource.Play();
			}

			if(OnNoDragClick != null)
			{
				OnNoDragClick(this);
			}
		}
	}

    void OnDrag(Vector2 dragAmount)
    {
		if(m_canDrag)
		{
	        Ray camPos = UICamera.currentCamera.ScreenPointToRay(
	            new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
	        transform.position = new Vector3(camPos.origin.x, camPos.origin.y, 0) - m_dragOffset;

	        m_dragOffset = m_dragOffset - (Time.deltaTime * m_dragOffset);
			
			m_hasDragged = true;
		}
    }
	
	public void TweenToStartPos()
	{
		iTween.MoveTo(gameObject, m_startPosition, 0.5f);
	}

	public void TweenToPos(Vector3 newPos)
	{
		iTween.MoveTo(gameObject, newPos, 0.5f);
	}
	
    IEnumerator DestroyCo()
    {
        collider.enabled = false;
        iTween.Stop(gameObject);
        iTween.ScaleTo(gameObject, Vector3.zero, m_tweenDuration);

        yield return new WaitForSeconds(m_tweenDuration);

        Destroy(gameObject);
    }

    public void Off()
    {
        StartCoroutine(DestroyCo());
    }
	
	public string GetText()
	{
		return m_text;
	}

	public void ChangeToOnTexture()
	{
		if(m_backgroundTexture != null && m_onTextures.Length > 0)
		{
			if(m_linkOnOffIndex && m_onTextures.Length > m_textureIndex)
			{
				m_backgroundTexture.mainTexture = m_onTextures[m_textureIndex];
			}
			else
			{
				m_backgroundTexture.mainTexture = m_onTextures[Random.Range(0, m_onTextures.Length)];
			}
		}

		if(m_backgroundSprite != null)
		{
			if(m_onSpriteNames.Length > 0)
			{
				if(m_linkOnOffIndex && m_onSpriteNames.Length > m_textureIndex)
				{
					m_backgroundSprite.spriteName = m_onSpriteNames[m_textureIndex];
				}
				else
				{
					m_backgroundSprite.spriteName = m_onSpriteNames[Random.Range(0, m_onSpriteNames.Length)];
				}
			}
			else
			{
				if(m_linkOnOffIndex)
				{
					EnviroLoader.Instance.GetContainerOnName(m_backgroundSprite.spriteName);
				}
				else
				{
					EnviroLoader.Instance.GetContainerOnName();
				}
			}
		}
	}

	public void SetCanDrag(bool canDrag)
	{
		m_canDrag = canDrag;
	}

	public void BackgroundScaleTween(Vector3 newLocalScale, float duration = -1)
	{
		if(duration == -1)
		{
			duration = m_tweenDuration;
		}

		if(m_backgroundSprite != null)
		{
			TweenScale.Begin(m_backgroundSprite.gameObject, duration, newLocalScale);
		}

		if(m_backgroundTexture != null)
		{
			TweenScale.Begin(m_backgroundTexture.gameObject, duration, newLocalScale);
		}
	}

	public DataRow GetData()
	{
		return m_data;
	}
}
