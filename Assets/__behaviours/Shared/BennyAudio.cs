using UnityEngine;
using System.Collections;

public class BennyAudio : MonoBehaviour 
{
	string m_instruction;

	public void SetInstruction (string instruction) 
	{
		m_instruction = instruction;
	}
	
	void OnClick()
	{
		if(m_instruction != null)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent(m_instruction);
		}
	}
}
