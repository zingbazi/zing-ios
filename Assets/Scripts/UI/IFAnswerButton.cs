// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;

public class IFAnswerButton : MonoBehaviour {
	
	public UISprite lightSprite;
	public UISysFontLabel fontLabel;
	public Action<IFAnswerButton, bool> AnswerSelectionDelegate;
	
	private UIButton uiButton;
	private IFSysFontButton sysFontButton;
	private bool needsUpdate = true;
	
	private string mAnswerText;
	public string AnswerText
	{
		get
		{
			return mAnswerText;
		}
		set
		{
			mAnswerText = value;
			needsUpdate = true;
		}
	}
	
	public bool isEnabled
	{
		get
		{
			return uiButton.isEnabled;
		}
		set
		{
			if(uiButton != null) {
				uiButton.isEnabled = value;	
			}
			
		}
	}
	
	private bool mIsCorrectAnswer;
	public bool IsCorrectAnswer
	{
		get
		{
			return mIsCorrectAnswer;
		}
		set
		{
			mIsCorrectAnswer = value;
			needsUpdate = true;
		}
	}
	
	void Start()
	{
		uiButton = GetComponent<UIButton>();
		sysFontButton = GetComponent<IFSysFontButton>();
	}
	
	void Update()
	{
		if(needsUpdate) {
			lightSprite.spriteName = "TabInactive";
			sysFontButton.Text = AnswerText;
			needsUpdate = false;
		}
	}
	
	void OnClick()
	{
		if(IsCorrectAnswer) {
			lightSprite.spriteName = "TabCorrect";
		} else {
			lightSprite.spriteName = "TabIncorrect";
		}
		if(AnswerSelectionDelegate != null) {
			AnswerSelectionDelegate(this, IsCorrectAnswer);	
		}
	}
	
	public void Invalidate()
	{
		if(lightSprite != null) {
			lightSprite.spriteName = "TabInactive";	
		}
	}
	
	public void TurnOnLight()
	{
		if(lightSprite != null) {
			if(IsCorrectAnswer) {
				lightSprite.spriteName = "TabCorrect";
			} else {
				lightSprite.spriteName = "TabIncorrect";
			}
		}
	}
}
