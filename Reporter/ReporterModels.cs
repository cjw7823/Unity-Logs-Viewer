 using UnityEngine;
 
[System.Serializable]
public class Images
{
	public Texture2D clearImage;
	public Texture2D collapseImage;
	public Texture2D clearOnNewSceneImage;
	public Texture2D showTimeImage;
	public Texture2D showSceneImage;
	public Texture2D userImage;
	public Texture2D showMemoryImage;
	public Texture2D softwareImage;
	public Texture2D dateImage;
	public Texture2D showFpsImage;
	public Texture2D infoImage;
    public Texture2D saveLogsImage; 
    public Texture2D searchImage;
    public Texture2D copyImage;
    public Texture2D closeImage;

	public Texture2D buildFromImage;
	public Texture2D systemInfoImage;
	public Texture2D graphicsInfoImage;
	public Texture2D backImage;

	public Texture2D logImage;
	public Texture2D warningImage;
	public Texture2D errorImage;

	public Texture2D barImage_White;
	public Texture2D barImage_Dark;
	public Texture2D button_activeImage_White;
	public Texture2D button_activeImage_Dark;
	public Texture2D even_logImage_White;
	public Texture2D even_logImage_Dark;
	public Texture2D odd_logImage_white;
	public Texture2D odd_logImage_Dark;
	public Texture2D selectedImage;

	public GUISkin reporterScrollerSkin;
}

public class Log
{
    public int count = 1;
    public _LogType logType;
    public string condition;
    public string stacktrace;
    public int sampleId;
    //public string   objectName="" ;//object who send error
    //public string   rootName =""; //root of object send error

    public Log CreateCopy()
    {
        return (Log)this.MemberwiseClone();
    }
    public float GetMemoryUsage()
    {
        return (float)(sizeof(int) +
                sizeof(_LogType) +
                condition.Length * sizeof(char) +
                stacktrace.Length * sizeof(char) +
                sizeof(int));
    }
}

public enum _LogType
{
    Assert    = LogType.Assert,
    Error     = LogType.Error,
    Exception = LogType.Exception,
    Log       = LogType.Log,
    Warning   = LogType.Warning,
}

public class Sample
{
    public string time;
    public byte loadedScene;
    public float memory;
    public float fps;
    public string fpsText;
    public static float MemSize()
    {
        float s = sizeof(float) + sizeof(byte) + sizeof(float) + sizeof(float);
        return s;
    }

    public string GetSceneName()
    {
        if (loadedScene == 255)
            return "AssetBundleScene";

        return Reporter.scenes[loadedScene];
    }
}

enum ReportView
{
    None,
    Logs,
    Info,
    Snapshot,
}

enum DetailView
{
    None,
    StackTrace,
    Graph,
}