using UnityEngine;
using System.Collections;

public class PanelReference : MonoBehaviour 
{
	[SerializeField]
	private UIPanel panel;
	
	public int GetDepth()
	{
		return panel.depth;
	}
}
