using UnityEngine;
using System.Collections;
using System;

public class TTSettings : MonoBehaviour, IEquatable<TTSettings> 
{
	public int m_numTrolls;
	//public bool m_grayscale;
	public int m_animFPS;
	public float m_splineSpeedModifier;
	public UIAtlas[] m_idleAnims;
	public UIAtlas[] m_tickleAnims;
	public UIAtlas m_transitionAnim;
	public GameObject m_animGo;
	public AudioClip m_audioClip;
	public Texture2D m_background;

	public bool Equals(TTSettings other)
	{
		if(other == null)
		{
			return false;
		}
		else
		{
			return name == other.name;
		}
	}
}
