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
    [SerializeField]
    private Color m_defaultColor;

    Dictionary<LineDraw, List<DrawRenderer>> m_lines = new Dictionary<LineDraw, List<DrawRenderer>>();

    class DrawRenderer
    {
        LineRenderer m_renderer;
        List<Vector3> m_positions = new List<Vector3>();
        int m_maxNumPositions;
        Color m_startColor = Color.white;
        Color m_endColor = Color.white;

        public DrawRenderer(LineRenderer renderer, Vector3 firstPoint, int maxNumPositions, Color startColor, Color endColor)
        {
            m_renderer = renderer;
            
            m_renderer.SetVertexCount(1);
            m_renderer.SetPosition(0, firstPoint);
            
            m_positions.Add(firstPoint);
            
            m_maxNumPositions = maxNumPositions;
            //m_maxNumPositions = 60;

            m_startColor = startColor;
            m_endColor = endColor;
            
            m_renderer.SetColors(m_startColor, m_endColor);
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

        public IEnumerator Off(float totalFadeTime = 0.25f)
        {
            float remainingFadeTime = totalFadeTime;

            float startColInitialAlpha = m_startColor.a;
            float endColInitialAlpha = m_endColor.a;
      
            while (!Mathf.Approximately(m_startColor.a, 0) || !Mathf.Approximately(m_endColor.a, 0))
            {
                remainingFadeTime = Mathf.Clamp(remainingFadeTime -= Time.deltaTime, 0, remainingFadeTime);

                m_startColor.a = Mathf.Lerp(0, startColInitialAlpha, remainingFadeTime / totalFadeTime);
                m_endColor.a = Mathf.Lerp(0, endColInitialAlpha, remainingFadeTime / totalFadeTime);

                m_renderer.SetColors(m_startColor, m_endColor);

                yield return null;
            }

            Destroy(m_renderer.gameObject);
        }
    }

    public IEnumerator DestroyLineRenderer(LineRenderer lineRenderer, Color color, float totalFadeTime = 0.25f)
    {
        float remainingFadeTime = totalFadeTime;
        
        float colorInitialAlpha = color.a;
        
        while (!Mathf.Approximately(color.a, 0) || !Mathf.Approximately(color.a, 0))
        {
            remainingFadeTime = Mathf.Clamp(remainingFadeTime -= Time.deltaTime, 0, remainingFadeTime);
            
            color.a = Mathf.Lerp(0, colorInitialAlpha, remainingFadeTime / totalFadeTime);

            lineRenderer.SetColors(color, color);
            
            yield return null;
        }
        
        Destroy(lineRenderer.gameObject);
    }
    
    public IEnumerator DrawDemoLine(Vector3 originalPosA, Vector3 originalPosB, Camera cam, Material mat, Color col)
    {
        GameObject newRendererGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_linePrefab, m_lineParent);
        LineRenderer lineRenderer = newRendererGo.GetComponent<LineRenderer>() as LineRenderer;

        lineRenderer.material = mat;
        lineRenderer.SetColors(col, col);

        float updateTime = 0.01f;
        float totalTime = 0.5f;
        float timeElapsed = 0f;

        //Vector3 posA = FindWorldPos(joinableA);

        originalPosA.z = -0.5f;
        originalPosB.z = -0.5f;

        Vector3 screenPosA = cam.WorldToScreenPoint(originalPosA);
        Vector3 worldPosA = m_lineCamera.ScreenToWorldPoint(screenPosA);

        Vector3 screenPosB = cam.WorldToScreenPoint(originalPosB);
        Vector3 worldPosB = m_lineCamera.ScreenToWorldPoint(screenPosB);

        Vector3 nextLinePos = worldPosA;
        Vector3 updateDelta = (worldPosB - worldPosA) * updateTime / totalTime;
        int vertexCount = 1;

        while (timeElapsed < totalTime)
        {
            lineRenderer.SetVertexCount(vertexCount);
            lineRenderer.SetPosition(vertexCount - 1, nextLinePos);

            nextLinePos += updateDelta;
            ++vertexCount;

            timeElapsed += updateTime;
            yield return new WaitForSeconds(updateTime);
        }

        yield return new WaitForSeconds(1f);

        StartCoroutine(DestroyLineRenderer(lineRenderer, col));
    }

    public void CreateLine(LineDraw line, Material mat, Color startColor, Color endColor)
    {
        line.LineDragEventHandler += OnLineDrag;
        
        GameObject newRendererGo = SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_linePrefab, m_lineParent);
        LineRenderer lineRenderer = newRendererGo.GetComponent<LineRenderer>() as LineRenderer;
        
        lineRenderer.material = mat != null ? mat : m_defaultMaterial;

        if (m_lines.ContainsKey(line))
        {
            m_lines[line].Add(new DrawRenderer(lineRenderer, FindWorldPos(line), line.maxNumPositions, startColor, endColor));
            //m_lines [line] = new DrawRenderer(lineRenderer, FindWorldPos(line), line.maxNumPositions, startColor, endColor);
        } 
        else
        {
            List<DrawRenderer> drawRenderers = new List<DrawRenderer>();
            drawRenderers.Add(new DrawRenderer(lineRenderer, FindWorldPos(line), line.maxNumPositions, startColor, endColor));
            m_lines.Add(line, drawRenderers);
            //m_lines.Add(line, new DrawRenderer(lineRenderer, FindWorldPos(line), line.maxNumPositions, startColor, endColor));
        }
    }

    public void CreateLine(LineDraw line, Material mat = null)
    {
        if (mat == null)
        {
            mat = m_defaultMaterial;
        }

        CreateLine(line, mat, m_defaultColor, m_defaultColor);
    }

    DrawRenderer GetLastDrawRenderer(LineDraw line)
    {
        List<DrawRenderer> drawRenderers = m_lines [line];
        return drawRenderers [drawRenderers.Count - 1];
    }

    void OnLineDrag(LineDraw line)
    {
        Vector3 worldPos = FindWorldPos(line);

        if (m_lines.ContainsKey(line))
        {
            //m_lines[line].AddPosition(FindWorldPos(line));
            GetLastDrawRenderer(line).AddPosition(FindWorldPos(line));
        } 
        else
        {
            Debug.LogError("Missing Key: " + line);
        }
    }

    Vector3 FindWorldPos(LineDraw line)
    {
        Vector2 screenPos = line.input.pos;
        return m_lineCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, m_lineParent.position.z));
    }

    public void DestroyLine(LineDraw line)
    {
        List<DrawRenderer> drawRenderers = m_lines [line];
        foreach (DrawRenderer drawRenderer in drawRenderers)
        {
            StartCoroutine(drawRenderer.Off());
        }
        //StartCoroutine(m_lines [line].Off());
        m_lines.Remove(line);
    }
}
