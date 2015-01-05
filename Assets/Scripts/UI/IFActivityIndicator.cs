// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IFActivityIndicator : MonoBehaviour {

	public Transform cachedTransform;
	private static List<IFActivityIndicator> liveIndicators = new List<IFActivityIndicator>();
	
	private UISpriteAnimation spriteAnimation;
	private UISprite sprite;
	private UIPanel parentPanel;
	private bool usesOrphanPanel = false;
	private bool wasDismissed;
		
	public static IFActivityIndicator Create()
	{
		GameObject go = Instantiate(IFGameManager.LoadableAssets.ActivityIndicatorPrefab, new Vector3(0f, 0f, -10f), Quaternion.identity) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFActivityIndicator>();
	}
	
	public static IFActivityIndicator CreateActivityIndicatorForPanel(UIPanel panel)
	{
		IFActivityIndicator indicator = Create();
		indicator.cachedTransform.parent = panel.cachedTransform;
		indicator.cachedTransform.localPosition = new Vector3(0f, 0f, -10f);
		indicator.cachedTransform.localScale = Vector3.one;
		indicator.parentPanel = panel;
		return indicator;
	}
	
	public static IFActivityIndicator CreateFloatingActivityIndicator()
	{
		IFActivityIndicator indicator = Create();
		GameObject panelGO = new GameObject("Activity Indicator Panel");
		indicator.parentPanel = panelGO.AddComponent<UIPanel>();
		indicator.cachedTransform.parent = indicator.parentPanel.cachedTransform;
		indicator.parentPanel.cachedTransform.parent = IFGameManager.SharedManager.mainPanel.cachedTransform;
		indicator.cachedTransform.localPosition = new Vector3(0f, 0f, -10f);
		indicator.cachedTransform.localScale = Vector3.one;
		indicator.parentPanel.cachedTransform.localPosition = Vector3.zero;
		indicator.parentPanel.cachedTransform.localScale = Vector3.one;
		indicator.usesOrphanPanel = true;
		return indicator;
	}
	
	public static IFActivityIndicator[] LiveIndicators
	{
		get
		{
			return liveIndicators.ToArray();
		}
	}
	
	void Awake()
	{
		cachedTransform = GetComponent<Transform>();
		sprite = GetComponentInChildren<UISprite>();
		spriteAnimation = GetComponentInChildren<UISpriteAnimation>();
		IFActivityIndicator.liveIndicators.Add(this);
		wasDismissed = false;
	}
	
	void OnDestroy()
	{
		IFActivityIndicator.liveIndicators.Remove(this);
	}
	
	public static void DismissAll()
	{
		foreach(IFActivityIndicator indicator in liveIndicators) {
			indicator.Dismiss();
		}
		liveIndicators = new List<IFActivityIndicator>();
	}
	
	public void Dismiss()
	{
		if(!wasDismissed) {
			if(usesOrphanPanel && parentPanel != null) {
				NGUITools.Destroy(parentPanel.gameObject);
			}
			NGUITools.Destroy(gameObject);	
			wasDismissed = true;	
		}
	}
	
	public void StartAnimating()
	{
		spriteAnimation.enabled = true;
	}
	
	public void StopAnimating()
	{
		spriteAnimation.enabled = false;
	}
	
	public bool IsAnimating
	{
		get
		{
			return spriteAnimation.enabled;
		}
	}
	
	public Color color
	{
		get
		{
			return sprite.color;
		}
		set
		{
			sprite.color = value;
		}
	}
}
