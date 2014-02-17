using UnityEngine;
using System.Collections;

public class FlashcardSetButton : MonoBehaviour 
{
	public delegate void SingleClick(bool isUnlocked, int setNum);
	public event SingleClick OnSingleClick;

	//[SerializeField]
	//private GameObject m_unlockedHierarchy;
	[SerializeField]
	private GameObject m_lockedHierarchy;
	[SerializeField]
	private GameObject m_lock;
	[SerializeField]
	private WobbleGUIElement m_wobbleBehaviour;
	[SerializeField]
	private UILabel m_label;
	[SerializeField]
	private UIDragPanelContents m_dragPanelContents;

	bool m_isUnlocked;
	int m_setNum;

	public void SetUp(bool isUnlocked, int setNum, UIDraggablePanel draggablePanel)
	{
		m_dragPanelContents.draggablePanel = draggablePanel;

		m_isUnlocked = isUnlocked;
		m_setNum = setNum;

		m_label.text = System.String.Format("Set {0}", setNum.ToString());

		if(m_isUnlocked)
		{
			//m_lock.SetActive(false);
			m_lockedHierarchy.SetActive(false);
		}
		else
		{
			Destroy(m_wobbleBehaviour);
		}
	}

	public IEnumerator Unlock()
	{
		m_isUnlocked = true;

		m_lockedHierarchy.SetActive(true);
		
		iTween.ScaleTo(gameObject, Vector3.one * 1.2f, 1.0f);
		//Vector3 pos = transform.position;
		//Vector3 tPos = pos * 0.5f;
		//iTween.MoveTo(gameObject, tPos, 1.0f);
		
		foreach (UIWidget widget in GetComponentsInChildren<UIWidget>(true))
		{
			widget.depth += 5;
		}
		
		yield return new WaitForSeconds(1.0f);
		
		CharacterPopper cPop = (CharacterPopper)GameObject.FindObjectOfType(typeof(CharacterPopper));
		if (cPop != null)
		{
			cPop.PopCharacter();
		}
		
		WingroveAudio.WingroveRoot.Instance.PostEvent("SFX_SPARKLE");
		iTween.RotateBy(m_lockedHierarchy, new Vector3(0, 0, Random.Range(-2.0f, 2.0f)), 2.0f);
		yield return new WaitForSeconds(0.1f);
		iTween.MoveTo(m_lockedHierarchy, m_lockedHierarchy.transform.position + (Random.onUnitSphere * 1.5f), 2.0f);
		yield return new WaitForSeconds(0.1f);
		TweenAlpha.Begin(m_lockedHierarchy, 1.0f, 0.0f);
		//m_unlockedHierarchy.SetActive(true);

		iTween.ScaleTo(gameObject, Vector3.one, 1.0f);
		//iTween.MoveTo(gameObject, pos, 1.0f);
		yield return new WaitForSeconds(1.0f);
		
		foreach (UIWidget widget in GetComponentsInChildren<UIWidget>(true))
		{
			widget.depth -= 5;
		}

		m_lockedHierarchy.SetActive(false);
		yield return null;
	}
	
	void OnClick()
	{
		Debug.Log("SetButton.OnClick()");
		if(OnSingleClick != null)
		{
			OnSingleClick(m_isUnlocked, m_setNum);
		}
		else
		{
			Debug.Log("Event was null");
		}
	}

	public int GetSetNum()
	{
		return m_setNum;
	}
}
