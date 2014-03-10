using UnityEngine;
using System.Collections;

public class EnviroLoader : Singleton<EnviroLoader> 
{
	[SerializeField]
	private UITexture[] m_backgrounds;

	GenericEnviro m_genericEnviro;
	bool m_foundEnviro = false;

	// Use this for initialization
	void Start () 
	{
		JourneyInformation.Environment enviro = JourneyInformation.Instance.GetEnvironment();
		m_genericEnviro = Resources.Load<GenericEnviro>("Generic/" + enviro + "_Generic");

		if(m_genericEnviro != null)
		{
			Texture2D tex = m_genericEnviro.GetBackground();

			foreach(UITexture background in m_backgrounds)
			{
				background.mainTexture = tex;
			}
		}
		else
		{
			Debug.LogError("m_genericEnviro is null");
		}
	}
	
	public static IEnumerator WaitForEnvironment()
	{
		while(!EnviroLoader.Instance.m_foundEnviro)
		{
			yield return null;
		}
	}

	public string GetContainerOffName(int index = -1)
	{
		return m_genericEnviro.GetContainerOffName(index);
	}

	public string GetContainerOnName(int index = -1)
	{
		return m_genericEnviro.GetContainerOnName(index);
	}
}
