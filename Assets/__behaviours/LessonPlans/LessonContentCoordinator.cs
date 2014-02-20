using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

public class LessonContentCoordinator : Singleton<LessonContentCoordinator> 
{
	[SerializeField]
	private GameObject m_storyPrefab;
	[SerializeField]
	private Transform m_storyParent;
	[SerializeField]
	private GameObject m_setButtonPrefab;
	[SerializeField]
	private UIDraggablePanel m_setPanel;
	[SerializeField]
	private Transform m_setGrid;
	[SerializeField]
	private GameObject m_contentButtonPrefab;
	[SerializeField]
	private UIDraggablePanel m_contentPanel;
	[SerializeField]
	private Transform m_contentGrid;
	[SerializeField]
	ClickEvent m_selectAllButton;
	[SerializeField]
	Color m_selectColor;
	[SerializeField]
	Color m_deselectColor;
	[SerializeField]
	Color m_targetColor;

	LessonInfo.DataType m_dataType = LessonInfo.DataType.Letters;
	string m_setAttribute = "setphonemes";
	string m_contentAttribute = "phonemes";
	string m_contentLabelAttribute = "phoneme";

	List<int> m_contentIds = new List<int>();

	UISprite m_targetSprite = null;

	UISprite m_currentDataSprite;
	UISprite m_currentSetSprite;
	
	Vector3 m_contentGridDefaultPos;
	Vector3 m_setGridDefaultPos;

	// Use this for initialization
	IEnumerator Start () 
	{
		m_contentGridDefaultPos = m_contentGrid.position;
		m_setGridDefaultPos = m_setGrid.transform.position;

		m_selectAllButton.OnSingleClick += OnClickAll;

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		StartCoroutine(FillGrids());
	}

	void DestroyChildren(Transform parent)
	{
		int numChildren = parent.childCount;
		for(int i = numChildren - 1; i > -1; --i)
		{
			Destroy(parent.GetChild(i).gameObject);
		}
	}

