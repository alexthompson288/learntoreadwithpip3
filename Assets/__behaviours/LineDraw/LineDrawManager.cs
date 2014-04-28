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

    //Dictionary<LineDraw, LineRenderer> m_lines = new Dictionary<LineDraw, LineRenderer>();
    Dictionary<LineDraw, DrawRenderer> m_lines = new Dictionary<LineDraw, DrawRenderer>();

    class DrawRenderer
    {
        LineRenderer m_renderer;
        List<Vector3> m_positions = new List<Vector3>();
        int m_maxNumPositions;

        public DrawRenderer (LineRenderer renderer, Vector3 firstPoint, int maxNumPositions)
        {
            m_renderer = renderer;
            m_renderer.SetVertexCount(2);
            m_renderer.SetPosition(0, firstPoint);
            m_positions.Add(firstPoint);

            //m_renderer.SetPosition(1, firstPoint + Vector3.right);
            //m_positions.Add(firstPoint + Vector3.right);

            m_maxNumPositions = maxNumPositions;
        }

        public void AddPosition(Vector3 newPosition)
        {
            m_positions.Add(newPosition);

            bool hasRemovedPositions = false;

            while (m_maxNumPositions > 0 && m_positions.Count > m_maxNumPositions)
            {
                hasRemovedPositions = true;
                m_positions.RemoveAt(0);
            }

            m_renderer.SetVertexCount(m_positions.Count);

            if (hasRemovedPositions)
            {
                for(int i = 0; i < m_positions.Count; ++i)
                {
                    m_renderer.SetPosition(i, m_positions[i]);
                }
            } 
            else
            {
                int index = m_positions.Count - 1;
                m_renderer.SetPosition(index, m_positions[index]);
            }
        }

        public void DestroyRenderer()
        {
            Destroy(m_renderer.gameObject);
        }
    }

    public void CreateLine(LineDraw line)
    {
        CreateLine(line, m_defaultMaterial);
    }

    public void CreateLine(LineDraw line, Material mat)
    {
        line.LineDragEventHandler += OnLineDrag;

        GameObject newRendererGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_linePrefab, m_lineParent);
        LineRenderer lineRenderer = newRendererGo.GetComponent<LineRenderer>() as LineRenderer;

        lineRenderer.material = mat;

        m_lines[line] = new DrawRenderer(lineRenderer, FindWorldPos(line), line.maxNumPositions);
    }

    void OnLineDrag(LineDraw line)
    {
        Vector3 worldPos = FindWorldPos(line);

        if (m_lines.ContainsKey(line))
        {
            m_lines [line].AddPosition(FindWorldPos(line));
        } 
        else
        {
            Debug.LogError("Missing Key: " + line);
        }
    }

    Vector3 FindWorldPos(LineDraw line)
    {
        Vector2 screenPos = line.input.pos;

        //Debug.Log("screenPos: " + screenPos);

        return m_lineCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, m_lineParent.position.z));
    }

    public void DestroyLine(LineDraw line)
    {
        m_lines [line].DestroyRenderer();
        m_lines.Remove(line);
    }
}
