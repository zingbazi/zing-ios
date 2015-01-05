// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UISysFontLabel))]
[ExecuteInEditMode]
[AddComponentMenu("IFES/Contain SysFont Label")]
public class IFContainSysFontLabel : MonoBehaviour
{
	public Camera uiCamera;
	public Transform container;
	public Vector2 padding = new Vector2(20f, 20f);
	
	private UIRoot uiRoot;
	private Vector2 screenSize;
	private Vector2 widgetSize;
	private UISysFontLabel label;
	
	private Vector2 lastScreenSize = Vector2.zero;
	private Vector2 lastWidgetSize = Vector2.zero;
	private Vector2 lastPadding = Vector2.zero;
	
	void Start()
	{
		label = GetComponent<UISysFontLabel>();
		uiRoot = NGUITools.FindInParents<UIRoot>(gameObject);
		uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
	}
	
	void OnEnable()
	{
		if(uiRoot == null)  uiRoot = NGUITools.FindInParents<UIRoot>(gameObject);
		if(uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
		if(label == null) label = GetComponent<UISysFontLabel>();
		lastScreenSize = lastWidgetSize = Vector2.zero;
	}
	
	void Update()
	{
		if(container != null) {
			Bounds widgetBounds = NGUIMath.CalculateRelativeWidgetBounds(container);
			widgetSize.x = widgetBounds.size.x;
			widgetSize.y = widgetBounds.size.y;
			if(widgetSize != lastWidgetSize || lastPadding != padding) {
				lastWidgetSize = widgetSize;
				lastPadding = padding;
				if(padding.x >= 0f) label.MaxWidthPixels = Mathf.RoundToInt(widgetSize.x - padding.x * 2f);
				if(padding.y >= 0f) label.MaxHeightPixels = Mathf.RoundToInt(widgetSize.y - padding.y * 2f);
				label.MarkAsChanged();
			}
		} else {
			screenSize.x = uiCamera.pixelWidth * uiRoot.pixelSizeAdjustment;
			screenSize.y = uiCamera.pixelHeight * uiRoot.pixelSizeAdjustment;
			if(screenSize != lastScreenSize || lastPadding != padding) {
				lastScreenSize = screenSize;
				lastPadding = padding;
				if(padding.x >= 0f) label.MaxWidthPixels = Mathf.RoundToInt(screenSize.x - padding.x * 2f);
				if(padding.y >= 0f) label.MaxHeightPixels = Mathf.RoundToInt(screenSize.y - padding.y * 2f);
				label.MarkAsChanged();
			}
		}
	}
	
}
