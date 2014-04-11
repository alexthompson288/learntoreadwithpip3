using UnityEngine;
using System.Collections;
using Wingrove;

public class VoyageMapButton : MonoBehaviour 
{
    [SerializeField]
    private ColorInfo.PipColor m_color;
    [SerializeField]
    private Spline m_spline;
    [SerializeField]
    private Transform m_debugFootprint;
    [SerializeField]
    private float m_debugParam;

    void Start()
    {
        if (m_spline != null)
        {
            //m_debugParam = Mathf.Clamp01(m_debugParam);
            //m_debugFootprint.position = m_spline.GetPositionOnSpline(m_debugParam);

            int numFootprints = 8;

            for(int i = 0; i < numFootprints; ++i)
            {
                /*
                Transform footprint = SpawningHelpers.InstantiateUnderWithIdentityTransforms(VoyageCoordinator.Instance.footprintPrefab, transform).transform;
                footprint.position = m_spline.GetPositionOnSpline((float) i / (numFootprints - 1));

                Vector3 tangent = m_spline.GetTangentToSpline((float) i / (numFootprints - 1));
                footprint.right = tangent;

                Vector3 perp1 = new Vector3(-tangent.y, tangent.x, tangent.z);
                Vector3 perp2 = new Vector3(tangent.y, -tangent.x, tangent.z);

                footprint.position += perp2;
                */
                for(int j = 0; j < 2; ++j)
                {
                    Transform footprint = SpawningHelpers.InstantiateUnderWithIdentityTransforms(VoyageCoordinator.Instance.footprintPrefab, transform).transform;
                    footprint.position = m_spline.GetPositionOnSpline((float) i / (numFootprints - 1));
                    
                    Vector3 tangent = m_spline.GetTangentToSpline((float) i / (numFootprints - 1));
                    
                    Vector3 perperpendicular = j == 0 ? new Vector3(-tangent.y, tangent.x, tangent.z) : new Vector3(tangent.y, -tangent.x, tangent.z);
                    
                    footprint.position += (perperpendicular * 0.25f);

                    //if(j == 1)
                    //{
                        //footprint.position += (tangent * 0.1f);
                    //}

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

	void OnClick()
    {
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color);
    }
}
