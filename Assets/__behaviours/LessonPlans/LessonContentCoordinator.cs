using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;
using System;

public class LessonContentCoordinator : Singleton<LessonContentCoordinator> 
{
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
	[SerializeField]
	ClickEvent m_nextButton;

	LessonInfo.DataType m_dataType = LessonInfo.DataType.Letters;
	string m_setAttribute = "setphonemes";
	string m_contentAttribute = "phonemes";
	string m_contentLabelAttribute = "phoneme";

	List<int> m_contentIds = new List<int>();

	UISprite m_targetSprite = null;

	// Use this for initialization
	IEnumerator Start () 
	{
		m_selectAllButton.OnSingleClick += OnClickAll;
		m_nextButton.OnSingleClick += OnClickNext;

		yield return StartCoroutine(GameDataBridge.WaitForDatabase());

		StartCoroutine(FillGrids());
	}

	void OnClickNext(ClickEvent clickBehaviour)
	{
		CreateLessonCamera.Instance.MoveToMenu(LessonGameCoordinator.Instance.transform);
		Invoke("DisableSelf", CreateLessonCamera.Instance.GetTweenDuration());
	}

	void DisableSelf()
	{
		gameObject.SetActive(false);
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

		yield return null;

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets ORDER BY number");

		int firstSet = -1;
		foreach(DataRow set in dt.Rows)
		{
			if(GameDataBridge.Instance.SetContainsData(set, m_setAttribute, m_contentAttribute))
			{
				GameObject newSetButton = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_setButtonPrefab, m_setGrid);
				newSetButton.GetComponentInChildren<UILabel>().text = String.Format("Set {0}", set["number"].ToString());
				newSetButton.GetComponent<UIDragPanelContents>().draggablePanel = m_setPanel;
				newSetButton.GetComponent<ClickEvent>().SetData(set);
				newSetButton.GetComponent<ClickEvent>().OnSingleClick += OnClickSet;

				if(firstSet == -1)
				{
					firstSet = Convert.ToInt32(set["number"]);
				}
			}

			++firstSet;
		}

		m_setGrid.GetComponent<UIGrid>().Reposition();

		//Debug.Log("firstSet: " + firstSet);
		//StartCoroutine(FillContentGrid(firstSet));

		StartCoroutine(FillContentGrid(1));
	}

	IEnumerator FillContentGrid(int setNum)
	{
		DestroyChildren(m_contentGrid);
		m_contentIds.Clear();
		m_targetSprite = null;

		yield return null;

		DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from phonicssets WHERE number=" + setNum);

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
		int setNum = Convert.ToInt32(clickBehaviour.GetData()["number"]);
		StartCoroutine(FillContentGrid(setNum));
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
}
