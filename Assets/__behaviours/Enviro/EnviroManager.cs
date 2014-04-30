using UnityEngine;
using System.Collections;

public class EnviroManager : Singleton<EnviroManager> 
{
	public enum Environment
	{
		Forest,
        Farm,
        Castle,
		Underwater,
		Market,
		Mountains
	}
	
	Environment m_environment;
	
	public void SetEnvironment(Environment environment)
	{
		m_environment = environment;
	}

    public void SetEnvironment(int environmentIndex)
    {
        m_environment = (Environment)environmentIndex;
    }
	
	public Environment GetEnvironment()
	{
		return m_environment;
	}
}
