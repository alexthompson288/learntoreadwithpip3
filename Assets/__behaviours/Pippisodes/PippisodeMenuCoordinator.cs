using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class PippisodeMenuCoordinator : MonoBehaviour 
{
	[SerializeField]
	private UIGrid m_grid;
	[SerializeField]
	private UIDraggablePanel m_draggablePanel;
	[SerializeField]
	private GameObject m_pippisodeButtonPrefab;
	[SerializeField]
	private ClickEvent m_playButton;
	[SerializeField]
	private UITexture m_backgroundTexture;

	int m_currentPippisodeId;

	// Use this for initialization
	void Start () 
	{
		m_playButton.OnSingleClick += OnClickPlayButton;

		int[] pippisodeIds = PippisodeInfo.Instance.GetPippisodeIds();

		m_currentPippisodeId = pippisodeIds[0];

		for(int i = 0; i < pippisodeIds.Length; ++i)
		{
			GameObject button = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pippisodeButtonPrefab, m_grid.transform);

			button.GetComponentInChildren<UILabel>().text = (i + 1).ToString();

			if(!PippisodeInfo.Instance.IsUnlocked(pippisodeIds[i]))
			{
				button.GetComponentInChildren<UISprite>().color = Color.gray;
			}

			button.GetComponent<ClickEvent>().OnSingleClick += OnChoosePippisode;
			button.GetComponent<ClickEvent>().SetInt(pippisodeIds[i]);

			button.GetComponent<UIDragPanelContents>().draggablePanel = m_draggablePanel;
		}

		m_grid.Reposition();
	}

	void OnClickPlayButton(ClickEvent click)
	{
		Debug.Log("OnClickPlayButton()");
		if(PippisodeInfo.Instance.IsUnlocked(m_currentPippisodeId))
		{
			//SessionManager.Instance.SetSessionType(SessionManager.ST.Pippisode);
			SessionManager.Instance.OnChooseSession(SessionManager.ST.Voyage, m_currentPippisodeId);
		}
	}

	void OnChoosePippisode(ClickEvent click)
	{
		Debug.Log("Chose Pippisode: " + click.GetInt());

		m_currentPippisodeId = click.GetInt();

		// Change background texture
	}
}
