using UnityEngine;
using System.Collections;

public class MovingWidget : MonoBehaviour 
{
	public delegate void WidgetClick (MovingWidget behaviour);
	public event WidgetClick OnWidgetClick;

	public delegate void TargetReach(MovingWidget behaviour);
	public event TargetReach OnTargetReach;

	[SerializeField]
	private Vector3 m_direction;
	[SerializeField]
	private float m_speed;
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private UISprite m_background;
	[SerializeField]
	private Game.Data m_dataType;

	Vector3 m_targetPos;
	
	DataRow m_data = null;

	public string m_debbugingWord;

	Vector3 m_thresholdPos;
	
	public void SetUp (DataRow data) 
	{
		m_data = data;

		string labelData = "word";
		if(m_dataType == Game.Data.Phonemes)
		{
			labelData = "phoneme";	
		}

		if(data != null)
		{
			m_label.text = m_data[labelData].ToString();
			m_debbugingWord = m_data["word"].ToString();

			if(m_debbugingWord == null || m_debbugingWord == "" || m_debbugingWord == " ")
			{
				Debug.LogError("EMPTY DATA!");
				Debug.Log("data: " + m_data);
				Debug.Log("id: " + m_data["id"].ToString());
				Debug.Log("word: " + m_data["word"].ToString());
			}
		}
		else
		{
			Debug.LogError("NO DATA!");
		}
	}

	void Update()
	{
		transform.position = transform.position + (m_direction.normalized * m_speed);

		float dir = 0;
		float delta = 1;

		if(!Mathf.Approximately(m_direction.x, 0))
		{
			dir = m_direction.x;
			delta = transform.position.x - m_thresholdPos.x;
		}
		else if(!Mathf.Approximately(m_direction.y, 0))
		{
			dir = m_direction.y;
			delta = transform.position.y - m_thresholdPos.y;
		}
		else
		{
			return;
		}

		if(Mathf.Approximately((dir / Mathf.Abs(dir)), (delta / Mathf.Abs(delta))))
		{
			if(OnTargetReach != null)
			{
				OnTargetReach(this);
			}
		}
	}

	public void SetMoveThreshold(Vector3 thresholdPos)
	{
		m_thresholdPos = thresholdPos;
	}

	public void SetSpeed(float newSpeed)
	{
		m_speed = newSpeed;
	}

	void OnClick()
	{
		if(OnWidgetClick != null)
		{
			OnWidgetClick(this);
		}
	}

	public DataRow GetData()
	{
		return m_data;
	}

	void OnDestroy()
	{
		StopAllCoroutines();
	}
}
