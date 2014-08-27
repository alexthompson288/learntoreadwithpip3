using UnityEngine;
using System.Collections;

public class StoryMenuBook : MonoBehaviour 
{
    public delegate void BookEventHandler(StoryMenuBook book);
    public event BookEventHandler Clicked;

    [SerializeField]
    private UITexture m_storyPicture;
 
    DataRow m_data;

    public DataRow GetData()
    {
        return m_data;
    }

    public void SetUp(DataRow dataRow)
    {
        m_data = dataRow;
        m_storyPicture.mainTexture = DataHelpers.GetPicture(m_data);
    }

    public IEnumerator Off()
    {
        float tweenDuration = 0.25f;
        iTween.ScaleTo(gameObject, Vector3.zero, tweenDuration);
        
        yield return new WaitForSeconds(tweenDuration);
        
        Destroy(gameObject);
    }

    float m_horizontalMagnitude;
    float m_horizontalThreshold = 10;

    void OnDrag(Vector2 delta)
    {
        m_horizontalMagnitude += Mathf.Abs(delta.x);
    }

    void OnPress(bool isDown)
    {
        if (isDown)
        {
            m_horizontalMagnitude = 0;
        }
        if (!isDown && m_horizontalMagnitude < m_horizontalThreshold && Clicked != null)
        {
            Clicked(this);
        }
    }
}
