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
    private LineRenderer m_completeLine;
    [SerializeField]
    private LineRenderer m_incompleteLine;
    [SerializeField]
    private ClickEvent m_click;

    void Start()
    {
        m_click.OnSingleClick += OnSingleClick;

        if (m_spline != null)
        {
            Color opaqueCol = ColorInfo.GetColor(m_color);
            Color transparentCol = opaqueCol;
            transparentCol.a = 0.5f;

            int numSessionsComplete = VoyageInfo.Instance.GetNumSessionsComplete((int)m_color);

            if(m_color == ColorInfo.PipColor.Pink)
            {
                numSessionsComplete = 8;
            }
            else if(m_color == ColorInfo.PipColor.Red)
            {
                numSessionsComplete = 16;
            }
            else if(m_color == ColorInfo.PipColor.Yellow)
            {
                numSessionsComplete = 12;
            }

            numSessionsComplete = 10;


            int numVerticesPerSession = 5;

            int numCompleteVertices = numSessionsComplete * numVerticesPerSession;
            int numIncompleteVertices = (VoyageInfo.sessionsPerModule - numSessionsComplete) * numVerticesPerSession;
            int numVertices = numCompleteVertices + numIncompleteVertices;

            int splineIndex = 0;
            if(m_completeLine != null)
            {
                m_completeLine.SetColors(opaqueCol, opaqueCol);
                m_completeLine.SetWidth(0.06f, 0.06f);
                m_completeLine.SetVertexCount(numCompleteVertices);

                for(; splineIndex < numCompleteVertices; ++splineIndex)
                {
                    //Vector3 pos = m_spline.GetPositionOnSpline((float) splineIndex / (numVertices - 1));
                    Vector3 pos = m_spline.GetPositionOnSpline((float) splineIndex / (numVertices));
                    pos.z = -0.25f;

                    m_completeLine.SetPosition(splineIndex, pos);
                }
            }


            if(m_incompleteLine != null)
            {
                //float greyLightness = 0.7f;
                //m_incompleteLine.SetColors(new Color(greyLightness, greyLightness, greyLightness, 1f), new Color(greyLightness, greyLightness, greyLightness, 1f));
                m_incompleteLine.SetColors(transparentCol, transparentCol);
                m_incompleteLine.SetVertexCount(numIncompleteVertices);
                m_incompleteLine.SetWidth(0.015f, 0.015f);

                for(int i = 0; i < numIncompleteVertices; ++i, ++splineIndex)
                {
                    Vector3 pos = m_spline.GetPositionOnSpline((float) splineIndex / (numVertices));
                    pos.z = -0.25f;
                    
                    m_incompleteLine.SetPosition(i, pos);
                }
            }
        }
    }

    void OnSingleClick(ClickEvent click)
    {
        Debug.Log("Clicked map: " + m_color + " " + (int)m_color);
        StartCoroutine(OnSingleClickCo());
    }
    
    IEnumerator OnSingleClickCo()
    {
        yield return new WaitForSeconds(GetComponentInChildren<PerspectiveButton>().tweenDuration + 0.3f);
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color);
    }

    /*
    void Start()
    {
        if (m_spline != null)
        {
            int numFootprints = VoyageInfo.Instance.GetNumSessionsComplete((int)m_color);

            if(m_color == ColorInfo.PipColor.Pink)
            {
                numFootprints = 8;
            }
            else if(m_color == ColorInfo.PipColor.Red)
            {
                numFootprints = 16;
            }

            numFootprints = 8;

            int numPairs = Mathf.CeilToInt((float)numFootprints / 2f); // Round up numPairs if numFootprints is odd
            int maxNumPairs = Mathf.CeilToInt((float)VoyageInfo.sessionsPerModule / 2); // Round up maxNumPairs if max number of footprints is odd

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
    */

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
}
