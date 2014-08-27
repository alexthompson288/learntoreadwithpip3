using UnityEngine;
using System.Collections;

public class TestSpriteAnim : MonoBehaviour 
{
    [SerializeField]
    private SpriteAnim m_anim;

    bool m_waitForFinish = true;

	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_waitForFinish = !m_waitForFinish;
            //////D.Log("TestSpriteAnim.waitForFinish:  " + m_waitForFinish);
        }

	    if (Input.GetKeyDown(KeyCode.J))
        {
            m_anim.PlayAnimation("JUMP", m_waitForFinish);
        } 
        else if (Input.GetKeyDown(KeyCode.T))
        {
            m_anim.PlayAnimation("THUMBS_UP", m_waitForFinish);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            m_anim.PlayAnimation("BLINK", m_waitForFinish);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            m_anim.PlayAnimation("GIGGLE", m_waitForFinish);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            m_anim.PlayAnimation("WALK", m_waitForFinish);
        }
	}

    void OnGUI()
    {
        GUILayout.Label("waitForFinish: " + m_waitForFinish);
    }
}
