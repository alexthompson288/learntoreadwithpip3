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
    private LineRenderer m_totalLine;
    [SerializeField]
    private ClickEvent m_click;

    void Start()
    {
        //yield return StartCoroutine(GameManager.WaitForInstance());

        m_click.Unpressed += UnpressedButton;

        if (m_spline != null)
        {
            Color opaqueCol = ColorInfo.GetColor(m_color);
            Color transparentCol = opaqueCol;
            transparentCol.a = 0.8f;

            int moduleId = DataHelpers.GetModuleId(m_color);
            int numVerticesPerSession = 5;

            int numSessionsComplete = VoyageInfo.Instance.GetNumSessionsComplete(moduleId);
            int numVerticesComplete = numSessionsComplete * numVerticesPerSession;

            DataTable dt = GameDataBridge.Instance.GetDatabase().ExecuteQuery("select * from programsessions WHERE programmodule_id=" + moduleId);
            int numSessionsTotal = dt.Rows.Count;
            int numVerticesTotal = numSessionsTotal * numVerticesPerSession;

            float lineWidth = 0.06f;

            if(m_completeLine != null)
            {
                m_completeLine.SetColors(opaqueCol, opaqueCol);
                m_completeLine.SetWidth(lineWidth, lineWidth);
                m_completeLine.SetVertexCount(numVerticesComplete);

                for(int i = 0; i < numVerticesComplete; ++i)
                {
                    Vector3 pos = m_spline.GetPositionOnSpline((float) i / (numVerticesTotal));
                    pos.z = -0.45f;

                    m_completeLine.SetPosition(i, pos);
                }
            }

            if(m_totalLine != null)
            {
                m_totalLine.SetColors(transparentCol, transparentCol);
                m_totalLine.SetVertexCount(numVerticesTotal);
                m_totalLine.SetWidth(lineWidth, lineWidth);

                for(int i = 0; i < numVerticesTotal; ++i)
                {
                    Vector3 pos = m_spline.GetPositionOnSpline((float) i / (numVerticesTotal));
                    pos.z = -0.25f;
                    
                    m_totalLine.SetPosition(i, pos);
                }
            }
        }
    }

    void UnpressedButton(ClickEvent click)
    {
        ////D.Log("Clicked map: " + m_color + " " + (int)m_color);
        StartCoroutine(UnpressedButtonCo());
    }
    
    IEnumerator UnpressedButtonCo()
    {
        yield return new WaitForSeconds(GetComponentInChildren<PerspectiveButton>().tweenDuration + 0.3f);
        VoyageCoordinator.Instance.MoveToModuleMap((int)m_color, true);
    }
}
