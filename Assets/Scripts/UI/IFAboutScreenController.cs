// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFAboutScreenController : MonoBehaviour {
	
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	
	private UIPanel mPanel;
	
	public static IFAboutScreenController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.AboutScreenPrefab == null) {
			return IFAboutScreenController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.AboutScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}
		
		return go.GetComponent<IFAboutScreenController>();
	}
	
	public static IFAboutScreenController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFAboutScreenController>();
	}

	public static IFAboutScreenController Create()
	{
		return Create("About Screen");
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
	
	void Start()
	{
		foreach(IFContainSysFontLabel contain in panel.GetComponentsInChildren<IFContainSysFontLabel>()) {
			if(contain.gameObject.name.Equals("About Text Label")) {
				if(Platforms.IsiOS) {
					contain.padding = new Vector2(80f, 10f);
				} else {
					contain.padding = new Vector2(120f, 10f);
				}
				
			}
		}
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}
	
	void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}

	void ShowFeedbackScreen()
	{
		IFGameManager.SharedManager.TransitionToFeedbackScreen(IFGameManager.TransitionDirection.LeftToRight, () => {
			IFGameManager.SharedManager.TransitionToAboutScreen(IFGameManager.TransitionDirection.RightToLeft, () => true);
			return false;
		});
	}

	void FeedbackButtonWasTapped()
	{
		if(!IFGameManager.IsLoggedIn) {
			IFGameManager.SharedManager.CreateAnonymousUser((success, error) => {
				ShowFeedbackScreen();
			});
			return;
		}
		ShowFeedbackScreen();
	}
	
	void HelpButtonWasTapped()
	{
		IFGameManager.SharedManager.TransitionToHelpScreen(IFGameManager.TransitionDirection.LeftToRight, () => {
			IFGameManager.SharedManager.TransitionToAboutScreen(IFGameManager.TransitionDirection.RightToLeft, () => true);
			return false;
		});
	}
	
	void OpenWebsite()
	{
		Application.OpenURL("http://www.zingbazi.com/");
	}
}
