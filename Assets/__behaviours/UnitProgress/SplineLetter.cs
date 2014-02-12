using UnityEngine;
using System.Collections;

public class SplineLetter : MonoBehaviour 
{
	[SerializeField]
	UITexture m_texture;
	[SerializeField]
	Texture2D[] m_letterTextures;
	[SerializeField]
	private float m_minSpeed;
	[SerializeField]
	private float m_maxSpeed;
	[SerializeField]
	private UILabel m_label;

	public void SetUp(DataRow data, bool isMnemonic)
	{
		MoveInNewDirection();

		if(isMnemonic)
		{
			m_label.gameObject.SetActive(false);

			string imageFilename =
				string.Format("Images/mnemonics_images_png_250/{0}_{1}",
				              data["phoneme"],
				              data["mneumonic"].ToString().Replace(" ", "_"));
			
			
			Texture2D newTex = (Texture2D)Resources.Load<Texture2D>(imageFilename);

			if(newTex != null)
			{
				m_texture.mainTexture = newTex;
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
		else
		{
			m_texture.gameObject.SetActive(false);
			m_label.text = data["phoneme"].ToString();
		}
	}

	void OnCollisionEnter(Collision other)
	{
		MoveInNewDirection();
	}

	IEnumerator Start()
	{
		while(true)
		{
			yield return new WaitForSeconds(Random.Range(3.0f, 9.0f));

			MoveInNewDirection();
		}
	}

	void Update()
	{
		if(rigidbody.velocity.magnitude < m_minSpeed)
		{
			MoveInNewDirection();
		}
	}
		
	void MoveInNewDirection()
	{
		Vector3 newDir = new Vector3(Random.Range(-1.1f, 1.1f), Random.Range(-1.1f, 1.1f), 0);
		newDir.Normalize();
		newDir *= Random.Range(m_minSpeed, m_maxSpeed);
		rigidbody.velocity = newDir;
	}
}
