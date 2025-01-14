#if UNITY_CHANGE1 || UNITY_CHANGE2 || UNITY_CHANGE3 || UNITY_CHANGE4
#warning UNITY_CHANGE has been set manually
#elif UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_CHANGE1
#elif UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_CHANGE2
#else
#define UNITY_CHANGE3
#endif
#if UNITY_2018_3_OR_NEWER
#define UNITY_CHANGE4
#endif
//use UNITY_CHANGE1 for unity older than "unity 5"
//use UNITY_CHANGE2 for unity 5.0 -> 5.3 
//use UNITY_CHANGE3 for unity 5.3 (fix for new SceneManger system)
//use UNITY_CHANGE4 for unity 2018.3 (Networking system)

using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#if UNITY_CHANGE3
using UnityEngine.SceneManagement;
#endif
#if UNITY_CHANGE4
using UnityEngine.Networking;
#endif

//To use Reporter just create reporter from menu (Reporter->Create) at first scene your game start.
//then set the ” Scrip execution order ” in (Edit -> Project Settings ) of Reporter.cs to be the highest.

//Now to view logs all what you have to do is to make a circle gesture using your mouse (click and drag) 
//or your finger (touch and drag) on the screen to show all these logs
//no coding is required 

public class Reporter : MonoBehaviour
{
	List<Sample> samples = new List<Sample>();

	//contains all uncollapsed log
	List<Log> logs = new List<Log>();
	//contains all collapsed logs
	List<Log> collapsedLogs = new List<Log>();
	//contain logs which should only appear to user , for example if you switch off show logs + switch off show warnings
	//and your mode is collapse,then this list will contains only collapsed errors
	List<Log> currentLog = new List<Log>();

	//used to check if the new coming logs is already exist or new one
	MultiKeyDictionary<string, string, Log> logsDic = new MultiKeyDictionary<string, string, Log>();
	//to save memory
	Dictionary<string, string> cachedString = new Dictionary<string, string>();

	//show hide In Game Logs
	public bool show = false;
	//collapse logs
	bool collapse;
	//to decide if you want to clean logs for new loaded scene
	bool clearOnNewSceneLoaded;

	bool showTime = true;

	bool showScene = false;

	bool showMemory;

	bool showFps;

	//show or hide logs
	bool showLog = true;
	//show or hide warnings
	bool showWarning = true;
	//show or hide errors
	bool showError = true;

	//total number of logs
	int numOfLogs = 0;
	//total number of warnings
	int numOfLogsWarning = 0;
	//total number of errors
	int numOfLogsError = 0;
	//total number of collapsed logs
	int numOfCollapsedLogs = 0;
	//total number of collapsed warnings
	int numOfCollapsedLogsWarning = 0;
	//total number of collapsed errors
	int numOfCollapsedLogsError = 0;

	//maximum number of allowed logs to view
	//public int maxAllowedLog = 1000 ;

	bool showClearOnNewSceneLoadedButton = false;
	bool showTimeButton = false;
	bool showSceneButton = false;
	bool showMemButton = true;
	bool showFpsButton = true;
	bool showSearchText = false;
    bool showCopyButton = false;
    bool showSaveButton = false;

    string buildDate;
	string logDate;
	float logsMemUsage;
	float graphMemUsage;
	public float TotalMemUsage
	{
		get
		{
			return logsMemUsage + graphMemUsage;
		}
	}
	float gcTotalMemory;
	public string UserData = "";
	//frame rate per second
	public float fps;
	public string fpsText;
	ReportView currentView = ReportView.Logs;

	//used to check if you have In Game Logs multiple time in different scene
	//only one should work and other should be deleted
	static bool created = false;
	//public delegate void OnLogHandler( string condition, string stack-trace, LogType type );
	//public event OnLogHandler OnLog ;

	public Images images;
	// gui
	GUIContent clearContent;
	GUIContent collapseContent;
	GUIContent clearOnNewSceneContent;
	GUIContent showTimeContent;
	GUIContent showSceneContent;
	GUIContent userContent;
	GUIContent showMemoryContent;
	GUIContent softwareContent;
	GUIContent dateContent;
	GUIContent showFpsContent;
	//GUIContent graphContent;
	GUIContent infoContent;
    GUIContent saveLogsContent;
	GUIContent searchContent;
    GUIContent copyContent;
    GUIContent closeContent;

	GUIContent buildFromContent;
	GUIContent systemInfoContent;
	GUIContent graphicsInfoContent;
	GUIContent backContent;

	//GUIContent cameraContent;

	GUIContent logContent;
	GUIContent warningContent;
	GUIContent errorContent;
	GUIStyle barStyle;
	GUIStyle buttonActiveStyle;

	GUIStyle nonStyle;
	GUIStyle lowerLeftFontStyle;
	GUIStyle backStyle;
	GUIStyle evenLogStyle;
	GUIStyle oddLogStyle;
	GUIStyle logButtonStyle;
	GUIStyle selectedLogStyle;
	GUIStyle selectedLogFontStyle;
	GUIStyle stackLabelStyle;
	GUIStyle stackTraceStyle;
	GUIStyle scrollerStyle;
	GUIStyle searchStyle;
	GUIStyle sliderBackStyle;
	GUIStyle sliderThumbStyle;
	GUISkin toolbarScrollerSkin;
	GUISkin logScrollerSkin;
	GUISkin graphScrollerSkin;

	public Vector2 size = new Vector2(32, 32);
	public float maxSize = 20;
	public int numOfCircleToShow = 1;
	public static string[] scenes {get; private set;}
	string currentScene;
	string filterText = "";

	string deviceModel;
	string deviceType;
	string deviceName;
	string graphicsMemorySize;
#if !UNITY_CHANGE1
	string maxTextureSize;
#endif
	string systemMemorySize;

	// 클래스 상단에 RectTransform 변수 추가
	[SerializeField] 
	private RectTransform gestureArea;
	private Canvas parentCanvas; // Canvas 참조 추가
	private bool isDarkTheme = false;

	void Awake()
	{
		if (!Initialized)
			Initialize();

#if UNITY_CHANGE3
        SceneManager.sceneLoaded += _OnLevelWasLoaded;
#endif
    }

    private void OnDestroy()
    {
#if UNITY_CHANGE3
        SceneManager.sceneLoaded -= _OnLevelWasLoaded;
#endif
    }

    void OnEnable()
	{
		if (logs.Count == 0)//if recompile while in play mode
			clear();
	}

