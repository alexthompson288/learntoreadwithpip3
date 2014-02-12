using UnityEngine;
using System.Collections;

public class PipPlane : Singleton<PipPlane>
{
	[SerializeField]
	private UILabel m_numberLabel;
	[SerializeField]
	private GameObject m_pip;
	[SerializeField]
	private Transform m_end;
	
	void Start ()
	{
		UpdateLabel();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.B))
		{
			LevelUp();
		}
	}

	public void UpdateLabel()
	{
		int level = ChallengeMenuCoordinator.Instance.GetLevelProgress() + 1;
		m_numberLabel.text = level.ToString();
	}

	public void LevelUp()
	{
		UpdateLabel();

		Hashtable tweenVar = new Hashtable();

		tweenVar.Add("position", m_end);
		tweenVar.Add("time", 3f);
		tweenVar.Add("easetype", iTween.EaseType.linear);

		iTween.MoveTo(m_pip, tweenVar);
	}
}
