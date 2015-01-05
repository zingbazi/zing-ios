using UnityEditor;
using UnityEngine;

public abstract class NonProBuildWindow : TestFlightWindow
{
	public static NonProBuildWindow CurrentBuildWindow {get; private set;}
	
	public virtual void OnEnable()
	{
		if(CurrentBuildWindow)
			CurrentBuildWindow.Close();
		
		CurrentBuildWindow = this;
	}
	
	public virtual void OnDisable()
	{
		if(CurrentBuildWindow == this)
			CurrentBuildWindow = null;
	}
	
	public abstract void OnBuild(UnityEditor.BuildTarget target, string pathToBuiltProject);
	
	[UnityEditor.Callbacks.PostProcessBuildAttribute]
	public static void PostBuildPlayerCallback(UnityEditor.BuildTarget target, string pathToBuiltProject)
	{
		if(CurrentBuildWindow)
		{
			CurrentBuildWindow.OnBuild(target, pathToBuiltProject);
		}
	}
}