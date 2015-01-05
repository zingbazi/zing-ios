// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFHelpScreenController : MonoBehaviour {

	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	
	private UIPanel mPanel;
	
	public static IFHelpScreenController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.HelpScreenPrefab == null) {
			return IFHelpScreenController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.HelpScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}
		
		return go.GetComponent<IFHelpScreenController>();
	}
	
	public static IFHelpScreenController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFHelpScreenController>();
	}

	public static IFHelpScreenController Create()
	{
		return Create("Help Screen");
	}
	
	public UIPanel panel
	{
		get
		{
			if(mPanel == null) {
				mPanel = GetComponentInChildren<UIPanel>();
			}
			return mPanel;
		}
	}
	
	void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
}
