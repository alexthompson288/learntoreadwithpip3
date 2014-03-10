using UnityEngine;
using System.Collections;

public class GenericEnviro : MonoBehaviour 
{
	[SerializeField]
	private Texture2D[] m_backgrounds;
	[SerializeField]
	private string[] m_containerOffNames;
	[SerializeField]
	private string[] m_containerOnNames;


	public Texture2D GetBackground(int index = -1)
	{
		if(index == -1)
		{
			index = Random.Range(0, m_backgrounds.Length);
		}

		return m_backgrounds[index];
	}

	public string GetContainerOffName(int index = -1)
	{
		if(index == -1)
		{
			index = Random.Range(0, m_containerOffNames.Length);
		}

		return m_containerOffNames[index];
	}

	public string GetContainerOnName(int index = -1)
	{
		if(index == -1)
		{
			index = Random.Range(0, m_containerOnNames.Length);
		}
		
		return m_containerOnNames[index];
	}
}
