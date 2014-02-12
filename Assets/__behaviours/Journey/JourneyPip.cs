using UnityEngine;
using System.Collections;

public class JourneyPip : Singleton<JourneyPip> 
{
	[SerializeField]
	private UIAtlas[] m_atlases;
	[SerializeField]
	private Transform m_pipPosition;
	[SerializeField]
	private Transform m_pipOff;

	Vector3 m_offset;

	[SerializeField] Transform m_lastPoint = null;
	[SerializeField] Transform m_currentPoint = null;

	void Awake()
	{
		m_offset =  transform.position - m_pipPosition.position;
	}

	// Use this for initialization
	IEnumerator Start () 
	{
		while (true)
		{
			GetComponent<SimpleSpriteAnim>().PlayAnimation("OFF_" + RandomizeAtlas().ToString());
			yield return new WaitForSeconds(Random.Range(2.0f, 6.0f));
			GetComponent<SimpleSpriteAnim>().PlayAnimation("ON_" + RandomizeAtlas().ToString());
			yield return new WaitForSeconds(0.9f);
		}
	}

	void Update()
	{
		if(m_currentPoint != null)
		{
			transform.position = m_currentPoint.position + m_offset;
		}
		else
		{
			transform.position = m_pipOff.position;
		}
	}
	
	int RandomizeAtlas ()
	{
		int atlasIndex = Random.Range(0,2);
		UISprite sprite = GetComponent<UISprite>() as UISprite;
		sprite.atlas = m_atlases[atlasIndex];
		sprite.spriteName = "pip_front_positive_" + (atlasIndex + 1).ToString() + "0001";
		
		return atlasIndex;
	}

	public void SetCurrentPoint(Transform currentPoint)
	{
		m_currentPoint = currentPoint;
	}
}
