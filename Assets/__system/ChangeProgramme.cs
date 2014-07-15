using UnityEngine;
using System.Collections;

public class ChangeProgramme : MonoBehaviour 
{
	[SerializeField]
	private string m_programmeName;

	void OnClick()
	{
		GameManager.Instance.SetProgramme(m_programmeName);
	}
}
