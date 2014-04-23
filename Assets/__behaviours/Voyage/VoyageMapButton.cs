using UnityEngine;
using System.Collections;
using Wingrove;

public class VoyageMapButton : MonoBehaviour 
{
    [SerializeField]
    private ColorInfo.PipColor m_color;
    [SerializeField]
    private Spline m_spline;

    void Start()
    {
        if (m_spline != null)
        {
            int numFootprints = VoyageInfo.Instance.GetNumSessionsComplete((int)m_color);
            //int numFootprints = 16;

            int numPairs = Mathf.CeilToInt((float)numFootprints / 2f); // Round up numPairs if numFootprints is odd
            int maxNumPairs = Mathf.CeilToInt((float)VoyageInfo.sessionsInModule / 2); // Round up maxNumPairs if max number of footprints is odd

            float offsetDistance = 0.2f;
            
            for(int i = 0; i < numPairs; ++i)
            {
                int numInPair = (i < numPairs - 1) || (numFootprints % 2 == 0) ? 2 : 1;

                for(int j = 0; j < numInPair; ++j)
                {
                    Transform footprint = SpawningHelpers.InstantiateUnderWithIdentityTransforms(VoyageCoordinator.Instance.footprintPrefab, transform).transform;
                    footprint.position = m_spline.GetPositionOnSpline((float) i / (maxNumPairs - 1));
                    
                    Vector3 tangent = m_spline.GetTangentToSpline((float) i / (maxNumPairs - 1));
                    
                    Vector3 perperpendicular = j == 0 ? new Vector3(-tangent.y, tangent.x, tangent.z) : new Vector3(tangent.y, -tangent.x, tangent.z);
                    
                    footprint.position += (perperpendicular * offsetDistance);
                    
                    Vector3 offset = tangent * offsetDistance;
                    if(j == 0)
                    {
                        offset *= -1;
                    }
                    
                    footprint.position += offset;
                    
                    footprint.right = tangent;
                }
            }
        }
    }

    /*
    void Start()
    {
        if (m_spline != null)
        {
            int numFootprints = 8;

            for(int i = 0; i < numFootprints; ++i)
            {
                for(int j = 0; j < 2; ++j)
                {
                    Transform footprint = SpawningHelpers.InstantiateUnderWithIdentityTransforms(VoyageCoordinator.Instance.footprintPrefab, transform).transform;
                    footprint.position = m_spline.GetPositionOnSpline((float) i / (numFootprints - 1));
                    
                    Vector3 tangent = m_spline.GetTangentToSpline((float) i / (numFootprints - 1));
                    
                    Vector3 perperpendicular = j == 0 ? new Vector3(-tangent.y, tangent.x, tangent.z) : new Vector3(tangent.y, -tangent.x, tangent.z);
                    
                    footprint.position += (perperpendicular * 0.25f);

                    Vector3 offset = tangent * 0.25f;
                    if(j == 0)
                    {
                        offset *= -1;
                    }

                    footprint.position += offset;

                    footprint.right = tangent;
                }
            }
        }
    }
    */

	void OnClick()
    {
        Debug.Log("Clicked map: " + m_color + " - " + (int)m_color);
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color);
    }
}