	IEnumerator FillGrids()
	{
		DestroyChildren(m_setGrid);
		DestroyChildren(m_contentGrid);
		DestroyChildren(m_storyParent);

		yield return null;

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets ORDER BY number");

		m_setGrid.position = m_setGridDefaultPos;

		if(dt.Rows.Count > 0)
		{
			List<DataRow> sets = dt.Rows;

			int lowestSetNum = -1;
			for(int i = 0; i < sets.Count; ++i)
			{
				if(GameDataBridge.Instance.SetContainsData(sets[i], m_setAttribute, m_contentAttribute))
				{
					GameObject newSetButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setButtonPrefab, m_setGrid);
					newSetButton.GetComponentInChildren<UILabel>().text = String.Format("Set {0}", sets[i]["number"].ToString());
					newSetButton.GetComponent<UIDragPanelContents>().draggablePanel = m_setPanel;
					newSetButton.GetComponent<ClickEvent>().SetData(sets[i]);
					newSetButton.GetComponent<ClickEvent>().OnSingleClick += OnClickSet;

					if(lowestSetNum == -1)
					{
						m_currentSetSprite = newSetButton.GetComponentInChildren<UISprite>() as UISprite;
						m_currentSetSprite.color = m_selectColor;

						lowestSetNum = Convert.ToInt32(sets[i]["number"]);
					}
				}
			}

			m_setGrid.GetComponent<UIGrid>().Reposition();

			if(m_dataType != LessonInfo.DataType.Stories)
			{
				StartCoroutine(FillContentGrid(lowestSetNum));
			}
			else
			{
				SpawnStory();
			}
		}
	}

	void SpawnStory()
	{
		DestroyChildren(m_storyParent);

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + 1);
		
		if(dt.Rows.Count > 0)
		{
			List<DataRow> data = GameDataBridge.Instance.GetSetData(dt.Rows[0], m_setAttribute, m_contentAttribute);

			if(data.Count > 0)
			{
				DataRow storyData = data[0];
				GameObject newStory = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_storyPrefab, m_storyParent);
				//newStory.GetComponentInChildren<UILabel>().text = storyData["title"].ToString();
				newStory.GetComponent<ClickEvent>().SetData(storyData);
				newStory.GetComponent<ClickEvent>().OnSingleClick += OnStoryClick;

				string coverName = dt.Rows[0]["storycoverartwork"] == null ? "" : dt.Rows[0]["storycoverartwork"].ToString().Replace(".png", "");
				Texture2D coverTex = LoaderHelpers.LoadObject<Texture2D>("Images/story_covers/" + coverName);

				if(coverTex != null)
				{
					newStory.GetComponentInChildren<UITexture>().mainTexture = coverTex;
				}

				if(LessonInfo.Instance.HasData(Convert.ToInt32(storyData), m_dataType))
				{
					newStory.GetComponentInChildren<UISprite>().color = m_selectColor;
				}
			}
		}
	}

	IEnumerator FillContentGrid(int setNum)
	{
		DestroyChildren(m_contentGrid);

		m_contentIds.Clear();
		m_targetSprite = null;

		yield return null;

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + setNum);

		m_contentGrid.position = m_contentGridDefaultPos;

		if(dt.Rows.Count > 0)
		{
			List<DataRow> data = GameDataBridge.Instance.GetSetData(dt.Rows[0], m_setAttribute, m_contentAttribute);

			foreach(DataRow datum in data)
			{
				GameObject newContentButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_contentButtonPrefab, m_contentGrid);
				newContentButton.GetComponentInChildren<UILabel>().text = datum[m_contentLabelAttribute].ToString();
				newContentButton.GetComponent<UIDragPanelContents>().draggablePanel = m_contentPanel;
				newContentButton.GetComponent<ClickEvent>().SetData(datum);
				newContentButton.GetComponent<ClickEvent>().OnSingleClick += OnClickContent;

				UISprite backgroundSprite = newContentButton.GetComponentInChildren<UISprite>() as UISprite;
				int id = Convert.ToInt32(datum["id"]);
				if(LessonInfo.Instance.HasData(id, m_dataType))
				{
					if(LessonInfo.Instance.IsTarget(id, m_dataType))
					{
						m_targetSprite = backgroundSprite;
						m_targetSprite.color = m_targetColor;
					}
					else
					{
						backgroundSprite.color = m_selectColor;
					}
				}
				else
				{
					backgroundSprite.color = m_deselectColor;
				}

				m_contentIds.Add(Convert.ToInt32(datum["id"]));
			}
		}

		m_contentGrid.GetComponent<UIGrid>().Reposition();

		UpdateSelectAllButton();
	}

	void OnStoryClick(ClickEvent clickBehaviour)
	{
	}

	void OnClickAll(ClickEvent clickBehaviour)
	{
		Color col;

		if(CollectionHelpers.IsSubset(m_contentIds, LessonInfo.Instance.GetDataIds(m_dataType)))
		{
			col = m_deselectColor;

			foreach(int id in m_contentIds)
			{
				LessonInfo.Instance.RemoveData(id, m_dataType);
			}
		}
		else
		{
			col = m_selectColor;

			foreach(int id in m_contentIds)
			{
				LessonInfo.Instance.AddData(id, m_dataType);
			}
		}

		int numContentButtons = m_contentGrid.childCount;
		int targetId = LessonInfo.Instance.GetTargetId(m_dataType);
		for(int i = 0; i < numContentButtons; ++i)
		{
			Transform tra = m_contentGrid.GetChild(i);
			if(targetId != Convert.ToInt32(tra.GetComponent<ClickEvent>().GetData()["id"]))
			{
				m_contentGrid.GetChild(i).GetComponentInChildren<UISprite>().color = col;
			}
		}

		UpdateSelectAllButton();
	}

	void OnClickContent(ClickEvent clickBehaviour)
	{
		string dataState = LessonInfo.Instance.ToggleData(Convert.ToInt32(clickBehaviour.GetData()["id"]), m_dataType);

		UISprite sprite = clickBehaviour.GetComponentInChildren<UISprite>() as UISprite;

		switch(dataState)
		{
		case "Add":
			sprite.color = m_selectColor;
			break;
		case "Remove":
			m_targetSprite = null; // If we are removing it then it was the target sprite
			sprite.color = m_deselectColor;
			break;
		case "Target":
			if(m_targetSprite != null)
			{
				m_targetSprite.color = m_selectColor;
			}

			m_targetSprite = sprite;
			m_targetSprite.color = m_targetColor;
			break;
		}

		UpdateSelectAllButton();
	}

	void LogData()
	{
		List<DataRow> selectedData = LessonInfo.Instance.GetData(m_dataType);

		Debug.Log("LOG DATA");

		DataRow target = LessonInfo.Instance.GetTargetData(m_dataType);

		if(target != null)
		{
			Debug.Log("Target: " + target["phoneme"].ToString());
		}
		else
		{
			Debug.Log("Target: null");
		}

		foreach(DataRow datum in selectedData)
		{
			Debug.Log(datum["phoneme"].ToString());
		}
	}

	void UpdateSelectAllButton()
	{
		if(CollectionHelpers.IsSubset(m_contentIds, LessonInfo.Instance.GetDataIds(m_dataType)))
		{
			m_selectAllButton.GetComponentInChildren<UISprite>().color = m_selectColor;
		}
		else
		{
			m_selectAllButton.GetComponentInChildren<UISprite>().color = m_deselectColor;
		}
	}

	void OnClickSet(ClickEvent clickBehaviour)
	{
		if(m_currentSetSprite != null)
		{
			m_currentSetSprite.color = m_deselectColor;
		}

		m_currentSetSprite = clickBehaviour.GetComponentInChildren<UISprite>() as UISprite;
		m_currentSetSprite.color = m_selectColor;

		int setNum = Convert.ToInt32(clickBehaviour.GetData()["number"]);

		if(m_dataType != LessonInfo.DataType.Stories)
		{
			StartCoroutine(FillContentGrid(setNum));
		}
		else
		{
			SpawnStory();
		}
	}

	public void ChangeDataType(LessonInfo.DataType dataType)
	{
		m_dataType = dataType;

		switch(dataType)
		{
		case LessonInfo.DataType.Letters:
			m_setAttribute = "setphonemes";
			m_contentAttribute = "phonemes";
			m_contentLabelAttribute = "phoneme";
			break;
		case LessonInfo.DataType.Words:
			m_setAttribute = "setwords";
			m_contentAttribute = "words";
			m_contentLabelAttribute = "word";
			break;
		case LessonInfo.DataType.Keywords:
			m_setAttribute = "setkeywords";
			m_contentAttribute = "words";
			m_contentLabelAttribute = "word";
			break;
		case LessonInfo.DataType.Stories:
			m_setAttribute = "setstories";
			m_contentAttribute = "stories";
			m_contentLabelAttribute = "title";
			break;
		}

		StartCoroutine(FillGrids());
	}

	public Color GetSelectColor()
	{
		return m_selectColor;
	}

	public Color GetDeselectColor()
	{
		return m_deselectColor;
	}
}
