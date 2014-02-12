using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wingrove;

public class LetterPictureJoinableDrag : MonoBehaviour
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
	
	private JoinLettersPlayer m_gamePlayer;
	
	private DataRow m_letterData;
    private string m_word;
    private Vector3 m_startDragPos;
    private Vector3 m_lastDropPos;
    private bool m_validDrag = false;

    List<GameObject> m_spawnedLineBits = new List<GameObject>();
	
	public void SetUp(JoinLettersPlayer gamePlayer, DataRow letterData, Texture2D texture)
	{
		m_gamePlayer = gamePlayer;
		m_letterData = letterData;
        m_word = m_letterData["phoneme"].ToString();
		
        if (m_isPicture)
        {
            m_pictureTexture.mainTexture = texture;
        }
        else
        {
            m_textLabel.text = m_word;
        }
    }

    void Start()
    {
        iTween.ScaleTo(gameObject, Vector3.one, 1.0f);
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
                    if (other.GetComponent<LetterPictureJoinableDrag>() != null)
                    {
						m_gamePlayer.Connect(other.GetComponent<LetterPictureJoinableDrag>(), this);
                    }
                }
            }
        }
    }
	
	void OnClick()
	{
		JoinLettersCoordinator.Instance.LetterClicked(this);
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
                    //newDottedLine.transform.localRotation = Quaternion.LookRotation(Vector3.forward,
                        //(curPos - m_lastDropPos).normalized);
					newDottedLine.transform.rotation = Quaternion.LookRotation(Vector3.forward,
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
		Destroy(collider);
        StartCoroutine(TransitionOff(targetPosition));
    }

    IEnumerator TransitionOff(Transform targetPosition)
    {
        iTween.Stop(gameObject);
        collider.enabled = false;
        iTween.MoveTo(gameObject, targetPosition.position, 2.5f);
        yield return new WaitForSeconds(2.5f);
        StartCoroutine(DestroyJoinable());
    }
	
	public IEnumerator DestroyJoinable()
	{
		iTween.ScaleTo(gameObject, Vector3.zero, 1.0f);
		WingroveAudio.WingroveRoot.Instance.PostEvent("SOMETHING_DISAPPEARS");
        //yield return new WaitForSeconds(1.0f);
		yield return new WaitForSeconds(0.9f);
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
	
	public DataRow GetLetterData()
	{
		return m_letterData;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.GetComponent<LetterPictureJoinableDrag>() == null)
		{
			return;
		}
		
		Debug.Log(other.name + " has entered collider of " + name);
		Vector3 delta = transform.position - other.transform.position;
		//delta = delta.normalized * 0.2f;
		delta.Normalize();
		rigidbody.velocity = delta;
		//transform.position += delta;
		Debug.Log("Pos: " + transform.position);
		Debug.Log("Delta: " + delta);
	}
	
	void OnTriggerExit(Collider other)
	{
		Debug.Log(other.name + " has exited collider of " + name);
		rigidbody.velocity = Vector3.zero;
	}
}
