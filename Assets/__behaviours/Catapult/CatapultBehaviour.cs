﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class CatapultBehaviour : Singleton<CatapultBehaviour> 
{
	[SerializeField]
	private GameObject m_cannonBallPrefab;
	[SerializeField]
	private Transform m_ballCentre;
    [SerializeField]
    private Transform m_ballDestroyLocation;
	[SerializeField]
	private Vector2 m_pullRange;
	[SerializeField]
    private Vector2 m_forceRange = new Vector2(1, 2.5f);
    [SerializeField]
    private Transform[] m_multiplayerLocations;
    [SerializeField]
    private float m_ballSpawnDelay = 0.5f;
    [SerializeField]
    private LineRenderer[] m_lineRenderers;
    [SerializeField]
    private Transform[] m_lineOrigins;
    [SerializeField]
    private Transform[] m_debugMarkers;
    [SerializeField]
    private Transform m_lineEndParent;
    [SerializeField]
    private Transform[] m_lineEnds;
    [SerializeField]
    private bool m_lineRendererUseWorld;
    [SerializeField]
    private GameObject m_moveable;
    [SerializeField]
    private Transform m_moveableOffLocation;
    [SerializeField]
    private Transform m_ballLaunchParent;


	List<CatapultAmmo> m_spawnedBalls = new List<CatapultAmmo>();

    CatapultAmmo m_currentBall = null;

	void Awake()
	{
		m_pullRange.x = Mathf.Clamp (m_pullRange.x, 0, m_pullRange.x);
		m_forceRange.x = Mathf.Clamp (m_forceRange.x, 0, m_forceRange.x);

        foreach (LineRenderer renderer in m_lineRenderers)
        {
            renderer.useWorldSpace = m_lineRendererUseWorld;
            renderer.SetVertexCount(2);
        }

        MoveLineOrigins();
        SetLineRenderersPos(m_ballCentre.position);

        SpawnBall();
	}

    void MoveLineOrigins()
    {
        for (int i = 0; i < m_lineRenderers.Length && i < m_lineOrigins.Length; ++i)
        {
            Vector3 pos = m_lineRendererUseWorld ? m_lineOrigins[i].transform.position : m_lineOrigins[i].transform.localPosition;
            pos.z = m_lineRendererUseWorld ? -0.2f : -50;
            m_lineRenderers[i].SetPosition(0, pos);
        }
    }

    public void RemoveBall(CatapultAmmo ball)
    {
        if (m_spawnedBalls.Contains(ball))
        {
            m_spawnedBalls.Remove(ball);
        }
    }

    void Update()
    {
        List<CatapultAmmo> ballsToDestroy = m_spawnedBalls.FindAll(IsBallBelowThreshold);
        foreach (CatapultAmmo ball in ballsToDestroy)
        {
            m_spawnedBalls.Remove(ball);
            Destroy(ball.gameObject);
        }

        if(m_currentBall != null)
        {
            m_lineEndParent.position = m_currentBall.FindOppositePosition(m_lineEndParent);
        }

        SetLineRenderersPos(m_lineEndParent.position);

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Off();
        }
#endif
    }

    void SetLineRenderersPos(Vector3 pos)
    {
        for(int i = 0; i < m_lineEnds.Length && i < m_lineRenderers.Length; ++i)
        {
            m_lineRenderers[i].SetPosition(1, m_lineEnds[i].position);
            //m_lineRenderers[i].SetPosition(1, pos);   
        }
    }

    public void MoveToMultiplayerLocation(int index)
    {
        if (index < m_multiplayerLocations.Length)
        {
            m_ballCentre.transform.localPosition = m_multiplayerLocations[index].transform.localPosition;
        }

        MoveLineOrigins();
    }

	public void OnBallRelease(CatapultAmmo ball)
	{
		if ((m_ballCentre.position - ball.rigidbody.transform.position).magnitude < m_pullRange.x) 
		{
			iTween.MoveTo (ball.rigidbody.gameObject, m_ballCentre.position, 0.3f);
		} 
		else 
		{
            ////////D.Log("LAUNCH");
            ball.transform.parent = m_ballLaunchParent;

            ball.OnLaunch();
            //StartCoroutine(ball.CheckForExit(m_ballCentre, m_pullRange.y * 1.01f));
            ResetLineRendererPos();

            WingroveAudio.WingroveRoot.Instance.PostEvent("CANNON_PLACEHOLDER_LAUNCH");

            Vector3 delta = m_ballCentre.position - ball.rigidbody.transform.position;
            
            float distance = delta.magnitude;
            float proportionalDistance = (distance - m_pullRange.x) / (m_pullRange.y - m_pullRange.x);
            
            Vector3 direction = delta.normalized;
            
            Vector3 force = Vector3.Lerp (direction * m_forceRange.x, direction * m_forceRange.y, proportionalDistance);

            ball.rigidbody.AddForce (force, ForceMode.VelocityChange); // ForceMode.VelocityChange makes the application of force independent of object mass
		}
	}

    public void ResetLineRendererPos()
    {
        m_currentBall = null;

        ////////D.Log("ResetLineRendererPos");
        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_ballCentre);
        tweenArgs.Add("speed", 3);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        tweenArgs.Add("oncomplete", "SpawnBall");
        tweenArgs.Add("oncompletetarget", gameObject);
        
        iTween.MoveTo(m_lineEndParent.gameObject, tweenArgs);
    }

    void SpawnBall()
    {
        ////////D.Log("SpawnBall");
        GameObject newBall = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_cannonBallPrefab, m_ballCentre);
        newBall.GetComponent<CatapultAmmo>().SetUp(this);
        m_spawnedBalls.Add(newBall.GetComponent<CatapultAmmo>() as CatapultAmmo);
        
        m_currentBall = newBall.GetComponent<CatapultAmmo>() as CatapultAmmo;
    }

    public void Off()
    {
        if (m_currentBall != null)
        {
            m_currentBall.StopAllCoroutines();
            Destroy(m_currentBall.gameObject);
        }

        Color lineColor = new Color(0, 0, 0, 0);
        for (int i = 0; i < m_lineRenderers.Length; ++i)
        {
            m_lineRenderers[i].SetColors(lineColor,lineColor);
        }

        Hashtable tweenArgs = new Hashtable();
        tweenArgs.Add("position", m_moveableOffLocation);
        tweenArgs.Add("time", 0.5f);
        tweenArgs.Add("easetype", iTween.EaseType.linear);

        WingroveAudio.WingroveRoot.Instance.PostEvent("BLACKBOARD_DISAPPEAR");
        iTween.MoveTo(m_moveable, tweenArgs);
    }


    bool IsBallBelowThreshold(CatapultAmmo ball)
    {
        return ball.transform.position.y < m_ballDestroyLocation.position.y;
    }

    public void OnBallDestroy(CatapultAmmo ball)
    {
        m_spawnedBalls.Remove(ball);
    }

    public Vector3 ballCentrePos
    {
        get
        {
            return m_ballCentre.position;
        }
    }
    
    public float maxPull
    {
        get
        {
            return m_pullRange.y;
        }
    }
    
    public float minPull
    {
        get
        {
            return m_pullRange.x;
        }
    }
}
