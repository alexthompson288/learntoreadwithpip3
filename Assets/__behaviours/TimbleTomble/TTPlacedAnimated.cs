using UnityEngine;
using System.Collections;
using Wingrove;

public class TTPlacedAnimated : MonoBehaviour 
{
	[SerializeField]
	private GameObject[] m_pathPrefabs;
	[SerializeField]
	private SimpleSpriteAnim m_simpleSpriteAnim;
	[SerializeField]
	private string[] m_animationNames;
	[SerializeField]
	private PathFollower m_pathFollower;

	private Transform m_popCutoff;

	private GameObject m_path = null;
	
	public void SetUp(Transform popCutoff)
	{
		m_popCutoff = popCutoff;
	}
	
	// Use this for initialization
	IEnumerator Start () 
	{
		m_simpleSpriteAnim.PlayAnimation(m_animationNames[0]);
		m_pathFollower.enabled = false;

		yield return StartCoroutine(TTCoordinator.WaitForSettings());

		m_simpleSpriteAnim.SetAnimFPS(TTCoordinator.Instance.GetSettings().m_animFPS);

		TTCoordinator.Instance.OnSettingsChange += OnSettingsChange;
	}

	void OnDestroy()
	{
		StopAllCoroutines();

		if(TTCoordinator.Instance != null)
		{
			TTCoordinator.Instance.OnSettingsChange -= OnSettingsChange;
		}
	}

	void OnSettingsChange(TTSettings newSettings)
	{
		m_simpleSpriteAnim.SetAnimFPS(newSettings.m_animFPS);
	}

	void OnDrag(Vector2 drag)
	{
		Ray camPos = UICamera.currentCamera.ScreenPointToRay(
			new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
		transform.position = 
			new Vector3(camPos.origin.x,camPos.origin.y,0);
	}

	void OnPress(bool pressed)
	{
		if (!pressed)
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("DROP_STICKER");
			if (transform.position.y < m_popCutoff.position.y)
			{
				TTCoordinator.Instance.DestroyPoppedSprite(gameObject);
			}
			else
			{
				CreatePath();
			}
		}
		else
		{
			WingroveAudio.WingroveRoot.Instance.PostEvent("PICK_UP_STICKER");

			if(m_path != null)
			{
				Destroy(m_path);

				if(m_pathFollower != null)
				{
					m_pathFollower.SetSpline(null);
					m_pathFollower.enabled = false;
				}
			}
		}
	}

	public void CreatePath()
	{
		if(m_path == null && m_pathPrefabs.Length > 0)
		{
			m_path = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_pathPrefabs[Random.Range(0, m_pathPrefabs.Length)], transform);
			m_path.transform.parent = transform.parent;
			m_pathFollower.SetSpline(m_path.GetComponent<Spline>());
			m_pathFollower.enabled = true;
		}
	}
}
