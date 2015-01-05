// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;

public class IFAlertViewController : MonoBehaviour {

	public UIPanel panel;
	public GameObject alertWindow;
	public UISprite mask;
	public UISysFontLabel messageLabel;
	public UISysFontLabel titleLabel;
	public UISprite windowBackground;
	public UISysFontLabel buttonLabel;
	
	// Per-side. Includes the shadow and other border elements
	public float textWidthPadding = 50f;
	
	public delegate void AlertViewDidClose(IFAlertViewController controller, bool okWasSelected);
	public event AlertViewDidClose AlertViewDidCloseEvent;
	
	private Transform cachedTransform;
	
	public static IFAlertViewController Instance
	{
		get
		{
			IFAlertViewController cached = IFGameManager.LoadableAssets.cachedAlertViewController;
			if(cached == null) {
				cached = CreateFromPrefab();
				IFGameManager.LoadableAssets.cachedAlertViewController = cached;
			}
			return cached;
		}
	}
	
	public static IFAlertViewController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.AlertViewPrefab == null) {
			return IFAlertViewController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.AlertViewPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFAlertViewController>();
	}
	
	public static IFAlertViewController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFAlertViewController>();
	}

	public static IFAlertViewController Create()
	{
		return Create("Alert View");
	}
	void Awake()
	{
		if(panel == null) {
			panel = GetComponentInChildren<UIPanel>();	
		}
		cachedTransform = GetComponent<Transform>();
	}
	
	public static IFAlertViewController ShowAlert(string message)
	{
		Instance.Show(message, null, Localization.Localize("OK"));
		return Instance;
	}
	
	public static IFAlertViewController ShowAlert(string message, string title)
	{
		Instance.Show(message, title, Localization.Localize("OK"));
		return Instance;
	}

	public static IFAlertViewController ShowAlert(string message, string title, string buttonText)
	{
		Instance.Show(message, title, buttonText);
		return Instance;
	}
	
	public static IFAlertViewController ShowAlert(string message, string title, string buttonText, AlertViewDidClose delegateCallback)
	{
		Instance.Show(message, title, buttonText, delegateCallback);
		return Instance;
	}
	
	public void Show(string message, string title, string buttonText, AlertViewDidClose delegateCallback)
	{
		if(delegateCallback != null) {
			AlertViewDidClose callback = (IFAlertViewController controller, bool okWasSelected) => {
				AlertViewDidCloseEvent -= callback;
				delegateCallback(controller, okWasSelected);
			};
			AlertViewDidCloseEvent += callback;
		}
		Show(message, title, buttonText);
	}
	
	public void Show(string message, string title, string buttonText)
	{
		cachedTransform.parent = IFGameManager.SharedManager.mainPanel.cachedTransform;
		cachedTransform.localScale = Vector3.one;
		cachedTransform.localRotation = Quaternion.identity;
		cachedTransform.localPosition = Vector3.zero;
		
		Vector3 windowSize = windowBackground.cachedTransform.localScale;
		float maxTextWidth = windowSize.x - textWidthPadding * 2f;
		int maxTextWidthPixels = Mathf.RoundToInt(maxTextWidth);
		messageLabel.MaxWidthPixels = maxTextWidthPixels;
		titleLabel.MaxWidthPixels = maxTextWidthPixels;
		
		gameObject.SetActive(true);
		
		buttonLabel.Text = buttonText;
		messageLabel.Text = message;
		if(title != null) {
			titleLabel.gameObject.SetActive(true);
			titleLabel.Text = title;
		} else {
			titleLabel.gameObject.SetActive(false);
		}
		
		NGUITools.AddWidgetCollider(mask.gameObject);
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
		TweenScale scaleWindow = TweenScale.Begin(alertWindow, .4f, Vector3.one);
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
		TweenScale scaleUp = TweenScale.Begin(alertWindow, .25f, Vector3.one);
		scaleUp.from = Vector3.one;
		scaleUp.to = new Vector3(1.2f, 1.2f, 1.2f);
		scaleUp.method = UITweener.Method.EaseIn;
		scaleUp.onFinished = (UITweener t) => {
			TweenAlpha fadeOut = TweenAlpha.Begin(panel.gameObject, .15f, 0f);
			fadeOut.method = UITweener.Method.EaseIn;
			
			TweenScale scaleDown = TweenScale.Begin(alertWindow, .15f, Vector3.one);
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
	
	void Update()
	{
		Vector3 windowSize = windowBackground.cachedTransform.localScale;
		Vector3 messageSize = messageLabel.cachedTransform.localScale;
		Vector3 titleSize = titleLabel.cachedTransform.localScale;
		
		float newHeight = Mathf.Max(messageSize.y, 240f) + titleSize.y + 220f;
		if(windowSize.y != newHeight) {
			windowSize.y = newHeight;
			windowBackground.cachedTransform.localScale = windowSize;
		}
	}
	
	public void Dismiss()
	{
		gameObject.SetActive(false);
		panel.alpha = 1f;
		transform.localScale = Vector3.one;
		transform.localRotation = Quaternion.identity;
	}
	
	public void OKButtonTapped()
	{
		DismissAnimated(() => {
			if(AlertViewDidCloseEvent != null) {
				AlertViewDidCloseEvent(this, true);
			}
		});
	}
}