	void addSample()
	{
		Sample sample = new Sample();
		sample.fps = fps;
		sample.fpsText = fpsText;
#if UNITY_CHANGE3
		sample.loadedScene = (byte)SceneManager.GetActiveScene().buildIndex;
#else
		sample.loadedScene = (byte)Application.loadedLevel;
#endif
		//sample.time = Time.realtimeSinceStartup;
		sample.time = System.DateTime.Now.ToString("HH:mm:ss");
		sample.memory = gcTotalMemory;
		samples.Add(sample);

		graphMemUsage = (samples.Count * Sample.MemSize()) / 1024 / 1024;
	}

	public bool Initialized = false;
	public void Initialize()
	{
		if (!created) {
			try {
				gameObject.SendMessage("OnPreStart");
			}
			catch (System.Exception e) {
				Debug.LogException(e);
			}
#if UNITY_CHANGE3
			scenes = new string[SceneManager.sceneCountInBuildSettings];
			currentScene = SceneManager.GetActiveScene().name;
#else
			scenes = new string[Application.levelCount];
			currentScene = Application.loadedLevelName;
#endif
			DontDestroyOnLoad(gameObject);
#if UNITY_CHANGE1
			Application.RegisterLogCallback (new Application.LogCallback (CaptureLog));
			Application.RegisterLogCallbackThreaded (new Application.LogCallback (CaptureLogThread));
#else
			//Application.logMessageReceived += CaptureLog ;
			Application.logMessageReceivedThreaded += CaptureLogThread;
#endif
			created = true;
			//addSample();
		}
		else {
			Debug.LogWarning("tow manager is exists delete the second");
			DestroyImmediate(gameObject, true);
			return;
		}


		//initialize gui and styles for gui purpose

		clearContent = new GUIContent("", images.clearImage, "Clear logs");
		collapseContent = new GUIContent("", images.collapseImage, "Collapse logs");
		clearOnNewSceneContent = new GUIContent("", images.clearOnNewSceneImage, "Clear logs on new scene loaded");
		showTimeContent = new GUIContent("", images.showTimeImage, "Show Hide Time");
		showSceneContent = new GUIContent("", images.showSceneImage, "Show Hide Scene");
		showMemoryContent = new GUIContent("", images.showMemoryImage, "Show Hide Memory");
		softwareContent = new GUIContent("", images.softwareImage, "Software");
		dateContent = new GUIContent("", images.dateImage, "Date");
		showFpsContent = new GUIContent("", images.showFpsImage, "Show Hide fps");
		infoContent = new GUIContent("", images.infoImage, "Information about application");
        saveLogsContent = new GUIContent("", images.saveLogsImage, "Save logs to device");
        searchContent = new GUIContent("", images.searchImage, "Search for logs");
        copyContent = new GUIContent("", images.copyImage, "Copy log to clipboard");
        closeContent = new GUIContent("", images.closeImage, "Hide logs");
		userContent = new GUIContent("", images.userImage, "User");

		buildFromContent = new GUIContent("", images.buildFromImage, "Build From");
		systemInfoContent = new GUIContent("", images.systemInfoImage, "System Info");
		graphicsInfoContent = new GUIContent("", images.graphicsInfoImage, "Graphics Info");
		backContent = new GUIContent("", images.backImage, "Back");


		//snapshotContent = new GUIContent("",images.cameraImage,"show or hide logs");
		logContent = new GUIContent("", images.logImage, "show or hide logs");
		warningContent = new GUIContent("", images.warningImage, "show or hide warnings");
		errorContent = new GUIContent("", images.errorImage, "show or hide errors");

        initializeStyle();

		Initialized = true;
		show = true;
		if (show) {
			doShow();
		}

		deviceModel = SystemInfo.deviceModel.ToString();
		deviceType = SystemInfo.deviceType.ToString();
		deviceName = SystemInfo.deviceName.ToString();
		graphicsMemorySize = SystemInfo.graphicsMemorySize.ToString();
#if !UNITY_CHANGE1
		maxTextureSize = SystemInfo.maxTextureSize.ToString();
#endif
		systemMemorySize = SystemInfo.systemMemorySize.ToString();

		// Canvas 컴포넌트 가져오기
		parentCanvas = gestureArea.GetComponentInParent<Canvas>();
	}

	void initializeStyle()
	{
		int paddingX = (int)(size.x * 0.2f);
		int paddingY = (int)(size.y * 0.2f);
		nonStyle = new GUIStyle();
		nonStyle.clipping = TextClipping.Clip;
		nonStyle.border = new RectOffset(0, 0, 0, 0);
		nonStyle.normal.background = null;
		nonStyle.fontSize = (int)(size.y / 2);
		nonStyle.alignment = TextAnchor.MiddleCenter;

		lowerLeftFontStyle = new GUIStyle();
		lowerLeftFontStyle.clipping = TextClipping.Clip;
		lowerLeftFontStyle.border = new RectOffset(0, 0, 0, 0);
		lowerLeftFontStyle.normal.background = null;
		lowerLeftFontStyle.fontSize = (int)(size.y / 2);
		lowerLeftFontStyle.fontStyle = FontStyle.Bold;
		lowerLeftFontStyle.alignment = TextAnchor.LowerLeft;


		barStyle = new GUIStyle();
		barStyle.border = new RectOffset(1, 1, 1, 1);
		barStyle.normal.background = images.barImage_White;
		barStyle.active.background = images.button_activeImage_White;
		barStyle.alignment = TextAnchor.MiddleCenter;
		barStyle.margin = new RectOffset(1, 1, 1, 1);

		//barStyle.padding = new RectOffset(paddingX,paddingX,paddingY,paddingY); 
		//barStyle.wordWrap = true ;
		barStyle.clipping = TextClipping.Clip;
		barStyle.fontSize = (int)(size.y / 2);


		buttonActiveStyle = new GUIStyle();
		buttonActiveStyle.border = new RectOffset(1, 1, 1, 1);
		buttonActiveStyle.normal.background = images.button_activeImage_White;
		buttonActiveStyle.alignment = TextAnchor.MiddleCenter;
		buttonActiveStyle.margin = new RectOffset(1, 1, 1, 1);
		//buttonActiveStyle.padding = new RectOffset(4,4,4,4);
		buttonActiveStyle.fontSize = (int)(size.y / 2);

		backStyle = new GUIStyle();
		backStyle.normal.background = images.even_logImage_White;
		backStyle.clipping = TextClipping.Clip;
		backStyle.fontSize = (int)(size.y / 2);

		evenLogStyle = new GUIStyle();
		evenLogStyle.normal.background = images.even_logImage_White;
		evenLogStyle.fixedHeight = size.y;
		evenLogStyle.clipping = TextClipping.Clip;
		evenLogStyle.alignment = TextAnchor.UpperLeft;
		evenLogStyle.imagePosition = ImagePosition.ImageLeft;
		evenLogStyle.fontSize = (int)(size.y / 2);
		//evenLogStyle.wordWrap = true;

		oddLogStyle = new GUIStyle();
		oddLogStyle.normal.background = images.odd_logImage_white;
		oddLogStyle.fixedHeight = size.y;
		oddLogStyle.clipping = TextClipping.Clip;
		oddLogStyle.alignment = TextAnchor.UpperLeft;
		oddLogStyle.imagePosition = ImagePosition.ImageLeft;
		oddLogStyle.fontSize = (int)(size.y / 2);
		//oddLogStyle.wordWrap = true ;

		logButtonStyle = new GUIStyle();
		//logButtonStyle.wordWrap = true;
		logButtonStyle.fixedHeight = size.y;
		logButtonStyle.clipping = TextClipping.Clip;
		logButtonStyle.alignment = TextAnchor.UpperLeft;
		//logButtonStyle.imagePosition = ImagePosition.ImageLeft ;
		//logButtonStyle.wordWrap = true;
		logButtonStyle.fontSize = (int)(size.y / 2);
		logButtonStyle.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);

