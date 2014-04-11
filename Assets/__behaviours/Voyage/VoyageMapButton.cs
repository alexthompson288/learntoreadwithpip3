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

	void OnClick()
    {
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color);
    }
}
