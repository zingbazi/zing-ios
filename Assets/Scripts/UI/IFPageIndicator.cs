// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IFPageIndicator : MonoBehaviour {

	public bool selected = false;
	
	private bool lastSelected = false;
	private UISprite sprite;
	
	void Start()
	{
		sprite = GetComponent<UISprite>();
		sprite.MakePixelPerfect();
	}
	
	void Update()
	{
		if(lastSelected != selected) {
			lastSelected = selected;
			
			if(selected) {
				sprite.spriteName = "PageControlFG";
			} else {
				sprite.spriteName = "PageControlBG";
			}
			sprite.MakePixelPerfect();
		}
	}
}
