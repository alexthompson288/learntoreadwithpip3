using UnityEngine;
using System.Collections;

public class EnviroLoader : Singleton<EnviroLoader> 
{
	[SerializeField]
	private UITexture[] m_backgrounds;

	GenericEnviro m_genericEnviro;
	bool m_foundEnviro = false;

	AudioSource m_audioSource;

	// Use this for initialization
	void Start () 
	{
		EnviroManager.Environment enviro = EnviroManager.Instance.GetEnvironment();
		m_genericEnviro = Resources.Load<GenericEnviro>("Generic/" + enviro + "_Generic");

		if(m_genericEnviro != null)
		{
			Texture2D tex = m_genericEnviro.GetBackground();

			foreach(UITexture background in m_backgrounds)
			{
				background.mainTexture = tex;
			}

			m_audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
			m_audioSource.loop = true;
			m_audioSource.volume = m_genericEnviro.GetVolume();
			m_audioSource.clip = m_genericEnviro.GetAudioClip();
			if(m_audioSource.clip != null)
			{
				m_audioSource.Play();
			}
		}
		else
		{
			////////D.LogError("m_genericEnviro is null");
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

	public string GetContainerOffName(string onName)
	{
		return m_genericEnviro.GetContainerOffName(onName);
	}

	public string GetContainerOnName(int index = -1)
	{
		return m_genericEnviro.GetContainerOnName(index);
	}

	public string GetContainerOnName(string offName)
	{
		return m_genericEnviro.GetContainerOnName(offName);
	}
}
