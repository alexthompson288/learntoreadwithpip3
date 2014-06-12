/*
using UnityEngine;
using System.Collections;

public class SimpleSpriteAnim : MonoBehaviour 
{
	public delegate void AnimFinish(string animName);
	public event AnimFinish OnAnimFinish;
	
	[System.Serializable]
	public class AnimDetails
	{
		[SerializeField]
		public string m_name;
		[SerializeField]
		public string m_prefix;
		[SerializeField]
		public int m_numFrames;
		[SerializeField]
		public int m_lowFrame = 0;
		[SerializeField]
		public bool m_toLetter = false;
		[SerializeField]
		public float m_fps;
		[SerializeField]
		public bool m_oneBased;
		[SerializeField]
		public int m_startFrame = 1;
		[SerializeField]
		public bool m_loop = true;
	}
	
	[SerializeField]
	AnimDetails[] m_availableAnimations;
	[SerializeField]
	UISprite m_sprite;
	[SerializeField]
	string m_format = "{0}";
	
	int m_currentAnimation;
	float m_currentT;
	int m_currentFrame;
	
	bool m_sentEvent;
	
	public void PlayAnimation(string animName)
	{
		m_sentEvent = false;
		int index = 0;
		foreach (AnimDetails ad in m_availableAnimations)
		{
			if (ad.m_name == animName)
			{
				m_currentAnimation = index;
				m_currentFrame = 0;
			}
			++index;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (string.IsNullOrEmpty(m_format))
		{
			m_format = "{0}";
		}
		if (m_availableAnimations.Length > m_currentAnimation)
		{
			m_currentT += Time.deltaTime * m_availableAnimations[m_currentAnimation].m_fps;
			if (m_currentT > 1)
			{
				m_currentT -= 1;
				m_currentFrame++;
				
				//if (m_currentFrame >= m_availableAnimations[m_currentAnimation].m_numFrames)
				if (m_currentFrame >= m_availableAnimations[m_currentAnimation].m_numFrames)
				{
					if(m_availableAnimations[m_currentAnimation].m_loop)
					{
						m_currentFrame = 0;
					}
					else if(!m_sentEvent)
					{
						if(OnAnimFinish != null)
						{
							OnAnimFinish(m_availableAnimations[m_currentAnimation].m_name);
						}
						
						m_sentEvent = true;
					}
				}
				
				if(m_currentFrame < m_availableAnimations[m_currentAnimation].m_numFrames)
				{
					int frame = m_currentFrame + m_availableAnimations[m_currentAnimation].m_lowFrame;
					frame += m_availableAnimations[m_currentAnimation].m_startFrame;

					string frameString = string.Format(m_format, frame);
					if (m_availableAnimations[m_currentAnimation].m_toLetter)
					{
						frameString = ((char)('a' + frame)).ToString();
					}
					m_sprite.spriteName = m_availableAnimations[m_currentAnimation].m_prefix + frameString;
				}
			}
		}
	}
	
	public string[] GetAnimNames()
	{
		string[] animNames = new string[m_availableAnimations.Length];
		for(int i = 0; i < m_availableAnimations.Length; ++i)
		{
			animNames[i] = m_availableAnimations[i].m_name;
		}
		
		return animNames;
	}
	
	public void SetSprite(UISprite sprite)
	{
		m_sprite = sprite;
	}
	
	public void SetAnimFPS(int newFPS)
	{
		foreach(AnimDetails animDetail in m_availableAnimations)
		{
			animDetail.m_fps = newFPS;
		}
	}
}
*/

using UnityEngine;
using System.Collections;

public class SimpleSpriteAnim : MonoBehaviour 
{
	public delegate void AnimFinish(string animName);
	public event AnimFinish OnAnimFinish;

