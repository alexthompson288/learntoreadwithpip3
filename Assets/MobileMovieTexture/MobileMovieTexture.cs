using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;


public class ChannelTexture
{
    public Texture2D tex;
#if !((UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR)
    public Color32[] pixels;
    public GCHandle handle;
#endif

#if UNITY_IPHONE && !UNITY_EDITOR
    private const string PLATFORM_DLL = "__Internal";
#else
    private const string PLATFORM_DLL = "theorawrapper";
#endif

    [DllImport(PLATFORM_DLL)]
    private static extern void ReAllocTexture(int handle, int x, int y);

    public ChannelTexture(int width, int height, TextureFormat format)
    {
        tex = new Texture2D(width, height, format, false);

        tex.wrapMode = TextureWrapMode.Clamp; //important for NPOT

#if !((UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR)
        pixels = tex.GetPixels32(0);
        handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
#endif

        ReAlloc();
    }

    public void ReAlloc()
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        ReAllocTexture( tex.GetNativeTextureID(), tex.width, tex.height);
#endif
    }

    public void Destroy()
    {
#if !((UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR)
        pixels = null;
        handle.Free();
#endif
		MonoBehaviour.Destroy(tex);
    }
};

public class MobileMovieTexture : MonoBehaviour
{
    [UnityEngine.SerializeField]
    private string m_path;

    [UnityEngine.SerializeField]
    private Material m_movieMaterial;

    [UnityEngine.SerializeField]
    private bool m_playAutomatically = true;

    [UnityEngine.SerializeField]
    private bool m_loop = true;

    private IntPtr m_nativeContext = IntPtr.Zero;

    private int m_picWidth = 0;
    private int m_picHeight = 0;
    private int m_picX = 0;
    private int m_picY = 0;

    private int m_yStride = 0;
    private int m_yHeight = 0;
    private int m_uvStride = 0;
    private int m_uvHeight = 0;

    private const int CHANNELS = 3; //Y,Cr,Cb
    private ChannelTexture[] m_ChannelTextures;

    private bool m_advance = true;

    private double m_elapsedTime;
    private double m_nextFrameTime;
    private double m_lastFrameDelta = double.MaxValue;


#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
    private bool m_reallocTextures = false;
#endif

    public delegate void OnFinished(MobileMovieTexture sender);

    public event OnFinished onFinished;

    private bool m_hasFinished = true;

    public string Path
    {
        get { return m_path; }
    }

    public bool playAutomatically
    {
        set { m_playAutomatically = value; }
    }

    public Material MovieMaterial
    {
        get { return m_movieMaterial; }
    }

    public int width
    {
        get { return m_picWidth; }
    }

    public int height
    {
        get { return m_picHeight; }
    }

    public float aspectRatio
    {
        get
        {
            if (m_nativeContext != IntPtr.Zero)
            {
                return GetAspectRatio(m_nativeContext);
            }
            else
            {
                return 1.0f;
            }
        }
    }

    public bool isPlaying
    {
        get { return m_nativeContext != IntPtr.Zero && !m_hasFinished && m_advance; }
    }

    public bool pause
    {
        get { return !m_advance; }
        set { m_advance = !value; }
    }

    #region NATIVE_INTERFACE

#if UNITY_IPHONE && !UNITY_EDITOR
    private const string PLATFORM_DLL = "__Internal";
#else
    private const string PLATFORM_DLL = "theorawrapper";
#endif

