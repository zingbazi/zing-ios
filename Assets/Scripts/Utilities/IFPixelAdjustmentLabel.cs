// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class IFPixelAdjustmentLabel : MonoBehaviour {

	public float updateInterval = 1f;
	public UILabel label;
	public UIRoot uiRoot;
	
    private float lastInterval;
	
	void Start()
	{
		if(label == null) {
			label = GetComponent<UILabel>();
		}
		if(uiRoot == null) {
			uiRoot = NGUITools.FindInParents<UIRoot>(gameObject);
		}
		lastInterval = Time.realtimeSinceStartup;
	}
	
	void Update()
	{
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval) {
			lastInterval = timeNow;
			label.text =  uiRoot.pixelSizeAdjustment.ToString("F3");
        }
	}
}
