using UnityEngine;
using System.Collections;

public class SplineFollower : MonoBehaviour 
{
    [SerializeField]
    private Spline Path;
    [SerializeField]
    private float m_speedModifier = 1;
    [SerializeField]
    private Vector3 m_RotationModifier = Vector3.zero;

    //public Spline Path;
    public float TotalTime = 20.0f;
    private float CurrTime = 0.0f;

    //Vector3 m_startPosition;

    // Use this for initialization
    void Awake () 
    {
        CurrTime = 0;
    }

    // Update is called once per frame
    void Update () 
    {
        if(Path != null)
        {
            CurrTime += Time.deltaTime * m_speedModifier;
            transform.position = Path.GetPositionOnSpline(CurrTime / TotalTime);
            transform.right = -Path.GetTangentToSpline(CurrTime / TotalTime);
            
            if(CurrTime > TotalTime)
            {
                CurrTime = 0.0f;
            }
        }
    }
}
