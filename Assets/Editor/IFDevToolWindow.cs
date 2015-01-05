// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class IFDevToolWindow : EditorWindow {
	public Transform mainPanel;
	public GameObject homeScreen;
	public GameObject background;
	
	public struct SceneObjectActiveState
	{
		public GameObject obj;
		public bool wasActive;
		
		public SceneObjectActiveState(GameObject o, bool active)
		{
			obj = o;
			wasActive = active;
		}
	}
	public List<SceneObjectActiveState> lastActive;
	
	private static IFDevToolWindow instance;
    

//    [MenuItem ("IFES/Open Dev Tool Window %#d")]
    static void Init () {
        // Get existing open window or if none, make a new one:
        instance = (IFDevToolWindow)EditorWindow.GetWindow (typeof (IFDevToolWindow));
		instance.title = "IFES Tool";
    }
    
    void OnGUI () {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Main Panel Object", EditorStyles.label);
		mainPanel = (Transform)EditorGUILayout.ObjectField(mainPanel, typeof(Transform), true, GUILayout.Width(150f));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Home Screen Root", EditorStyles.label);
		homeScreen = (GameObject)EditorGUILayout.ObjectField(homeScreen, typeof(GameObject), true, GUILayout.Width(150f));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Background", EditorStyles.label);
		background = (GameObject)EditorGUILayout.ObjectField(background, typeof(GameObject), true, GUILayout.Width(150f));
		EditorGUILayout.EndHorizontal();
    }
	
//	[MenuItem("IFES/Prepare Scene for Play %#r")]
	public static void PrepareForPlay()
	{
		if(instance != null) {
			instance.lastActive = new List<SceneObjectActiveState>();
			Transform root = instance.mainPanel;
			for(int i = 0; i < root.childCount; i++) {
				GameObject kid = root.GetChild(i).gameObject;
				instance.lastActive.Add(new SceneObjectActiveState(kid, kid.activeSelf));
				if(ReferenceEquals(kid, instance.homeScreen) || ReferenceEquals(kid, instance.background)) {
					kid.SetActive(true);
				} else {
					Debug.Log(kid.name + " now inactive");
					kid.SetActive(false);	
				}
			}
		}
		EditorApplication.SaveScene();
//		EditorApplication.isPlaying = true;
//		EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
//		instance.homeScreen.transform.parent.get
	}
	
//	public static void PlaymodeStateChanged()
//	{
//		if(!EditorApplication.isPlayingOrWillChangePlaymode) {
//			EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
//			RestoreLastSceneState();
//			instance.lastActive = null;
//		}
//	}
	
//	[MenuItem("IFES/Restore Last Scene State %&r")]
	public static void RestoreLastSceneState()
	{
		if(instance != null) {
			foreach(SceneObjectActiveState stateObj in instance.lastActive) {
				Debug.Log("Setting "+stateObj.obj.name+" back to "+ (stateObj.wasActive ? "active" : "inactive"));
				stateObj.obj.SetActive(stateObj.wasActive);
			}
		}
	}
}