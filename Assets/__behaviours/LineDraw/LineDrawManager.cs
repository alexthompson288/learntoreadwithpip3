using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LineDrawManager : Singleton<LineDrawManager> 
{
    [SerializeField]
    private Camera m_lineCamera;
    [SerializeField]
    private GameObject m_linePrefab;
    [SerializeField]
    private Transform m_lineParent;
    [SerializeField]
    private Material m_defaultMaterial;

    Dictionary<LineDraw, LineRenderer> m_lines = new Dictionary<LineDraw, LineRenderer>();

    public void CreateLine(LineDraw line)
    {
        CreateLine(line, m_defaultMaterial);
    }

    public void CreateLine(LineDraw line, Material mat)
    {
        line.LineDragEventHandler += OnLineDrag;

        GameObject newRendererGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_linePrefab, m_lineParent);
        LineRenderer lineRenderer = newRendererGo.GetComponent<LineRenderer>() as LineRenderer;
        
        m_lines.Add(line, lineRenderer);

        lineRenderer.material = mat;

        // Set the first point of the renderer
        Vector2 screenPos = line.input.pos;
        Debug.Log("screenPos: " + screenPos);

        Vector3 worldPos = m_lineCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, transform.position.z));
        Debug.Log("worldPos: " + worldPos);

        // Debug: Set the 2nd point, a little to the right of the first point
    }

    void OnLineDrag(LineDraw line, Vector2 delta)
    {

    }

    public void DestroyLine(LineDraw line)
    {

    }
}