		selectedLogStyle = new GUIStyle();
		selectedLogStyle.normal.background = images.selectedImage;
		selectedLogStyle.fixedHeight = size.y;
		selectedLogStyle.clipping = TextClipping.Clip;
		selectedLogStyle.alignment = TextAnchor.UpperLeft;
		selectedLogStyle.normal.textColor = Color.white;
		//selectedLogStyle.wordWrap = true;
		selectedLogStyle.fontSize = (int)(size.y / 2);

		selectedLogFontStyle = new GUIStyle();
		selectedLogFontStyle.normal.background = images.selectedImage;
		selectedLogFontStyle.fixedHeight = size.y;
		selectedLogFontStyle.clipping = TextClipping.Clip;
		selectedLogFontStyle.alignment = TextAnchor.UpperLeft;
		selectedLogFontStyle.normal.textColor = Color.white;
		//selectedLogStyle.wordWrap = true;
		selectedLogFontStyle.fontSize = (int)(size.y / 2);
		selectedLogFontStyle.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);

		stackLabelStyle = new GUIStyle();
		stackLabelStyle.wordWrap = true;
		stackLabelStyle.fontSize = (int)(size.y / 2) + 5;
		stackLabelStyle.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);

		stackTraceStyle = new GUIStyle();
		stackTraceStyle.wordWrap = true;
		stackTraceStyle.fontSize = (int)(size.y / 2) + 5;
		stackTraceStyle.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);
		//stackTraceStyle.normal.textColor = Color.white;

		scrollerStyle = new GUIStyle();
		scrollerStyle.normal.background = images.barImage_White;

		searchStyle = new GUIStyle();
		searchStyle.clipping = TextClipping.Clip;
		searchStyle.alignment = TextAnchor.LowerCenter;
		searchStyle.fontSize = (int)(size.y / 2);
		searchStyle.wordWrap = true;


		sliderBackStyle = new GUIStyle();
		sliderBackStyle.normal.background = images.barImage_White;
		sliderBackStyle.fixedHeight = size.y;
		sliderBackStyle.border = new RectOffset(1, 1, 1, 1);

		sliderThumbStyle = new GUIStyle();
		sliderThumbStyle.normal.background = images.selectedImage;
		sliderThumbStyle.fixedWidth = size.x;

		GUISkin skin = images.reporterScrollerSkin;

		toolbarScrollerSkin = (GUISkin)GameObject.Instantiate(skin);
		toolbarScrollerSkin.verticalScrollbar.fixedWidth = 0f;
		toolbarScrollerSkin.horizontalScrollbar.fixedHeight = 0f;
		toolbarScrollerSkin.verticalScrollbarThumb.fixedWidth = 0f;
		toolbarScrollerSkin.horizontalScrollbarThumb.fixedHeight = 0f;

		logScrollerSkin = (GUISkin)GameObject.Instantiate(skin);
		logScrollerSkin.verticalScrollbar.fixedWidth = size.x * 2f;
		logScrollerSkin.horizontalScrollbar.fixedHeight = 0f;
		logScrollerSkin.verticalScrollbarThumb.fixedWidth = size.x * 2f;
		logScrollerSkin.horizontalScrollbarThumb.fixedHeight = 0f;

		graphScrollerSkin = (GUISkin)GameObject.Instantiate(skin);
		graphScrollerSkin.verticalScrollbar.fixedWidth = 0f;
		graphScrollerSkin.horizontalScrollbar.fixedHeight = size.x * 2f;
		graphScrollerSkin.verticalScrollbarThumb.fixedWidth = 0f;
		graphScrollerSkin.horizontalScrollbarThumb.fixedHeight = size.x * 2f;
		//inGameLogsScrollerSkin.verticalScrollbarThumb.fixedWidth = size.x * 2;
		//inGameLogsScrollerSkin.verticalScrollbar.fixedWidth = size.x * 2;

		// Set initial styles based on the theme
		UpdateThemeStyles();
	}

	// Method to toggle dark theme
	public void ToggleDarkTheme()
	{
		isDarkTheme = !isDarkTheme; // Toggle the theme state
		UpdateThemeStyles(); // Update styles based on the new theme
	}

	// Method to update styles based on the current theme
	private void UpdateThemeStyles()
	{
		if (isDarkTheme)
		{
			backStyle.normal.background = images.even_logImage_Dark;
			evenLogStyle.normal.background = images.even_logImage_Dark;
			oddLogStyle.normal.background = images.odd_logImage_Dark;
			evenLogStyle.normal.textColor = Color.white;
			oddLogStyle.normal.textColor = Color.white;
			nonStyle.normal.textColor = Color.white;
			selectedLogStyle.normal.textColor = Color.white; // Assuming you want white text for selected log in dark theme
			logButtonStyle.normal.textColor = Color.white; // Applying logButtonStyle for dark theme
			stackLabelStyle.normal.textColor = Color.white; // Applying stackLabelStyle for dark theme
			stackTraceStyle.normal.textColor = Color.white; // Applying stackTraceStyle for dark theme

			// Update button styles for dark theme
			buttonActiveStyle.normal.background = images.button_activeImage_Dark; // Assuming you have a dark version
			barStyle.normal.background = images.barImage_Dark; // Assuming you have a dark version
		}
		else
		{
			backStyle.normal.background = images.even_logImage_White;
			evenLogStyle.normal.background = images.even_logImage_White;
			oddLogStyle.normal.background = images.odd_logImage_white;
			evenLogStyle.normal.textColor = Color.black;
			oddLogStyle.normal.textColor = Color.black;
			nonStyle.normal.textColor = Color.black;
			selectedLogStyle.normal.textColor = Color.black; // Assuming you want black text for selected log in light theme
			logButtonStyle.normal.textColor = Color.black; // Applying logButtonStyle for light theme
			stackLabelStyle.normal.textColor = Color.black; // Applying stackLabelStyle for light theme
			stackTraceStyle.normal.textColor = Color.black; // Applying stackTraceStyle for light theme

			// Update button styles for light theme
			buttonActiveStyle.normal.background = images.button_activeImage_White; // Assuming you have a light version
			barStyle.normal.background = images.barImage_White; // Assuming you have a light version
		}
	}

	void Start()
	{
		logDate = System.DateTime.Now.ToString();
		StartCoroutine("readInfo");
	}

	//clear all logs
	void clear()
	{
		logs.Clear();
		collapsedLogs.Clear();
		currentLog.Clear();
		logsDic.Clear();
		//selectedIndex = -1;
		selectedLog = null;
		numOfLogs = 0;
		numOfLogsWarning = 0;
		numOfLogsError = 0;
		numOfCollapsedLogs = 0;
		numOfCollapsedLogsWarning = 0;
		numOfCollapsedLogsError = 0;
		logsMemUsage = 0;
		graphMemUsage = 0;
		samples.Clear();
		System.GC.Collect();
		selectedLog = null;
	}

	Rect screenRect = Rect.zero;
	Rect toolBarRect = Rect.zero;
	Rect logsRect = Rect.zero;
	Rect stackRect = Rect.zero;
	Rect graphRect = Rect.zero;
	Rect graphMinRect = Rect.zero;
	Rect graphMaxRect = Rect.zero;
	Rect bottomRect = Rect.zero ;
	Vector2 stackRectTopLeft;
	Rect detailRect = Rect.zero;

	Vector2 scrollPosition;
	Vector2 scrollPosition2;
	Vector2 toolbarScrollPosition;

	//int 	selectedIndex = -1;
	Log selectedLog;

	float toolbarOldDrag = 0;
	float oldDrag;
	float oldDrag2;
	int startIndex;

	//calculate what is the currentLog : collapsed or not , hide or view warnings ......
	void calculateCurrentLog()
	{
		bool filter = !string.IsNullOrEmpty(filterText);
		string _filterText = "";
		if (filter)
			_filterText = filterText.ToLower();
		currentLog.Clear();
		if (collapse) {
			for (int i = 0; i < collapsedLogs.Count; i++) {
				Log log = collapsedLogs[i];
				if (log.logType == _LogType.Log && !showLog)
					continue;
				if (log.logType == _LogType.Warning && !showWarning)
					continue;
				if (log.logType == _LogType.Error && !showError)
					continue;
				if (log.logType == _LogType.Assert && !showError)
					continue;
				if (log.logType == _LogType.Exception && !showError)
					continue;

				if (filter) {
					if (log.condition.ToLower().Contains(_filterText))
						currentLog.Add(log);
				}
				else {
					currentLog.Add(log);
				}
			}
		}
		else {
			for (int i = 0; i < logs.Count; i++) {
				Log log = logs[i];
				if (log.logType == _LogType.Log && !showLog)
					continue;
				if (log.logType == _LogType.Warning && !showWarning)
					continue;
				if (log.logType == _LogType.Error && !showError)
					continue;
				if (log.logType == _LogType.Assert && !showError)
					continue;
				if (log.logType == _LogType.Exception && !showError)
					continue;

				if (filter) {
					if (log.condition.ToLower().Contains(_filterText))
						currentLog.Add(log);
				}
				else {
					currentLog.Add(log);
				}
			}
		}

		if (selectedLog != null) {
			int newSelectedIndex = currentLog.IndexOf(selectedLog);
			if (newSelectedIndex == -1) {
				Log collapsedSelected = logsDic[selectedLog.condition][selectedLog.stacktrace];
				newSelectedIndex = currentLog.IndexOf(collapsedSelected);
				if (newSelectedIndex != -1)
					scrollPosition.y = newSelectedIndex * size.y;
			}
			else {
				scrollPosition.y = newSelectedIndex * size.y;
			}
		}
	}

	Rect countRect = Rect.zero;
	Rect timeRect = Rect.zero;
	Rect timeLabelRect = Rect.zero;
	Rect sceneRect = Rect.zero;
	Rect sceneLabelRect = Rect.zero;
	Rect memoryRect = Rect.zero;
	Rect memoryLabelRect = Rect.zero;
	Rect fpsRect = Rect.zero;
	Rect fpsLabelRect = Rect.zero;
	GUIContent tempContent = new GUIContent();

	Vector2 toolbarOldPos = new Vector2(0,0);

	void drawToolBar()
	{
		toolBarRect.x = 0f;
		toolBarRect.y = toolbarOldPos.y;
		toolBarRect.width = Screen.width;
		toolBarRect.height = size.y * 2f;
		GUI.skin = toolbarScrollerSkin;

		//toolbarScrollerSkin.verticalScrollbar.fixedWidth = 0f;
		//toolbarScrollerSkin.horizontalScrollbar.fixedHeight= 0f;

		Vector2 drag = getDrag(); // 마우스 포지션 delta
		Vector2 dragPos = drag + downPos;

		dragPos.y = Screen.height - dragPos.y; // 좌표계 수정.
		if (toolBarRect.Contains(dragPos)) // 툴바를 드래그 하는지 확인.
		{
			if (drag.y != 0)
			{
				toolbarOldPos.y = dragPos.y - toolBarRect.height / 2;
			}
		}

		//가로 스크롤
		if ((drag.x != 0) && (downPos != Vector2.zero) && (downPos.y > Screen.height - size.y * 2f)) {
			toolbarScrollPosition.x -= (drag.x - toolbarOldDrag);
		}
		toolbarOldDrag = drag.x;

		GUILayout.BeginArea(toolBarRect);
		toolbarScrollPosition = GUILayout.BeginScrollView(toolbarScrollPosition);
		GUILayout.BeginHorizontal(barStyle);

		if (GUILayout.Button(clearContent, barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			clear();
		}
		if (GUILayout.Button(collapseContent, (collapse) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			collapse = !collapse;
			calculateCurrentLog();
		}
		if (showClearOnNewSceneLoadedButton && GUILayout.Button(clearOnNewSceneContent, (clearOnNewSceneLoaded) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			clearOnNewSceneLoaded = !clearOnNewSceneLoaded;
		}

		if (showTimeButton)
		{
			if(GUILayout.Button(showTimeContent, (showTime) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
			{
				showTime = !showTime;
			}
			
			tempRect = GUILayoutUtility.GetLastRect();
			GUI.Label(tempRect, Time.realtimeSinceStartup.ToString("0.0"), lowerLeftFontStyle);
		}

		if (showSceneButton) {
			if (GUILayout.Button(showSceneContent, (showScene) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
				showScene = !showScene;
			}
			tempRect = GUILayoutUtility.GetLastRect();
			GUI.Label(tempRect, currentScene, lowerLeftFontStyle);
		}
		if (showMemButton) {
			if (GUILayout.Button(showMemoryContent, (showMemory) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
				showMemory = !showMemory;
			}
			tempRect = GUILayoutUtility.GetLastRect();
			GUI.Label(tempRect, gcTotalMemory.ToString("0.0"), lowerLeftFontStyle);
		}
		if (showFpsButton) {
			if (GUILayout.Button(showFpsContent, (showFps) ? buttonActiveStyle : barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
				showFps = !showFps;
			}
			tempRect = GUILayoutUtility.GetLastRect();
			GUI.Label(tempRect, fpsText, lowerLeftFontStyle);
		}

        if (showCopyButton)
        {
            if (GUILayout.Button(copyContent, barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
            {
                if (selectedLog == null)
                    GUIUtility.systemCopyBuffer = "No log selected";
                else
                    GUIUtility.systemCopyBuffer = selectedLog.condition + System.Environment.NewLine + System.Environment.NewLine  + selectedLog.stacktrace;
            }
        }
		
		if (GUILayout.Button("Theme", barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2)))
		{
			ToggleDarkTheme();
		}

        GUILayout.FlexibleSpace();


		string logsText = " ";
		if (collapse) {
			logsText += numOfCollapsedLogs;
		}
		else {
			logsText += numOfLogs;
		}
		string logsWarningText = " ";
		if (collapse) {
			logsWarningText += numOfCollapsedLogsWarning;
		}
		else {
			logsWarningText += numOfLogsWarning;
		}
		string logsErrorText = " ";
		if (collapse) {
			logsErrorText += numOfCollapsedLogsError;
		}
		else {
			logsErrorText += numOfLogsError;
		}

		GUILayout.BeginHorizontal((showLog) ? buttonActiveStyle : barStyle);
		if (GUILayout.Button(logContent, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			showLog = !showLog;
			calculateCurrentLog();
		}
		if (GUILayout.Button(logsText, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			showLog = !showLog;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal((showWarning) ? buttonActiveStyle : barStyle);
		if (GUILayout.Button(warningContent, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			showWarning = !showWarning;
			calculateCurrentLog();
		}
		if (GUILayout.Button(logsWarningText, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			showWarning = !showWarning;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal((showError) ? buttonActiveStyle : nonStyle);
		if (GUILayout.Button(errorContent, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			showError = !showError;
			calculateCurrentLog();
		}
		if (GUILayout.Button(logsErrorText, nonStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			showError = !showError;
			calculateCurrentLog();
		}
		GUILayout.EndHorizontal();

		if (GUILayout.Button(closeContent, barStyle, GUILayout.Width(size.x * 2), GUILayout.Height(size.y * 2))) {
			show = false;
			ReporterGUI gui = gameObject.GetComponent<ReporterGUI>();
			DestroyImmediate(gui);

			try {
				gameObject.SendMessage("OnHideReporter");
			}
			catch (System.Exception e) {
				Debug.LogException(e);
			}
		}

		GUILayout.EndHorizontal();

		GUILayout.EndScrollView();

		GUILayout.EndArea();
	}


	Rect tempRect;
	void DrawLogs()
	{
		logsRect.y += toolBarRect.y;
		GUILayout.BeginArea(logsRect, backStyle);

		GUI.skin = logScrollerSkin;
		//setStartPos();
		Vector2 drag = getDrag();

		if (drag.y != 0 && logsRect.Contains(new Vector2(downPos.x, Screen.height - downPos.y))) {
			scrollPosition.y += (drag.y - oldDrag);
		}
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);

		oldDrag = drag.y;


		int totalVisibleCount = (int)(Screen.height * 0.75f / size.y);
		int totalCount = currentLog.Count;
		/*if( totalCount < 100 )
			inGameLogsScrollerSkin.verticalScrollbarThumb.fixedHeight = 0;
		else 
			inGameLogsScrollerSkin.verticalScrollbarThumb.fixedHeight = 64;*/

		totalVisibleCount = Mathf.Min(totalVisibleCount, totalCount - startIndex);
		int index = 0;
		int beforeHeight = (int)(startIndex * size.y);
		//selectedIndex = Mathf.Clamp( selectedIndex , -1 , totalCount -1);
		if (beforeHeight > 0) {
			//fill invisible gap before scroller to make proper scroller pos
			GUILayout.BeginHorizontal(GUILayout.Height(beforeHeight));
			GUILayout.Label("---");
			GUILayout.EndHorizontal();
		}

		int endIndex = startIndex + totalVisibleCount;
		endIndex = Mathf.Clamp(endIndex, 0, totalCount);
		bool scrollerVisible = (totalVisibleCount < totalCount);
		for (int i = startIndex; (startIndex + index) < endIndex; i++) {

			if (i >= currentLog.Count)
				break;
			Log log = currentLog[i];

			if (log.logType == _LogType.Log && !showLog)
				continue;
			if (log.logType == _LogType.Warning && !showWarning)
				continue;
			if (log.logType == _LogType.Error && !showError)
				continue;
			if (log.logType == _LogType.Assert && !showError)
				continue;
			if (log.logType == _LogType.Exception && !showError)
				continue;

			if (index >= totalVisibleCount) {
				break;
			}

			GUIContent content = null;
			if (log.logType == _LogType.Log)
				content = logContent;
			else if (log.logType == _LogType.Warning)
				content = warningContent;
			else
				content = errorContent;
			//content.text = log.condition ;

			GUIStyle currentLogStyle = ((startIndex + index) % 2 == 0) ? evenLogStyle : oddLogStyle;
			if (log == selectedLog) {
				//selectedLog = log ;
				currentLogStyle = selectedLogStyle;
			}
			else {
			}

			tempContent.text = log.count.ToString();
			float w = 0f;
			if (collapse)
				w = barStyle.CalcSize(tempContent).x + 3;
			countRect.x = Screen.width - w;
			countRect.y = size.y * i;
			if (beforeHeight > 0)
				countRect.y += 8;//i will check later why
			countRect.width = w;
			countRect.height = size.y;

			if (scrollerVisible)
				countRect.x -= size.x * 2;

			Sample sample = samples[log.sampleId];
			fpsRect = countRect;
			if (showFps) {
				tempContent.text = sample.fpsText;
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				fpsRect.x -= w;
				fpsRect.width = size.x;
				fpsLabelRect = fpsRect;
				fpsLabelRect.x += size.x;
				fpsLabelRect.width = w - size.x;
			}


			memoryRect = fpsRect;
			if (showMemory) {
				tempContent.text = sample.memory.ToString("0.000") + " mb";
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				memoryRect.x -= w;
				memoryRect.width = size.x;
				memoryLabelRect = memoryRect;
				memoryLabelRect.x += size.x;
				memoryLabelRect.width = w - size.x;
			}
			sceneRect = memoryRect;
			if (showScene) {

				tempContent.text = sample.GetSceneName();
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				sceneRect.x -= w;
				sceneRect.width = size.x;
				sceneLabelRect = sceneRect;
				sceneLabelRect.x += size.x;
				sceneLabelRect.width = w - size.x;
			}
			
			timeRect = sceneRect;
			if (showTime) {
				tempContent.text = $"[{sample.time}]";
				w = currentLogStyle.CalcSize(tempContent).x + size.x;
				timeRect.x -= w;
				timeRect.x -= 5;
				timeRect.width = size.x;
				timeLabelRect = timeRect;
				timeLabelRect.x += size.x - 10;
				timeLabelRect.width = w - size.x;// + 100;
			}

			string timeLabelText = $"[{sample.time}]";

			GUILayout.BeginHorizontal(currentLogStyle);
			if (log == selectedLog) {
				GUILayout.Box(content, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y));
				GUILayout.Label(log.condition, selectedLogFontStyle);
				//GUILayout.FlexibleSpace();
				if (showTime) {
					//GUI.Box(timeRect, showTimeContent, currentLogStyle);
					GUI.Label(timeLabelRect, timeLabelText, currentLogStyle);
				}
				if (showScene) {
					GUI.Box(sceneRect, showSceneContent, currentLogStyle);
					GUI.Label(sceneLabelRect, sample.GetSceneName(), currentLogStyle);
				}
				if (showMemory) {
					GUI.Box(memoryRect, showMemoryContent, currentLogStyle);
					GUI.Label(memoryLabelRect, sample.memory.ToString("0.000") + " mb", currentLogStyle);
				}
				if (showFps) {
					GUI.Box(fpsRect, showFpsContent, currentLogStyle);
					GUI.Label(fpsLabelRect, sample.fpsText, currentLogStyle);
				}


			}
			else {
				if (GUILayout.Button(content, nonStyle, GUILayout.Width(size.x), GUILayout.Height(size.y))) {
					//selectedIndex = startIndex + index ;
					selectedLog = log;
				}
				if (GUILayout.Button(log.condition, logButtonStyle)) {
					//selectedIndex = startIndex + index ;
					selectedLog = log;
				}
				//GUILayout.FlexibleSpace();
				if (showTime) {
					//GUI.Box(timeRect, showTimeContent, currentLogStyle);
					GUI.Label(timeLabelRect, timeLabelText, currentLogStyle);
				}
				if (showScene) {
					GUI.Box(sceneRect, showSceneContent, currentLogStyle);
					GUI.Label(sceneLabelRect, sample.GetSceneName(), currentLogStyle);
				}
				if (showMemory) {
					GUI.Box(memoryRect, showMemoryContent, currentLogStyle);
					GUI.Label(memoryLabelRect, sample.memory.ToString("0.000") + " mb", currentLogStyle);
				}
				if (showFps) {
					GUI.Box(fpsRect, showFpsContent, currentLogStyle);
					GUI.Label(fpsLabelRect, sample.fpsText, currentLogStyle);
				}
			}
			if (collapse)
				GUI.Label(countRect, log.count.ToString(), barStyle);
			GUILayout.EndHorizontal();
			index++;
		}

		int afterHeight = (int)((totalCount - (startIndex + totalVisibleCount)) * size.y);
		if (afterHeight > 0) {
			//fill invisible gap after scroller to make proper scroller pos
			GUILayout.BeginHorizontal(GUILayout.Height(afterHeight));
			GUILayout.Label(" ");
			GUILayout.EndHorizontal();
		}

		GUILayout.EndScrollView();
		GUILayout.EndArea();

		bottomRect.x = 0f;
		bottomRect.y = Screen.height - size.y;
		bottomRect.width = Screen.width;
		bottomRect.height = size.y;

		drawStack();
	}

	void drawStack()
	{
		stackRect.y = logsRect.y + logsRect.height;

		if (selectedLog != null) {
			Vector2 drag = getDrag();
			if (drag.y != 0 && stackRect.Contains(new Vector2(downPos.x, Screen.height - downPos.y))) {
				scrollPosition2.y += drag.y - oldDrag2;
			}
			oldDrag2 = drag.y;

			GUILayout.BeginArea(stackRect, backStyle);
			scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2);
			Sample selectedSample = null;
			try {
				selectedSample = samples[selectedLog.sampleId];
			}
			catch (System.Exception e) {
				Debug.LogException(e);
			}

			GUILayout.BeginHorizontal();
			GUILayout.Label(selectedLog.condition, stackLabelStyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(size.y * 0.25f);
			GUILayout.BeginHorizontal();
			//GUILayout.Label(selectedLog.stacktrace, stackLabelStyle);
			GUILayout.Label(selectedLog.stacktrace, stackTraceStyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(size.y);
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
		else {
			GUILayout.BeginArea(stackRect, backStyle);
			GUILayout.EndArea();
			GUILayout.BeginArea(bottomRect, backStyle);
			GUILayout.EndArea();
		}
	}


	public void OnGUIDraw()
	{
		if (!show) {
			return;
		}

		var screen_height = Screen.height / 3;

		screenRect.x = 0;
		screenRect.y = 0;
		screenRect.width = Screen.width;
		screenRect.height = screen_height;

		getDownPos();

		logsRect.x = 0f;
		logsRect.y = size.y * 2f;
		logsRect.width = Screen.width;
		logsRect.height = screen_height * 0.75f - size.y * 2f;

		stackRectTopLeft.x = 0f;
		stackRect.x = 0f;
		stackRectTopLeft.y = screen_height * 0.75f;
		stackRect.y = screen_height * 0.75f;
		stackRect.width = Screen.width;
		stackRect.height = screen_height * 0.25f;// - size.y;


		detailRect.x = 0f;
		detailRect.y = screen_height - size.y * 3;
		detailRect.width = Screen.width;
		detailRect.height = size.y * 3;

		if (currentView == ReportView.Logs) {
			DrawLogs();
			drawToolBar();
		}

		// Display the build date in the bottomRect
		GUILayout.BeginArea(bottomRect, backStyle);
		GUILayout.Label("[Build Info] " + buildDate, nonStyle);
		GUILayout.EndArea();
	}

	List<Vector2> gestureDetector = new List<Vector2>();
	Vector2 gestureSum = Vector2.zero;
	float gestureLength = 0;
	int gestureCount = 0;
	bool isGestureDone()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touches.Length != 1) {
				gestureDetector.Clear();
				gestureCount = 0;
			}
			else {
				if (Input.touches[0].phase == TouchPhase.Canceled || Input.touches[0].phase == TouchPhase.Ended)
					gestureDetector.Clear();
				else if (Input.touches[0].phase == TouchPhase.Moved) {
					Vector2 p = Input.touches[0].position;
					if (gestureDetector.Count == 0 || (p - gestureDetector[gestureDetector.Count - 1]).magnitude > 10)
						gestureDetector.Add(p);
				}
			}
		}
		else {
			if (Input.GetMouseButtonUp(0)) {
				gestureDetector.Clear();
				gestureCount = 0;
			}
			else {
				if (Input.GetMouseButton(0)) {
					Vector2 p = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
					if (gestureDetector.Count == 0 || (p - gestureDetector[gestureDetector.Count - 1]).magnitude > 10)
						gestureDetector.Add(p);
				}
			}
		}

		if (gestureDetector.Count < 10)
			return false;
		
		// 모든 제스처 포인트가 지정된 영역 안에 있는지 확인
		for (int i = 0; i < gestureDetector.Count; i++)
		{
			Vector2 point = gestureDetector[i];
			if (!RectTransformUtility.RectangleContainsScreenPoint(gestureArea, point, parentCanvas.worldCamera))
			{
				gestureDetector.Clear();
				gestureCount = 0;
				return false;
			}
		}

		gestureSum = Vector2.zero;
		gestureLength = 0;
		Vector2 prevDelta = Vector2.zero;
		for (int i = 0; i < gestureDetector.Count - 2; i++) {

			Vector2 delta = gestureDetector[i + 1] - gestureDetector[i];
			float deltaLength = delta.magnitude;
			gestureSum += delta;
			gestureLength += deltaLength;

			float dot = Vector2.Dot(delta, prevDelta);
			if (dot < 0f) {
				gestureDetector.Clear();
				gestureCount = 0;
				return false;
			}

			prevDelta = delta;
		}
		
		//int gestureBase = (Screen.width + Screen.height) / 4;
		int gestureBase = (Screen.width + Screen.height) / 8;
		
		bool isCircleAccurate  = gestureSum.magnitude < gestureBase / 2; // 상수가 클수록 더 완벽한 원형이어야 함.
		if (gestureLength > gestureBase && isCircleAccurate) {
			gestureDetector.Clear();
			gestureCount++;
			Debug.Log($"GestureCount : {gestureCount} / Screen : {Screen.width}");
			Debug.LogWarning($"GestureCount : {gestureCount} / Screen : {Screen.width}");
			Debug.LogError($"GestureCount : {gestureCount} / Screen : {Screen.width}");
			if (gestureCount >= numOfCircleToShow)
				return true;
		}

		return false;
	}

	float lastClickTime = -1;
	bool isDoubleClickDone()
	{
		if (Application.platform == RuntimePlatform.Android ||
		   Application.platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touches.Length != 1) {
				lastClickTime = -1;
			}
			else {
				if (Input.touches[0].phase == TouchPhase.Began) {
					if (lastClickTime == -1)
						lastClickTime = Time.realtimeSinceStartup;
					else if (Time.realtimeSinceStartup - lastClickTime < 0.2f) {
						lastClickTime = -1;
						return true;
					}
					else {
						lastClickTime = Time.realtimeSinceStartup;
					}
				}
			}
		}
		else {
			if (Input.GetMouseButtonDown(0)) {
				if (lastClickTime == -1)
					lastClickTime = Time.realtimeSinceStartup;
				else if (Time.realtimeSinceStartup - lastClickTime < 0.2f) {
					lastClickTime = -1;
					return true;
				}
				else {
					lastClickTime = Time.realtimeSinceStartup;
				}
			}
		}
		return false;
	}

	//calculate  pos of first click on screen
	Vector2 startPos;

	Vector2 downPos;
	Vector2 getDownPos()
	{
		if (Application.platform == RuntimePlatform.Android ||
		   Application.platform == RuntimePlatform.IPhonePlayer) {

			if (Input.touches.Length == 1 && Input.touches[0].phase == TouchPhase.Began) {
				downPos = Input.touches[0].position;
				return downPos;
			}
		}
		else {
			if (Input.GetMouseButtonDown(0)) {
				downPos.x = Input.mousePosition.x;
				downPos.y = Input.mousePosition.y;
				return downPos;
			}
		}

		return Vector2.zero;
	}
	//calculate drag amount , this is used for scrolling

	Vector2 mousePosition;
	Vector2 getDrag()
	{
		if (Application.platform == RuntimePlatform.Android ||
			Application.platform == RuntimePlatform.IPhonePlayer) {
			if (Input.touches.Length != 1) {
				return Vector2.zero;
			}
			return Input.touches[0].position - downPos;
		}
		else {
			if (Input.GetMouseButton(0)) {
				mousePosition = Input.mousePosition;
				return mousePosition - downPos;
			}
			else {
				return Vector2.zero;
			}
		}
	}

	//calculate the start index of visible log
	void calculateStartIndex()
	{
		startIndex = (int)(scrollPosition.y / size.y);
		startIndex = Mathf.Clamp(startIndex, 0, currentLog.Count);
	}

	// For FPS Counter
	private int frames = 0;
	private bool firstTime = true;
	private float lastUpdate = 0f;
	private const int requiredFrames = 10;
	private const float updateInterval = 0.25f;

#if UNITY_CHANGE1
	float lastUpdate2 = 0;
#endif

	void doShow()
	{
		show = true;
		currentView = ReportView.Logs;
		gameObject.AddComponent<ReporterGUI>();

		try {
			gameObject.SendMessage("OnShowReporter");
		}
		catch (System.Exception e) {
			Debug.LogException(e);
		}
	}

	//제스처 인식 후 GUI Drawing
	void Update()
	{
		fpsText = fps.ToString("0.000");
		gcTotalMemory = (((float)System.GC.GetTotalMemory(false)) / 1024 / 1024);
		//addSample();

#if UNITY_CHANGE3
		int sceneIndex = SceneManager.GetActiveScene().buildIndex ;
		if( sceneIndex != -1 && string.IsNullOrEmpty( scenes[sceneIndex] ))
			scenes[ SceneManager.GetActiveScene().buildIndex ] = SceneManager.GetActiveScene().name ;
#else
		int sceneIndex = Application.loadedLevel;
		if (sceneIndex != -1 && string.IsNullOrEmpty(scenes[Application.loadedLevel]))
			scenes[Application.loadedLevel] = Application.loadedLevelName;
#endif

		calculateStartIndex();
		if (!show && isGestureDone()) {
			doShow();
		}


		if (threadedLogs.Count > 0) {
			lock (threadedLogs) {
				for (int i = 0; i < threadedLogs.Count; i++) {
					Log l = threadedLogs[i];
					AddLog(l.condition, l.stacktrace, (LogType)l.logType);
				}
				threadedLogs.Clear();
			}
		}

#if UNITY_CHANGE1
		float elapsed2 = Time.realtimeSinceStartup - lastUpdate2;
		if (elapsed2 > 1) {
			lastUpdate2 = Time.realtimeSinceStartup;
			//be sure no body else take control of log 
			Application.RegisterLogCallback (new Application.LogCallback (CaptureLog));
			Application.RegisterLogCallbackThreaded (new Application.LogCallback (CaptureLogThread));
		}
#endif

		// FPS Counter
		if (firstTime) {
			firstTime = false;
			lastUpdate = Time.realtimeSinceStartup;
			frames = 0;
			return;
		}
		frames++;
		float dt = Time.realtimeSinceStartup - lastUpdate;
		if (dt > updateInterval && frames > requiredFrames) {
			fps = (float)frames / dt;
			lastUpdate = Time.realtimeSinceStartup;
			frames = 0;
		}
	}

	void AddLog(string condition, string stacktrace, LogType type)
	{
		float memUsage = 0f;
		string _condition = "";
		if (cachedString.ContainsKey(condition)) {
			_condition = cachedString[condition];
		}
		else {
			_condition = condition;
			cachedString.Add(_condition, _condition);
			memUsage += (string.IsNullOrEmpty(_condition) ? 0 : _condition.Length * sizeof(char));
			memUsage += System.IntPtr.Size;
		}
		string _stacktrace = "";
		if (cachedString.ContainsKey(stacktrace)) {
			_stacktrace = cachedString[stacktrace];
		}
		else {
			_stacktrace = stacktrace;
			cachedString.Add(_stacktrace, _stacktrace);
			memUsage += (string.IsNullOrEmpty(_stacktrace) ? 0 : _stacktrace.Length * sizeof(char));
			memUsage += System.IntPtr.Size;
		}
		bool newLogAdded = false;

		addSample();
		Log log = new Log() { logType = (_LogType)type, condition = _condition, stacktrace = _stacktrace, sampleId = samples.Count - 1 };
		memUsage += log.GetMemoryUsage();
		//memUsage += samples.Count * 13 ;

		logsMemUsage += memUsage / 1024 / 1024;

		if (TotalMemUsage > maxSize) {
			clear();
			Debug.Log("Memory Usage Reach" + maxSize + " mb So It is Cleared");
			return;
		}

		bool isNew = false;
		//string key = _condition;// + "_!_" + _stacktrace ;
		if (logsDic.ContainsKey(_condition, stacktrace)) {
			isNew = false;
			logsDic[_condition][stacktrace].count++;
		}
		else {
			isNew = true;
			collapsedLogs.Add(log);
			logsDic[_condition][stacktrace] = log;

			if (type == LogType.Log)
				numOfCollapsedLogs++;
			else if (type == LogType.Warning)
				numOfCollapsedLogsWarning++;
			else
				numOfCollapsedLogsError++;
		}

		if (type == LogType.Log)
			numOfLogs++;
		else if (type == LogType.Warning)
			numOfLogsWarning++;
		else
			numOfLogsError++;


		logs.Add(log);
		if (!collapse || isNew) {
			bool skip = false;
			if (log.logType == _LogType.Log && !showLog)
				skip = true;
			if (log.logType == _LogType.Warning && !showWarning)
				skip = true;
			if (log.logType == _LogType.Error && !showError)
				skip = true;
			if (log.logType == _LogType.Assert && !showError)
				skip = true;
			if (log.logType == _LogType.Exception && !showError)
				skip = true;

			if (!skip) {
				if (string.IsNullOrEmpty(filterText) || log.condition.ToLower().Contains(filterText.ToLower())) {
					currentLog.Add(log);
					newLogAdded = true;
				}
			}
		}

		if (newLogAdded) {
			calculateStartIndex();
			int totalCount = currentLog.Count;
			int totalVisibleCount = (int)(Screen.height * 0.75f / size.y);
			if (startIndex >= (totalCount - totalVisibleCount))
				scrollPosition.y += size.y;
		}

		try {
			gameObject.SendMessage("OnLog", log);
		}
		catch (System.Exception e) {
			Debug.LogException(e);
		}
	}

	List<Log> threadedLogs = new List<Log>();
	void CaptureLogThread(string condition, string stacktrace, LogType type)
	{
		Log log = new Log() { condition = condition, stacktrace = stacktrace, logType = (_LogType)type };
		lock (threadedLogs) {
			threadedLogs.Add(log);
		}
	}

#if !UNITY_CHANGE3
    class Scene
    {
    }
    class LoadSceneMode
    {
    }
    void OnLevelWasLoaded()
    {
        _OnLevelWasLoaded( null );
    }
#endif
    //new scene is loaded
    void _OnLevelWasLoaded( Scene _null1 , LoadSceneMode _null2 )
	{
		if (clearOnNewSceneLoaded)
			clear();

#if UNITY_CHANGE3
		currentScene = SceneManager.GetActiveScene().name ;
		Debug.Log( "Scene " + SceneManager.GetActiveScene().name + " is loaded");
#else
		currentScene = Application.loadedLevelName;
		Debug.Log("Scene " + Application.loadedLevelName + " is loaded");
#endif
	}

	//save user config
	void OnApplicationQuit()
	{
	}

	//read build information 
	IEnumerator readInfo()
	{
		string prefFile = "build_info"; 
		string url = prefFile; 

		if (prefFile.IndexOf("://") == -1) {
			string streamingAssetsPath = Application.streamingAssetsPath;
			if (streamingAssetsPath == "")
				streamingAssetsPath = Application.dataPath + "/StreamingAssets/";
			url = System.IO.Path.Combine(streamingAssetsPath, prefFile);
		}

		//if (Application.platform != RuntimePlatform.OSXWebPlayer && Application.platform != RuntimePlatform.WindowsWebPlayer)
			if (!url.Contains("://"))
				url = "file://" + url;


		// float startTime = Time.realtimeSinceStartup;
#if UNITY_CHANGE4
		UnityWebRequest www = UnityWebRequest.Get(url);
		yield return www.SendWebRequest();
#else
		WWW www = new WWW(url);
		yield return www;
#endif

		if (!string.IsNullOrEmpty(www.error)) {
			Debug.LogError(www.error);
		}
		else {
#if UNITY_CHANGE4
			buildDate = www.downloadHandler.text;
#else
			buildDate = www.text;
#endif
		}

		yield break;
	}
}


