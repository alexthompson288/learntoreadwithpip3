using UnityEngine;
using System.Collections;
using Wingrove;

public class Troll : MonoBehaviour 
{
	[SerializeField]
	private Transform m_mouth;
	[SerializeField]
	private SimpleSpriteAnim m_animBehaviour;
	[SerializeField]
	private bool m_randomFart = false;
	[SerializeField]
	private float m_probabilityBurp = 0.25f;

	void Start()
	{
		Mathf.Clamp(m_probabilityBurp, 0f, 1f);

		if(m_randomFart)
		{
			StartCoroutine(RandomFart());
		}
	}

	public float EatFood(GameObject food)
	{
		StartCoroutine(EatFoodCo(food));

		if(Random.Range(0f, 1f) < m_probabilityBurp)
		{
			float burpDelay = 1.5f;
			StartCoroutine(Burp(burpDelay));
			return burpDelay;
		}
		else
		{
			return 0;
		}
	}
	
	public IEnumerator EatFoodCo (GameObject food)
	{
		iTween.MoveTo(food, m_mouth.position, 0.5f);

		yield return new WaitForSeconds(1.2f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_GULP");

		yield return new WaitForSeconds(0.15f);

		iTween.ScaleTo(food, Vector3.zero, 0.2f);


		yield return new WaitForSeconds(0.21f);

		Destroy(food);
	}
	
	public IEnumerator Burp (float delay)
	{
		yield return new WaitForSeconds(delay);

		m_animBehaviour.PlayAnimation("BURP");

		yield return new WaitForSeconds(0.25f);

		WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_RANDOM_BURP");
	}

	public void Fart ()
	{
		WingroveAudio.WingroveRoot.Instance.PostEvent("TROLL_RANDOM_FART");
	}

	IEnumerator RandomFart()
	{
		while(true)
		{
			yield return Random.Range(2, 60);
			Fart ();
		}
	}
}
