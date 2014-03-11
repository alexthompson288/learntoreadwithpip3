using UnityEngine;
using System.Collections;
using System;

public class GenericEnviro : MonoBehaviour 
{
	[SerializeField]
	private Texture2D[] m_backgrounds;
	[SerializeField]
	private string[] m_containerOffNames;
	[SerializeField]
	private string[] m_containerOnNames;
	[SerializeField]
	private AudioClip m_audioClip;
	[SerializeField]
	private float m_volume; // 0-1


	public Texture2D GetBackground(int index = -1)
	{
		if(index == -1)
		{
			index = UnityEngine.Random.Range(0, m_backgrounds.Length);
		}

		return m_backgrounds[index];
	}

	// Finds an off container randomly or by index
	public string GetContainerOffName(int index = -1) 
	{
		if(index == -1)
		{
			index = UnityEngine.Random.Range(0, m_containerOffNames.Length);
		}

		return m_containerOffNames[index];
	}

	// Finds an off container by matching index with an on container
	public string GetContainerOffName(string onName) 
	{
		int onIndex = Array.IndexOf(m_containerOnNames, onName);

		if(onIndex != -1 && onIndex < m_containerOffNames.Length)
		{
			return m_containerOffNames[onIndex];
		}
		else
		{
			return GetContainerOffName();
		}
	}

	// Finds an on container randomly or by index
	public string GetContainerOnName(int index = -1) 
	{
		if(index == -1)
		{
			index = UnityEngine.Random.Range(0, m_containerOnNames.Length);
		}
		
		return m_containerOnNames[index];
	}

	// Finds an on container by matching index with an off container
	public string GetContainerOnName(string offName) 
	{
		int offIndex = Array.IndexOf(m_containerOffNames, offName);

		if(offIndex != -1 && offIndex < m_containerOnNames.Length)
		{
			return m_containerOnNames[offIndex];
		}
		else
		{
			return GetContainerOnName();
		}
	}

	public AudioClip GetAudioClip()
	{
		return m_audioClip;
	}

	public float GetVolume()
	{
		return Mathf.Clamp01(m_volume);
	}
}
