using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class ChangeProgramme : MonoBehaviour 
{
	[SerializeField]
	private string m_programmeName;

	void OnClick()
	{
		GameManager.Instance.SetProgramme(m_programmeName);
	}
}
