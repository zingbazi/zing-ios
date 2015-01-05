using UnityEditor;
using UnityEngine;

public class TestFlightNonProSchemaWindow : NonProBuildWindow
{
	public delegate void SchemaEvent(TestFlightXCodeSchemas schema);
	public static event SchemaEvent OnSchemasLoaded = delegate {};

	Texture instructionTex = null;

	public static void ShowShemaWindow()
	{
		var window = GetWindow<TestFlightNonProSchemaWindow>(true);
		window.title = "Unity Basic Detected - Manual build step required";
		var preferences = TestFlightPreferences.Load();
		window.instructionTex = TestFlightResources.GetTextureResource("NonProBuildInstructions.jpg");
		window.minSize  = new Vector2(window.instructionTex.width, window.instructionTex.height); 
		window.position = new Rect(10, 10, window.minSize.x, window.minSize.y);
		
		EditorUserBuildSettings.SetBuildLocation(BuildTarget.iPhone, preferences.teamPrefs.buildPath);
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iPhone);
	}
	
	public override void OnBuild (BuildTarget target, string pathToBuiltProject)
	{
		if(target != BuildTarget.iPhone)
			return;
		
		var schemas = TestFlightXCodeSchemas.Enumerate(pathToBuiltProject, true);
		if(schemas != null)
		{
			OnSchemasLoaded(schemas);
		}
		Close();
	}
	
	public void OnGUI()
	{	
		if(instructionTex)
			GUI.DrawTexture(new Rect(0,Screen.height-instructionTex.height,instructionTex.width, instructionTex.height), instructionTex);		
	}
}