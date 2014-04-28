using UnityEngine;
using System.Collections;

public class LineDraw : MonoBehaviour 
{
    public delegate void LineEvent(LineDraw line);
    public event LineEvent LineCreateEventHandler;
    public event LineEvent LineDragEventHandler;
    public event LineEvent LineReleaseEventHandler;


    [SerializeField]
    private int m_maxNumPositions = -1;
    [SerializeField]
    protected Material m_material;

    public int maxNumPositions
    {
        get
        {
            return m_maxNumPositions;
        }
    }

    UICamera.MouseOrTouch m_input;
    public UICamera.MouseOrTouch input
    {
        get
        {
            return m_input;
        }
    }

    Camera m_camera;
    public Camera camera
    {
        get
        {
            return m_camera;
        }
    }

    protected virtual void OnPress(bool pressed)
    {
        if (pressed) 
        {
            m_input = UICamera.currentTouch;
            m_camera = UICamera.currentCamera;

            // Only create the line if this object is not from a derived class
            // Derived types handle their own line creation because they might want to use a range of different materials
            if(this.GetType().Name == "LineDraw")
            {
                CreateLine(m_material);
            }
        }

        if (!pressed && LineReleaseEventHandler != null)
        {
            LineReleaseEventHandler(this);
        }
    }

    void OnDrag(Vector2 delta)
    {
        if (LineDragEventHandler != null)
        {
            LineDragEventHandler(this);
        }
    }

    protected void CreateLine(Material mat = null)
    {
        LineDrawManager.Instance.CreateLine(this, mat);
        
        if(LineCreateEventHandler != null)
        {
            LineCreateEventHandler(this);
        }
    }

    protected void CreateLine(Material mat, Color startCol, Color endCol)
    {
        LineDrawManager.Instance.CreateLine(this, mat, startCol, endCol);

        if(LineCreateEventHandler != null)
        {
            LineCreateEventHandler(this);
        }
    }
}
