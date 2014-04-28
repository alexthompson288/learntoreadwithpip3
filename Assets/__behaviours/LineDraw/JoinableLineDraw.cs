using UnityEngine;
using System.Collections;

public class JoinableLineDraw : LineDraw 
{
    public delegate void JoinableEvent(JoinableLineDraw self, JoinableLineDraw other);
    public event JoinableEvent JoinableReleaseEventHandler;

    [SerializeField]
    private bool m_isPicture;
    [SerializeField]
    private UITexture m_pictureTexture;
    [SerializeField]
    private UILabel m_label;

    public bool isPicture
    {
        get
        {
            return m_isPicture;
        }
    }

    DataRow m_data;
    public DataRow data
    {
        get
        {
            return m_data;
        }
    }

    string m_word;
    public string word
    {
        get
        {
            return m_word;
        }
    }

    void Start()
    {
        iTween.ScaleFrom(gameObject, Vector3.zero, 0.3f);
    }

    public void SetUp(DataRow myData, string myWord)
    {
        m_data = myData;
        m_word = myWord;

        if (m_isPicture)
        {
            Texture2D wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + word);
            m_pictureTexture.mainTexture = wordImage;
        }
        else
        {
            m_label.text = word;
        }
    }

    protected override void OnPress(bool pressed)
    {
        base.OnPress(pressed);

        if (pressed)
        {
            // TODO: Pass a material from the game player
            CreateLine(null);
        } 
        else
        {
            Ray camPos = camera.ScreenPointToRay(new Vector3(input.pos.x, input.pos.y, 0));
                
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(camPos, out hit, 1 << gameObject.layer))
            {
                GameObject other = hit.collider.gameObject;
                if (JoinableReleaseEventHandler != null && other.GetComponent<JoinableLineDraw>() != null)
                {
                    JoinableReleaseEventHandler(this, other.GetComponent<JoinableLineDraw>());
                }
            }

            LineDrawManager.Instance.DestroyLine(this);
        }
    }

    public void Off(Transform targetPosition)
    {
        StartCoroutine(TweenToPos(targetPosition));
    }

    IEnumerator TweenToPos(Transform targetPosition)
    {
        iTween.Stop(gameObject);
        collider.enabled = false;
        iTween.MoveTo(gameObject, targetPosition.position, 1.0f);
        yield return new WaitForSeconds(1.0f);
        iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}
