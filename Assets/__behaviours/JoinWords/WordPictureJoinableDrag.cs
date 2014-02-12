using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class WordPictureJoinableDrag : MonoBehaviour
{
    [SerializeField]
    private bool m_isPicture;
    [SerializeField]
    private UITexture m_pictureTexture;
    [SerializeField]
    private UILabel m_textLabel;
    [SerializeField]
    private GameObject m_linePrefab;
    [SerializeField]
    float m_dotFrequency = 0.1f;

    private string m_word;
    private Vector3 m_startDragPos;
    private Vector3 m_lastDropPos;
    private bool m_validDrag = false;

    List<GameObject> m_spawnedLineBits = new List<GameObject>();

    public void SetUp(string word)
    {
        m_word = word;
        if (m_isPicture)
        {
            Texture2D wordImage = (Texture2D)Resources.Load("Images/word_images_png_350/_" + word);
            m_pictureTexture.mainTexture = wordImage;
        }
        else
        {
            m_textLabel.text = word;
        }
    }

    void Start()
    {
        iTween.ScaleTo(gameObject, Vector3.one, 1.0f);
    }

    void OnClick()
    {
        if (m_isPicture)
        {
            JoinWordsCoordinator.Instance.SpeakWord(m_word);
        }
    }

    void OnPress(bool press)
    {
        if (press)
        {
            Ray camPos = UICamera.currentCamera.ScreenPointToRay(
    new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
            m_startDragPos = camPos.origin;
            m_startDragPos.z = 0;
            m_lastDropPos = m_startDragPos;
            m_validDrag = true;
        }
        else
        {
            foreach(GameObject go in m_spawnedLineBits)
            {
                Destroy(go);
            }
            m_spawnedLineBits.Clear();
            if (m_validDrag)
            {
                Ray camPos = UICamera.currentCamera.ScreenPointToRay(
                    new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));

                RaycastHit rhi = new RaycastHit();
                if (Physics.Raycast(camPos, out rhi, 1 << gameObject.layer))
                {
                    GameObject other = rhi.collider.gameObject;
                    if (other.GetComponent<WordPictureJoinableDrag>() != null)
                    {
                        JoinWordsCoordinator.Instance.Connect(other.GetComponent<WordPictureJoinableDrag>(),
                            this);
                    }
                }
            }
        }
    }

    void OnDrag(Vector2 drag)
    {
        if (m_validDrag)
        {
            Ray camPos = UICamera.currentCamera.ScreenPointToRay(
                new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0));
            Vector3 curPos = camPos.origin;
            curPos.z = 0;

            Vector3 delta = curPos - m_lastDropPos;
            if (delta.magnitude > m_dotFrequency)
            {
                float amt = delta.magnitude;
                while (amt > m_dotFrequency)
                {

                    GameObject newDottedLine = SpawningHelpers.InstantiateUnderWithIdentityTransforms(
                        m_linePrefab, transform);
                    m_spawnedLineBits.Add(newDottedLine);

                    newDottedLine.transform.position = m_lastDropPos + (amt * delta.normalized);
                    newDottedLine.transform.localRotation = Quaternion.LookRotation(Vector3.forward,
                        (curPos - m_lastDropPos).normalized);

                    amt -= m_dotFrequency;
                }
                m_lastDropPos = curPos;
            }
            if (m_spawnedLineBits.Count > 200)
            {
                foreach (GameObject go in m_spawnedLineBits)
                {
                    Destroy(go);
                }
                m_spawnedLineBits.Clear();
                m_validDrag = false;
            }
        }
    }

    public void Off(Transform targetPosition)
    {
        StartCoroutine(TransitionOff(targetPosition));
    }

    IEnumerator TransitionOff(Transform targetPosition)
    {
        iTween.Stop(gameObject);
        collider.enabled = false;
        iTween.MoveTo(gameObject, targetPosition.position, 1.0f);
        yield return new WaitForSeconds(1.0f);
        iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    public bool IsPicture()
    {
        return m_isPicture;
    }

    public string GetWord()
    {
        return m_word;
    }
}
