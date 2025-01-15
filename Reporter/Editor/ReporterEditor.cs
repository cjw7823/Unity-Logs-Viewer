using UnityEngine;
using UnityEditor;

using System.IO;
using System.Drawing;


public class ReporterEditor : Editor
{
	[MenuItem("Reporter/Create")]
	public static void CreateReporter()
	{
		const int ReporterExecOrder = -12000;
		GameObject reporterObj = new GameObject();
		reporterObj.name = "Reporter";
		Reporter reporter = reporterObj.AddComponent<Reporter>();
		reporterObj.AddComponent<ReporterMessageReceiver>();
		Canvas canvas = reporterObj.AddComponent<Canvas>();
		canvas.vertexColorAlwaysGammaSpace = true;
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;

		GameObject childObj = new GameObject();
		var image = childObj.AddComponent<UnityEngine.UI.Image>();
		image.color = new UnityEngine.Color(149/255f, 63/255f, 128/255f, 100/255f);
		var rect = childObj.GetComponent<RectTransform>();
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 4);
		rect.position = new Vector3(0,0,0);
		childObj.name = "GestureArea";
		childObj.transform.SetParent(reporterObj.transform, false);

		reporter.GestureArea = rect;

		//reporterObj.AddComponent<TestReporter>();
		
		// Register root object for undo.
		Undo.RegisterCreatedObjectUndo(reporterObj, "Create Reporter Object");

		MonoScript reporterScript = MonoScript.FromMonoBehaviour(reporter);
		string reporterPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(reporterScript));

		if (MonoImporter.GetExecutionOrder(reporterScript) != ReporterExecOrder) {
			MonoImporter.SetExecutionOrder(reporterScript, ReporterExecOrder);
			//Debug.Log("Fixing exec order for " + reporterScript.name);
		}

		reporter.images = new Images();
		reporter.images.clearImage           = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/clear.png"), typeof(Texture2D));
		reporter.images.collapseImage        = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/collapse.png"), typeof(Texture2D));
		reporter.images.clearOnNewSceneImage = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/clearOnSceneLoaded.png"), typeof(Texture2D));
		reporter.images.showTimeImage        = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/timer_1.png"), typeof(Texture2D));
		reporter.images.showSceneImage       = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/UnityIcon.png"), typeof(Texture2D));
		reporter.images.userImage            = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/user.png"), typeof(Texture2D));
		reporter.images.showMemoryImage      = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/memory.png"), typeof(Texture2D));
		reporter.images.softwareImage        = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/software.png"), typeof(Texture2D));
		reporter.images.dateImage            = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/date.png"), typeof(Texture2D));
		reporter.images.showFpsImage         = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/fps.png"), typeof(Texture2D));
		//reporter.images.graphImage           = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/chart.png"), typeof(Texture2D));
		reporter.images.infoImage            = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/info.png"), typeof(Texture2D));
        reporter.images.saveLogsImage        = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/Save.png"), typeof(Texture2D));
        reporter.images.searchImage          = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/search.png"), typeof(Texture2D));
        reporter.images.copyImage            = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/copy.png"), typeof(Texture2D));
        reporter.images.closeImage           = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/close.png"), typeof(Texture2D));
		reporter.images.buildFromImage       = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/buildFrom.png"), typeof(Texture2D));
		reporter.images.systemInfoImage      = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/ComputerIcon.png"), typeof(Texture2D));
		reporter.images.graphicsInfoImage    = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/graphicCard.png"), typeof(Texture2D));
		reporter.images.backImage            = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/back.png"), typeof(Texture2D));
		reporter.images.logImage             = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/log_icon.png"), typeof(Texture2D));
		reporter.images.warningImage         = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/warning_icon.png"), typeof(Texture2D));
		reporter.images.errorImage           = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/error_icon.png"), typeof(Texture2D));
		reporter.images.barImage_White       = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/bar_white.png"), typeof(Texture2D));
		reporter.images.barImage_Dark        = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/bar_dark.png"), typeof(Texture2D));
		reporter.images.button_activeImage_White   = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/button_active_white.png"), typeof(Texture2D));
		reporter.images.button_activeImage_Dark    = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/button_active_dark.png"), typeof(Texture2D));
		reporter.images.even_logImage_White  = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/even_log_white.png"), typeof(Texture2D));
		reporter.images.even_logImage_Dark   = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/even_log_dark.png"), typeof(Texture2D));
		reporter.images.odd_logImage_white   = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/odd_log_white.png"), typeof(Texture2D));
		reporter.images.odd_logImage_Dark    = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/odd_log_dark.png"), typeof(Texture2D));
		reporter.images.selectedImage        = (Texture2D)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/selected.png"), typeof(Texture2D));

		reporter.images.reporterScrollerSkin = (GUISkin)AssetDatabase.LoadAssetAtPath(Path.Combine(reporterPath, "Images/reporterScrollerSkin.guiskin"), typeof(GUISkin));
	}
}

public class ReporterModificationProcessor : UnityEditor.AssetModificationProcessor
{
	[InitializeOnLoad]
	public class BuildInfo
	{
		static BuildInfo()
		{
			EditorApplication.update += Update;
		}

		static bool isCompiling = true;
		static void Update()
		{
          
			if (!EditorApplication.isCompiling && isCompiling) {
				//Debug.Log("Finish Compile");
				if (!Directory.Exists(Application.dataPath + "/StreamingAssets")) {
					Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");
				}
				string info_path = Application.dataPath + "/StreamingAssets/build_info"; 
				StreamWriter build_info = new StreamWriter(info_path);
				build_info.Write("Build from " + SystemInfo.deviceName + " at " + System.DateTime.Now.ToString());
				build_info.Close();
			}

			isCompiling = EditorApplication.isCompiling;
		}
	}
}
