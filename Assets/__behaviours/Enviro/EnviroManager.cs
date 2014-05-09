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
		Mountains,
        Count // Count MUST be last
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

    public void RandomEnvironment()
    {
        SetEnvironment(Random.Range(0, (int)Environment.Count));
    }
	
	public Environment GetEnvironment()
	{
		return m_environment;
	}
}
