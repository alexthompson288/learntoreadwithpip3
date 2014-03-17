using UnityEngine;
using System.Collections;

public class EnviroManager : Singleton<EnviroManager> 
{
	public enum Environment
	{
		Forest,
		Underwater,
		Castle,
		Farm,
		School,
		Space
	}
	
	Environment m_environment;
	
	public void SetEnvironment(Environment environment)
	{
		m_environment = environment;
	}
	
	public Environment GetEnvironment()
	{
		return m_environment;
	}
}
