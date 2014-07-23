﻿using UnityEngine;
using System.Collections;
using System;

public class SplineFollower : MonoBehaviour 
{
    [SerializeField]
    private Transform m_follower;
    [SerializeField]
    private bool m_startOn = true;
    [SerializeField]
    private int m_startIndex;
    [SerializeField]
    private float m_startDistance = 0;
    [SerializeField]
    private float m_speedModifier = 1;
    [SerializeField]
    private UIWidget[] m_alphaWidgets;
    [SerializeField]
    private Path[] m_paths;

    int m_pathIndex = 0;

    private float m_currentTime;

    bool m_isOn = false;

    
    public void SetSpeedModifier(float newModifier)
    {
        m_speedModifier = newModifier;
    }

    public enum ForwardDirection
    {
        Constant,
        Left,
        Right,
        Up, 
        Down
    }

    public enum OnCompleteAction
    {
        Restart,
        Stop,
        DestroySelf,
        DestroySelfAndSplines
    }

    [System.Serializable]
    class Path
    {
        [SerializeField]
        public string m_name;
        [SerializeField]
        public Spline m_spline;
        [SerializeField]
        public float m_totalTime = 12.0f;
        [SerializeField]
        public Vector2 m_alphaProportionalDistances = new Vector2(0, 1);
        [SerializeField]
        public ForwardDirection m_forwardDirection;
        [SerializeField]
        public OnCompleteAction m_onCompleteAction;

        public void ClampVar()
        {
            m_alphaProportionalDistances.x = Mathf.Clamp01(m_alphaProportionalDistances.x);
            m_alphaProportionalDistances.y = Mathf.Clamp01(m_alphaProportionalDistances.y);
        }

        public float GetSpeed()
        {
            return m_spline.Length / m_totalTime;
        }
    }
    
    // Use this for initialization
    void Awake () 
    {
        if (m_follower == null)
        {
            m_follower = transform;
        }

        m_startDistance = Mathf.Clamp01(m_startDistance);

        foreach (Path path in m_paths)
        {
            path.ClampVar();
        }

        if (m_paths.Length > 0)
        {
            m_pathIndex = Mathf.Clamp(m_startIndex, 0, m_paths.Length - 1);
            m_currentTime = m_paths[m_pathIndex].m_totalTime * m_startDistance;
        }
    }
    
    void Start ()
    {
        if (m_startOn && m_paths.Length > 0)
        {
            StartCoroutine(On());
        }
    }

    void OnEnable()
    {
        if (m_isOn)
        {
            StartCoroutine(On());
        }
    }

    public void AddSpline(Spline spline, string pathName = "")
    {
        if(m_paths.Length > 0)
        {
            Path path = !String.IsNullOrEmpty(pathName) ? Array.Find(m_paths, x => x.m_name == pathName) : m_paths[0];

            if(path != null)
            {
                path.m_spline = spline;
            }
        }
    }

    public void ChangePath(string pathName)
    {
        int pathIndex = Array.FindIndex(m_paths, x => x.m_name == pathName);

        if (pathIndex != -1 && gameObject != null)
        {
            StopAllCoroutines();
            m_currentTime = 0;
            m_pathIndex = pathIndex;
            StartCoroutine(TweenToNewSpline());
        }
    }
    
    public void ChangePath(int index = -1)
    {
        StopAllCoroutines();
        
        m_currentTime = 0;
        
        if (index == -1)
        {
            ++m_pathIndex;
        } 
        else
        {
            m_pathIndex = index;
        }
        
        if (m_pathIndex > m_paths.Length - 1)
        {
            m_pathIndex = 0;
        }
        else if (m_pathIndex < 0)
        {
            m_pathIndex = m_pathIndex - 1;
        }
        
        StartCoroutine(TweenToNewSpline());
    }

    public IEnumerator TweenToNewSpline()
    {
        Vector3 targetPos = m_paths[m_pathIndex].m_spline.GetPositionOnSpline(0);
        float speed = m_paths [m_pathIndex].GetSpeed();
        
        Hashtable tweenArgs = new Hashtable();
        
        tweenArgs.Add("position", targetPos);
        tweenArgs.Add("easetype", iTween.EaseType.linear);
        tweenArgs.Add("speed", speed);
        
        float tweenDuration = (targetPos - m_follower.position).magnitude / speed;
        
        iTween.MoveTo(m_follower.gameObject, tweenArgs);
        
        yield return new WaitForSeconds(tweenDuration);
        
        StartCoroutine("On");

        yield break;
    }
    
    public IEnumerator On () 
    {
        if (m_pathIndex < m_paths.Length)
        {
            m_isOn = true;

            Path path = m_paths [m_pathIndex];
            Spline spline = path.m_spline;

            m_currentTime += Time.deltaTime * m_speedModifier;
                
            float proportionalDistance = m_currentTime / path.m_totalTime;
                
            m_follower.position = spline.GetPositionOnSpline(proportionalDistance);
                
            if (m_alphaWidgets.Length > 0)
            {
                float widgetAlpha = 1;
                    
                if (proportionalDistance < path.m_alphaProportionalDistances.x)
                {
                    widgetAlpha = proportionalDistance / path.m_alphaProportionalDistances.x;
                } 
                else if (proportionalDistance > path.m_alphaProportionalDistances.y)
                {
                    widgetAlpha = (1 - proportionalDistance) / (1 - path.m_alphaProportionalDistances.y);
                }
                    
                foreach (UIWidget widget in m_alphaWidgets)
                {
                    widget.alpha = widgetAlpha;
                }
            }
           
            Vector3 splineTangent = spline.GetTangentToSpline(m_currentTime / path.m_totalTime);

            switch(path.m_forwardDirection)
            {
                case ForwardDirection.Left:
                    m_follower.right = -splineTangent;
                    break;
                case ForwardDirection.Right:
                    m_follower.right = splineTangent;
                    break;
                case ForwardDirection.Up:
                    m_follower.up = splineTangent;
                    break;
                case ForwardDirection.Down:
                    m_follower.up = -splineTangent;
                    break;
                case ForwardDirection.Constant:
                    break;
            }
                
            if (m_currentTime > path.m_totalTime)
            {
                switch(path.m_onCompleteAction)
                {
                    case OnCompleteAction.Restart:
                        m_currentTime = 0.0f;
                        break;
                    case OnCompleteAction.DestroySelf:
                        Destroy(gameObject);
                        break;
                    case OnCompleteAction.DestroySelfAndSplines:
                        foreach(Path p in m_paths)
                        {
                            Destroy(p.m_spline.gameObject);
                        }
                        Destroy(gameObject);
                        break;
                    case OnCompleteAction.Stop:
                        StopAllCoroutines();
                        break;
                }
            }
        }

        yield return null;
        StartCoroutine("On");
    }

    public void Stop()
    {
        StopAllCoroutines();
        m_isOn = false;
    }
}