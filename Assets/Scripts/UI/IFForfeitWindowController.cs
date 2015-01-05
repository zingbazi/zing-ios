// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;

public class IFForfeitWindowController : MonoBehaviour {
	
	public UIPanel panel;
	public GameObject window;
	public UISprite mask;
	public UISprite windowBackground;
	public UILabel gameCountLabel;
	
	public enum ButtonSelection { CurrentGames, Forfeit, Resume }
	public delegate void ForfeitWindowDidClose(ButtonSelection selectedButton);
	public ForfeitWindowDidClose ForfeitWindowDidCloseDelegate;
	
	private Transform cachedTransform;
	
	public static IFForfeitWindowController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.ForfeitWindowPrefab == null) {
			return Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.ForfeitWindowPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFForfeitWindowController>();
	}
	
	public static IFForfeitWindowController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFForfeitWindowController>();
	}

	public static IFForfeitWindowController Create()
	{
		return Create("Forfeit Window");
	}
	void Awake()
	{
		if(panel == null) {
			panel = GetComponentInChildren<UIPanel>();	
		}
		cachedTransform = GetComponent<Transform>();
	}
	
	public void Show(ForfeitWindowDidClose delegateCallback)
	{
		ForfeitWindowDidCloseDelegate = delegateCallback;
		Show();
	}
	
	public void Show()
	{
		cachedTransform.parent = IFGameManager.SharedManager.mainPanel.cachedTransform;
		cachedTransform.localScale = Vector3.one;
		cachedTransform.localRotation = Quaternion.identity;
		cachedTransform.localPosition = Vector3.zero;
		
		gameCountLabel.text = IFChallenge.CountOfPlayableChallenges().ToString();
		
		gameObject.SetActive(true);
		
		NGUITools.AddWidgetCollider(mask.gameObject);
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
		TweenScale scaleWindow = TweenScale.Begin(window, .4f, Vector3.one);
		scaleWindow.from = new Vector3(.01f, .01f, 1f);
		scaleWindow.to = Vector3.one;
		scaleWindow.method = UITweener.Method.BounceIn;
		scaleWindow.onFinished += (tween) => {
			IFUtils.SetEnabledAllCollidersInChildren(gameObject, true);
		};
		
		TweenAlpha fadeIn = TweenAlpha.Begin(panel.gameObject, .2f, 1f);
		fadeIn.from = 0f;
		fadeIn.to = 1f;
		fadeIn.method = UITweener.Method.EaseOut;
		IFGameManager.SharedManager.UpdateSoundConfiguration(true);
	}
	
	public void DismissAnimated(Action callback)
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
		TweenScale scaleUp = TweenScale.Begin(window, .25f, Vector3.one);
		scaleUp.from = Vector3.one;
		scaleUp.to = new Vector3(1.2f, 1.2f, 1.2f);
		scaleUp.method = UITweener.Method.EaseIn;
		scaleUp.onFinished = (UITweener t) => {
			TweenAlpha fadeOut = TweenAlpha.Begin(panel.gameObject, .15f, 0f);
			fadeOut.method = UITweener.Method.EaseIn;

			TweenScale scaleDown = TweenScale.Begin(window, .15f, Vector3.one);
			scaleDown.from = scaleUp.to;
			scaleDown.to = new Vector3(.25f, .25f, 1f);
			scaleDown.method = UITweener.Method.EaseOut;
			scaleDown.onFinished = (UITweener t2) => {
				if(callback != null) {
					callback();
				}
				Dismiss();
			};
		};
	}
	
	public void Dismiss()
	{
		gameObject.SetActive(false);
		panel.alpha = 1f;
		transform.localScale = Vector3.one;
		transform.localRotation = Quaternion.identity;

		NGUITools.Destroy(gameObject);
	}
	
	public void ResumeButtonTapped()
	{
		DismissAnimated(() => {
			if(ForfeitWindowDidCloseDelegate != null) {
				ForfeitWindowDidCloseDelegate(ButtonSelection.Resume);	
			}
		});
	}
	
	public void CurrentGamesButtonTapped()
	{
		DismissAnimated(() => {
			if(ForfeitWindowDidCloseDelegate != null) {
				ForfeitWindowDidCloseDelegate(ButtonSelection.CurrentGames);	
			}
		});
	}
	
	public void ForfeitButtonTapped()
	{
		DismissAnimated(() => {
			if(ForfeitWindowDidCloseDelegate != null) {
				ForfeitWindowDidCloseDelegate(ButtonSelection.Forfeit);
			}
		});
	}
}
