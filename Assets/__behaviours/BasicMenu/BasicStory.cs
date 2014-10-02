using UnityEngine;
using System.Collections;

public class BasicStory : MonoBehaviour 
{
    [SerializeField]
    private UITexture m_storyTexture;
	
    DataRow m_story;
    public DataRow story
    {
        get
        {
            return m_story;
        }
    }

    public void SetUp(DataRow myStory)
    {
        m_story = myStory;
        m_storyTexture.mainTexture = DataHelpers.GetPicture(m_story);
    }
}
