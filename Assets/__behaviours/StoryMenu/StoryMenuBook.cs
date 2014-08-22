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

    void OnPress(bool isDown)
    {
        if (!isDown && Clicked != null)
        {
            Clicked(this);
        }
    }
}
