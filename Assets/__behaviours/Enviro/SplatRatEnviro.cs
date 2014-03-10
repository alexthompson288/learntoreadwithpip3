using UnityEngine;
using System.Collections;

public class SplatRatEnviro : MonoBehaviour 
{
	[SerializeField]
	private Texture2D m_frontTex;
	[SerializeField]
	private Texture2D m_rearTex;

	public Texture2D GetFrontTex()
	{
		return m_frontTex;
	}

	public Texture2D GetRearTex()
	{
		return m_rearTex;
	}
}
