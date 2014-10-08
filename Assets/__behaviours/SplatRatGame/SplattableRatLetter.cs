using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplattableRatLetter : MonoBehaviour 
{
	[SerializeField]
    private UILabel m_letterLabel;
	[SerializeField]
    private float m_minSpeed;
    [SerializeField]
    private float m_maxSpeed;
	[SerializeField]
	private float m_gravity;
    [SerializeField]
    private UISprite m_sprite;
	[SerializeField]
	private SimpleSpriteAnim m_spriteAnimation;
	[SerializeField]
	private float m_splatDuration;

    private GameObject m_locator;
	
	SplatRatGamePlayer m_gamePlayer;
	
	string m_letter;
	
	List<DataRow> m_lettersPool = new List<DataRow>();

    public void SetUp(SplatRatGamePlayer gamePlayer, GameObject locator, List<DataRow> lettersPool)
	{
		m_gamePlayer = gamePlayer;
		m_locator = locator;
		m_lettersPool = lettersPool;
		
		ChangeLetter();
		
        iTween.ScaleFrom(gameObject, Vector3.zero, 1.0f);
	}
	
	void ChangeLetter()
	{
		string currentLetter = SplatRatGameCoordinator.Instance.GetCurrentLetter();
		m_letter = currentLetter;

		if(m_lettersPool.Count > 1)
		{
			int random = Random.Range(0, 101);
			if(random > SplatRatGameCoordinator.Instance.GetPercentageProbabilityLetterIsTarget())
			{
				while(m_letter == currentLetter)
				{
					m_letter = m_lettersPool[Random.Range(0, m_lettersPool.Count)]["phoneme"].ToString();
				}
			}
		}

		m_letterLabel.text = m_letter;
	}
	
	void OnClick()
    {
		StartCoroutine(HitEffect());

        //UserStats.Activity.IncrementNumAnswers();
		
        bool isCorrect = m_gamePlayer.LetterClicked(m_letter);
		
		if(isCorrect)
		{
			StartCoroutine(Splat());
		}
		else
		{
            SplatRatGameCoordinator.Instance.PostOnIncorrect();
			WingroveAudio.WingroveRoot.Instance.PostEvent("GAWP_HAPPY");
		}
    }
	
	void ResetSplattable()
    {
		iTween.ScaleTo(gameObject, Vector3.one, 1.0f);
        transform.position = m_locator.transform.position;
		collider.enabled = true;
		rigidbody.velocity = Vector3.zero;
		ChangeLetter();
    }

    IEnumerator HitEffect()
    {
		collider.enabled = false;
        //m_spriteAnimation.PlayAnimation("HIT");
        iTween.ShakePosition(gameObject, Vector3.one * 0.01f, m_splatDuration);
		
		yield return new WaitForSeconds(m_splatDuration);
		
		//m_spriteAnimation.PlayAnimation("JUMP");
		collider.enabled = true;
    }

    IEnumerator Splat()
    {     
		yield return new WaitForSeconds(m_splatDuration);
		WingroveAudio.WingroveRoot.Instance.PostEvent("GAWP_SQUEAL");
		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_APPEAR");
		iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
		yield return new WaitForSeconds(1.0f);
		ResetSplattable();
    }
	
	public IEnumerator DestroySplattable()
	{
		StopAllCoroutines();
		
		StartCoroutine(HitEffect());
		StartCoroutine(Splat());
		
		yield return new WaitForSeconds(m_splatDuration * 2);
		Destroy(gameObject);
	}

    void FixedUpdate()
    {
		rigidbody.AddForce(-transform.up * m_gravity);
    }
	
	void OnCollisionEnter(Collision other)
	{
        D.Log("Hit: " + other.gameObject.name);

        if(other.collider.CompareTag("Floor"))
		{
			ChangeLetter();
			rigidbody.AddForce(-rigidbody.velocity + transform.up * Random.Range(m_minSpeed, m_maxSpeed), ForceMode.Impulse);
			WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEAR");
		}
	}
}