    [DllImport(PLATFORM_DLL)]
    private static extern IntPtr OpenStream(string path, int offset, int size, bool loop, bool pot);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetPicWidth(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetPicHeight(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetPicX(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetPicY(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern bool DecodeFrame(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetYStride(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetYHeight(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetUVStride(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern int GetUVHeight(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern bool DecodeAndUpload(IntPtr context, int yHandle, int uHandle, int vHandle);

    [DllImport(PLATFORM_DLL)]
    private static extern void GetPlane(IntPtr context, int planeIndex, IntPtr buffer);

    [DllImport(PLATFORM_DLL)]
    private static extern double GetNextFrameTime(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern void CloseStream(IntPtr context);

    [DllImport(PLATFORM_DLL)]
    private static extern float GetAspectRatio(IntPtr context);

    #endregion

    void Start()
    {
        if (m_playAutomatically)
        {
            Play();
        }
    }

    public void SetFilename(string path)
    {
        m_path = path;
    }

    public void Play()
    {
        Close();

        m_elapsedTime = 0.0;
        m_nextFrameTime = 0.0;

        Open();

        m_advance = true;
        m_hasFinished = false;
    }

    public void Stop()
    {
        Close();
    }

#if UNITY_EDITOR
    private bool SanityCheckPath(string path)
    {
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("Unable to find movie at path: " + System.IO.Path.GetFullPath(path));
            return false;
        }

        if (path.Contains("\\"))
        {
            Debug.LogError("Movie path contains a back slash, these don't work on Android, please use forward slashes, path " + path);
            return false;
        }

        return true;
    }
#endif //UNITY_EDITOR

    private void Open()
    {
        bool mobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
        bool powerOf2Textures = !mobile;

        string path;

        long offset;
        long length;

        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                path = Application.dataPath;

                if (!AssetStream.GetZipFileOffsetLength(Application.dataPath, m_path, out offset, out length))
                {
                    throw new System.Exception("problem opening movie");
                }
                break;
            default:
                path = Application.streamingAssetsPath + "/" + m_path;
                offset = 0;
                length = 0;
                break;
        }

#if UNITY_EDITOR
        if (!SanityCheckPath(path))
        {
            throw new System.Exception("Can't find video at path" + path);
        }
#endif //UNITY_EDITOR

        m_nativeContext = OpenStream(path, (int)offset, (int)length, m_loop, powerOf2Textures);

        if (m_nativeContext != IntPtr.Zero)
        {
            m_picWidth = GetPicWidth(m_nativeContext);
            m_picHeight = GetPicHeight(m_nativeContext);

            m_picX = GetPicX(m_nativeContext);
            m_picY = GetPicY(m_nativeContext);

            int yStride = GetYStride(m_nativeContext);
            int yHeight = GetYHeight(m_nativeContext);
            int uvStride = GetUVStride(m_nativeContext);
            int uvHeight = GetUVHeight(m_nativeContext);
			
			if (yStride != m_yStride || yHeight != m_yHeight || uvStride != m_uvStride || uvHeight != m_uvHeight)
			{
				m_yStride = yStride;
				m_yHeight = yHeight;
				m_uvStride = uvStride;
				m_uvHeight = uvHeight;
				
				DestroyTextures();
			
            	AllocateTexures();
			}

            //Decode the first frame to avoid a flash
            Decode(true);
            m_nextFrameTime = GetNextFrameTime(m_nativeContext);
        }
        else
        {
            throw new System.Exception("problem opening movie");
        }
    }

    private void AllocateTexures()
    {
        m_ChannelTextures = new ChannelTexture[CHANNELS];

#if UNITY_ANDROID && !UNITY_EDITOR
        TextureFormat format = TextureFormat.RGBA32; //We realloc as GL_LUMINANCE 
#elif UNITY_IPHONE && !UNITY_EDITOR
        TextureFormat format = TextureFormat.Alpha8; //We realloc as GL_LUMINANCE 
#else
        TextureFormat format = TextureFormat.RGBA32;
#endif

        m_ChannelTextures[0] = new ChannelTexture(m_yStride, m_yHeight, format);
        m_ChannelTextures[1] = new ChannelTexture(m_uvStride, m_uvHeight, format);
        m_ChannelTextures[2] = new ChannelTexture(m_uvStride, m_uvHeight, format);

        Vector2 uvYScale = new Vector2((float)(m_picWidth) / (float)(m_yStride), -((float)(m_picHeight) / (float)(m_yHeight)));

        Vector2 uvYOffset = new Vector2((float)m_picX / (float)(m_yStride), ((float)(m_picHeight) + (float)(m_picY)) / (float)(m_yHeight));

        Vector2 uvCrCbScale = new Vector2();
        Vector2 uvCrCbOffset = new Vector2();

        if (m_uvStride == m_yStride)
        {
            uvCrCbScale.x = uvYScale.x;
        }
        else
        {
            uvCrCbScale.x = ((float)(m_picWidth) / 2.0f) / (float)(m_uvStride);
        }

        if (m_uvHeight == m_yHeight)
        {
            uvCrCbScale.y = uvYScale.y;
            uvCrCbOffset = uvYOffset;
        }
        else
        {
            uvCrCbScale.y = -(((float)(m_picHeight) / 2.0f) / (float)(m_uvHeight));
            uvCrCbOffset = new Vector2((((float)m_picX) / 2.0f) / (float)(m_uvStride), ((((float)(m_picHeight) + (float)(m_picY)) / 2.0f) / (float)(m_uvHeight)));
        }

        m_movieMaterial.SetTexture("_YTex", m_ChannelTextures[0].tex);
        m_movieMaterial.SetTexture("_CrTex", m_ChannelTextures[1].tex);
        m_movieMaterial.SetTexture("_CbTex", m_ChannelTextures[2].tex);

        m_movieMaterial.SetTextureScale("_YTex", uvYScale);
        m_movieMaterial.SetTextureOffset("_YTex", uvYOffset);

        m_movieMaterial.SetTextureScale("_CbTex", uvCrCbScale);
        m_movieMaterial.SetTextureOffset("_CbTex", uvCrCbOffset);
    }

    private void DestroyTextures()
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        m_reallocTextures = false;
#endif
		if (m_ChannelTextures != null)
		{
			for( int i = 0; i < CHANNELS; ++i)
			{
				if(m_ChannelTextures[i] != null)
				{
					m_ChannelTextures[i].Destroy();
				}
			}
		}
        
        m_ChannelTextures = null;
    }


    void Close()
    {
        if (m_nativeContext != IntPtr.Zero)
        {
            CloseStream(m_nativeContext);

            m_nativeContext = IntPtr.Zero;
        }
    }

    void OnDestroy()
    {
        Close();

        DestroyTextures();
    }

#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            m_reallocTextures = true; //To early to do it here, the context isn't restored yet on android , do it in update
        }
    }
#endif


    void Update()
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
        if (m_reallocTextures && m_ChannelTextures != null)
        {
            for (int i = 0; i < CHANNELS; ++i)
            {
                m_ChannelTextures[i].ReAlloc();
            }

            m_reallocTextures = false;
        }
#endif

        if (m_nativeContext != IntPtr.Zero && !m_hasFinished && m_advance)
        {
            m_elapsedTime += Time.deltaTime;

            while (m_elapsedTime > m_nextFrameTime)
            {
                bool upload = false;

                if (m_nextFrameTime + m_lastFrameDelta >= m_elapsedTime)
                {
                    upload = true;
                }

                Decode(upload);

                double nextFrameTime = GetNextFrameTime(m_nativeContext);

                if (nextFrameTime == m_nextFrameTime)
                {
                    m_hasFinished = true;

                    if (onFinished != null)
                    {
                        onFinished(this);
                    }
					
					 break;
                }

                m_lastFrameDelta = nextFrameTime - m_nextFrameTime;

                m_nextFrameTime = nextFrameTime;
            }
        }
    }

    private void Decode(bool upload)
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR

        if (upload)
        {
            DecodeAndUpload(m_nativeContext, m_ChannelTextures[0].tex.GetNativeTextureID(), m_ChannelTextures[1].tex.GetNativeTextureID(), m_ChannelTextures[2].tex.GetNativeTextureID()  );

		    GL.InvalidateState();
        }
        else
        {
            DecodeAndUpload(m_nativeContext, -1, -1, -1  );
        }
        
#else
        bool frameReady = DecodeFrame(m_nativeContext);

        if (frameReady && upload)
        {
            GetPlane(m_nativeContext, 0, m_ChannelTextures[0].handle.AddrOfPinnedObject());
            GetPlane(m_nativeContext, 1, m_ChannelTextures[1].handle.AddrOfPinnedObject());
            GetPlane(m_nativeContext, 2, m_ChannelTextures[2].handle.AddrOfPinnedObject());

            m_ChannelTextures[0].tex.SetPixels32(m_ChannelTextures[0].pixels, 0);
            m_ChannelTextures[1].tex.SetPixels32(m_ChannelTextures[1].pixels, 0);
            m_ChannelTextures[2].tex.SetPixels32(m_ChannelTextures[2].pixels, 0);

            m_ChannelTextures[0].tex.Apply(false);
            m_ChannelTextures[1].tex.Apply(false);
            m_ChannelTextures[2].tex.Apply(false);
        }
#endif
    }
}
