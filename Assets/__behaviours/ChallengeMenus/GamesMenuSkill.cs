using UnityEngine;
using System.Collections;

public class GamesMenuSkill : MonoBehaviour 
{
	[SerializeField]
	private string m_levelProgress;
	[SerializeField]
	private string m_starProgress;

	public string GetLevelProgressName()
	{
		return m_levelProgress;
	}

	public string GetStarProgressName()
	{
		return m_starProgress;
	}
}
