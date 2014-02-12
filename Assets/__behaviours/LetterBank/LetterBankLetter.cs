using UnityEngine;
using System.Collections;
using System;

public class LetterBankLetter : MonoBehaviour {

    [SerializeField]
    private UILabel m_textLabel;
    [SerializeField]
    private UISprite m_sprite;
	[SerializeField]
	private string m_lockedSpriteName;
	[SerializeField]
	private string m_unlockableSpriteName;
    [SerializeField]
    private GameObject m_unlockedHierarchy;
    [SerializeField]
    private Transform[] m_starSprites;
	[SerializeField]
	private WobbleGUIElement m_wobbleBehaviour;

    DataRow m_letterData;
    bool m_isUnlocked;
	
	public enum LockState
	{
		locked,
		unlockable,
		unlocked
	}
	
	LockState lockState;

    public void SetUp(DataRow letterData)
    {
        m_letterData = letterData;
        m_textLabel.text = letterData["phoneme"].ToString();
		
		int[] sectionIds = ((PipGameBuildSettings)SettingsHolder.Instance.GetSettings()).m_difficultyDatabaseIds;
		int sectionId = Convert.ToInt32(letterData["section_id"].ToString());
		int difficulty = Array.IndexOf(sectionIds, sectionId);
		
		// Adjust difficulty because Year1 app has 6 sectionIds. TODO: Don't hard code the values in the adjustment
		if(sectionIds.Length > 3)
		{
			difficulty -= 3;
		}
		
		if(difficulty > SessionInformation.Instance.GetHighestLevelCompletedForApp())
		{
			lockState = LockState.locked;
			m_sprite.spriteName = m_lockedSpriteName;
			//m_sprite.color = new Color(1f, 0f, 0f, 1f); // TODO: Have a better visual indicator when something is locked vs unlockable
		}
		else
		{
			lockState = LockState.unlockable;
			m_sprite.spriteName = m_unlockableSpriteName;
			m_wobbleBehaviour.enabled = true;
		}
		
        Refresh(true);
    }
	
	public void Refresh(bool original)
    {
        if (SessionInformation.Instance.IsLetterUnlocked(m_letterData["phoneme"].ToString()))
        {
            if (lockState != LockState.unlocked)
            {
                if (original)
                {
                    m_sprite.enabled = false;
                    m_unlockedHierarchy.SetActive(true);
                }
                else
                {
                    StartCoroutine(ShowUnlock());
                }
                lockState = LockState.unlocked;
            }
			m_wobbleBehaviour.enabled = false;
        }
    }
	
	/*
    public void Refresh(bool original)
    {
        if (SessionInformation.Instance.IsLetterUnlocked(m_letterData["phoneme"].ToString()))
        {
            if (!m_isUnlocked)
            {
                if (original)
                {
                    m_sprite.enabled = false;
                    m_unlockedHierarchy.SetActive(true);
                }
                else
                {
                    StartCoroutine(ShowUnlock());
                }
                m_isUnlocked = true;
            }
        }
    }
    */

    IEnumerator ShowUnlock()
    {
        // yeah yeah, this isn't a good way to get it :\
        transform.parent.parent.GetComponent<UIDraggablePanel>().enabled = false;

        iTween.ScaleTo(gameObject, Vector3.one * 1.2f, 1.0f);
        Vector3 pos = transform.position;
        Vector3 tPos = pos * 0.5f;
        iTween.MoveTo(gameObject, tPos, 1.0f);

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
        iTween.RotateBy(m_sprite.gameObject, new Vector3(0, 0, UnityEngine.Random.Range(-2.0f, 2.0f)), 2.0f);
        yield return new WaitForSeconds(0.1f);
        iTween.MoveTo(m_sprite.gameObject, m_sprite.transform.position + (UnityEngine.Random.onUnitSphere * 1.5f), 2.0f);
        yield return new WaitForSeconds(0.1f);
        TweenAlpha.Begin(m_sprite.gameObject, 1.0f, 0.0f);
        m_unlockedHierarchy.SetActive(true);
		Vector3 localStarScale = m_starSprites[0].transform.localScale;
        foreach (Transform t in m_starSprites)
        {
            t.localScale = Vector3.zero;
        }
        foreach (Transform t in m_starSprites)
        {
            //iTween.ScaleTo(t.gameObject, Vector3.one * 2.0f, 0.75f);
			iTween.ScaleTo(t.gameObject, localStarScale, 0.75f);
            yield return new WaitForSeconds(0.3f);
        }
        iTween.ScaleTo(gameObject, Vector3.one, 1.0f);
        iTween.MoveTo(gameObject, pos, 1.0f);
        yield return new WaitForSeconds(1.0f);

        foreach (UIWidget widget in GetComponentsInChildren<UIWidget>(true))
        {
            widget.depth -= 5;
        }

        // yeah yeah, this isn't a good way to get it :\
        transform.parent.parent.GetComponent<UIDraggablePanel>().enabled = true;
        m_sprite.enabled = false;
        yield break;
    }
	
	void OnClick()
    {
        if (lockState == LockState.unlockable && SessionInformation.Instance.GetCoins() > 0)
        {
			GoldCoinBar.Instance.SetCurrentLetterPosition(transform.position);
            StartCoroutine(LetterBankGrid.Instance.ShowThreeLetters(m_letterData));
        }
		else
		{
			StartCoroutine(LetterBankGrid.Instance.ShowOneLetter(m_letterData));
		}
    }
	
	public DataRow GetLetterData()
	{
		return m_letterData;
	}
	
	/*
    void OnClick()
    {
        string letterAsString = m_letterData["phoneme"].ToString();
        //PipPadBehaviour.Instance.Show(letterAsString);
        if (!SessionInformation.Instance.IsLetterUnlocked(letterAsString))
        {
            StartCoroutine(LetterBankGrid.Instance.ShowThreeLetters(m_letterData));
        }
		else
		{
			StartCoroutine(LetterBankGrid.Instance.ShowOneLetter(m_letterData));
		}
    }
    */
}
