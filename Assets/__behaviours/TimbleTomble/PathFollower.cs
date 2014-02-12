using UnityEngine;
using System.Collections;

public abstract class PathFollower : MonoBehaviour 
{
	public Spline Path;
	protected float m_speedModifier = 1;

	protected virtual void Start()
	{
		if(TTCoordinator.Instance != null)
		{
			TTSettings settings = TTCoordinator.Instance.GetSettings();
			if(settings != null)
			{
				m_speedModifier = settings.m_splineSpeedModifier;

				TTCoordinator.Instance.OnSettingsChange += OnSettingsChange;
			}
		}
	}

	void OnDestroy()
	{
		if(TTCoordinator.Instance != null)
		{
			TTCoordinator.Instance.OnSettingsChange -= OnSettingsChange;
		}
	}

	public void SetSpline(Spline path)
	{
		Path = path;
	}

	void OnSettingsChange(TTSettings newSettings)
	{
		m_speedModifier = newSettings.m_splineSpeedModifier;
	}
}