    [System.Serializable]
    public class AnimDetails
    {
        [SerializeField]
        public UIAtlas m_spriteAtlas;
        [SerializeField]
        public string m_name;
        [SerializeField]
        public string m_prefix;
        [SerializeField]
        public int m_numFrames;
        [SerializeField]
        public int m_lowFrame = 0;
        [SerializeField]
        public bool m_toLetter = false;
        [SerializeField]
        public float m_fps;
        [SerializeField]
        public bool m_oneBased;
		[SerializeField]
		public bool m_loop = true;
    }

    [SerializeField]
    AnimDetails[] m_availableAnimations;
    [SerializeField]
    UISprite m_sprite;
    [SerializeField]
    string m_format = "{0}";

    int m_currentAnimation;
    float m_currentT;
    int m_currentFrame;

	bool m_sentEvent;

	void Awake()
	{
		if(m_sprite == null)
		{
			m_sprite = GetComponent<UISprite>() as UISprite;
		}
	}

    public void PlayAnimation(string animName)
    {
		m_sentEvent = false;
        int index = 0;
        foreach (AnimDetails ad in m_availableAnimations)
        {
            if (ad.m_name == animName)
            {
                m_currentAnimation = index;
                m_currentFrame = 0;

                if(ad.m_spriteAtlas != null)
                {
                    m_sprite.atlas = ad.m_spriteAtlas;
                }

                SetSpriteName();
            }
            ++index;
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (string.IsNullOrEmpty(m_format))
        {
            m_format = "{0}";
        }
        if (m_availableAnimations.Length > m_currentAnimation)
        {
            m_currentT += Time.deltaTime * m_availableAnimations[m_currentAnimation].m_fps;
            if (m_currentT > 1)
            {
                m_currentT -= 1;
                m_currentFrame++;

                //if (m_currentFrame >= m_availableAnimations[m_currentAnimation].m_numFrames)
				if (m_currentFrame >= m_availableAnimations[m_currentAnimation].m_numFrames)
				{
					if(m_availableAnimations[m_currentAnimation].m_loop)
					{
                        if(transform.parent.name.Contains("FlowerBlue") && m_availableAnimations[m_currentAnimation].m_name == "ON")
                        {
                            Debug.Log("Looping ON");
                        }
                    	m_currentFrame = 0;
					}
					else if(!m_sentEvent)
					{
						if(OnAnimFinish != null)
						{
							OnAnimFinish(m_availableAnimations[m_currentAnimation].m_name);
						}
						
						m_sentEvent = true;
					}
                }

				if(m_currentFrame < m_availableAnimations[m_currentAnimation].m_numFrames)
				{
                    SetSpriteName();
				}
            }
        }
	}

    void SetSpriteName()
    {
        int frame = m_currentFrame + m_availableAnimations[m_currentAnimation].m_lowFrame;
        if (m_availableAnimations[m_currentAnimation].m_oneBased)
        {
            frame++;
        }
        string frameString = string.Format(m_format, frame);
        if (m_availableAnimations[m_currentAnimation].m_toLetter)
        {
            frameString = ((char)('a' + frame)).ToString();
        }

        m_sprite.spriteName = m_availableAnimations [m_currentAnimation].m_prefix + frameString;
    }

	public string[] GetAnimNames()
	{
		string[] animNames = new string[m_availableAnimations.Length];
		for(int i = 0; i < m_availableAnimations.Length; ++i)
		{
			animNames[i] = m_availableAnimations[i].m_name;
		}

		return animNames;
	}

	public void SetSprite(UISprite sprite)
	{
		m_sprite = sprite;
	}

	public void SetAnimFPS(int newFPS)
	{
		foreach(AnimDetails animDetail in m_availableAnimations)
		{
			animDetail.m_fps = newFPS;
		}
	}

    public bool HasAnim(string animName)
    {
        bool foundAnim = false;

        foreach (AnimDetails anim in m_availableAnimations)
        {
            if(anim.m_name == animName)
            {
                foundAnim = true;
                break;
            }
        }
       
        return foundAnim;
    }
}