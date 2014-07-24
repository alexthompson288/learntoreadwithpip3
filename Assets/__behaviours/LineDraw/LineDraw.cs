using UnityEngine;
using System.Collections;
using System.Linq;

public class LineDraw : MonoBehaviour 
{
    public delegate void LineEventHandler(LineDraw line);

    //public event LineEventHandler LineCreated;

    private event LineEventHandler LineReleasedPrivate;
    public event LineEventHandler LineReleased
    {
        add
        {
            if(LineReleasedPrivate == null || !LineReleasedPrivate.GetInvocationList().Contains(value))
            {
                LineReleasedPrivate += value;
            }
        }
        remove
        {
            LineReleasedPrivate -= value;
        }
    }

    private event LineEventHandler LineDraggedPrivate;
    public event LineEventHandler LineDragged
    {
        add
        {
            if(LineDraggedPrivate == null || !LineDraggedPrivate.GetInvocationList().Contains(value))
            {
                LineDraggedPrivate += value;
            }
        }
        remove
        {
            LineDraggedPrivate -= value;
        }
    }

    [SerializeField]
    protected Material m_material;
    [SerializeField]
    protected Color m_startColor = Color.white;
    [SerializeField]
    protected Color m_endColor = Color.white;
    [SerializeField]
    private int m_maxNumPositions = -1;

    public void SetMaterial(Material newMaterial)
    {
        m_material = newMaterial;
    }

    public void SetColors(Color newStartColor, Color newEndColor)
    {
        m_startColor = newStartColor;
        m_endColor = newEndColor;
    }

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
                CreateLine();
            }
        }

        if (!pressed && LineReleasedPrivate != null)
        {
            LineReleasedPrivate(this);
        }
    }

    void OnDrag(Vector2 delta)
    {
        if (LineDraggedPrivate != null)
        {
            LineDraggedPrivate(this);
        }
    }

    public void CreateLine()
    {
        LineDrawManager.Instance.CreateLine(this, m_material, m_startColor, m_endColor);

        /*
        if(LineCreated != null)
        {
            LineCreated(this);
        }
        */
    }
}
