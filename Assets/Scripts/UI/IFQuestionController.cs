// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class IFQuestionController : MonoBehaviour
{
	public IFGameController gameController;
	public IFSysFontButton questionButton;
	public IFAnswerButton[] answerButtons;
	public UISprite questionBackground;
	public float textWidthPadding = 50f;

	private IFSysFontButton[] allButtons;
	
	private bool labelsNeedUpdate = false;
	private UIPanel mPanel;
	
	public static IFQuestionController CreateFromPrefab()
	{
		return IFQuestionController.Create();
	}
	
	public static IFQuestionController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFQuestionController>();
	}

	public static IFQuestionController Create()
	{
		return Create("Question");
	}

	
	private IFQuestion mQuestion;
	public IFQuestion Question
	{
		get
		{
			return mQuestion;
		}
		set
		{
			if(!ReferenceEquals(mQuestion, value)) {
				mQuestion = value;
				labelsNeedUpdate = true;
			}
		}
	}

	private float mButtonTextAlpha = 1f;
	public float ButtonTextAlpha
	{
		get
		{
			return mButtonTextAlpha;
		}
		set
		{
			if(mButtonTextAlpha != value) {
				mButtonTextAlpha = value;
				if(allButtons != null) {
					foreach(IFSysFontButton button in allButtons) {
						TweenAlpha.Begin(button.fontLabel.gameObject, .25f, mButtonTextAlpha);
					}
				}
			}
		}
	}

	
	public void GamePauseWasToggled(bool isPaused)
	{
		isEnabled = !isPaused;
	}
	
	private bool mIsEnabled = true;
	public bool isEnabled
	{
		get
		{
			return mIsEnabled;
		}
		set
		{
			if(mIsEnabled != value) {
				mIsEnabled = value;
				foreach(IFAnswerButton button in answerButtons) {
					button.isEnabled = mIsEnabled;
				}
			}
		}
	}
	
	public bool IsHidden
	{
		get
		{
			return mPanel.enabled;
		}
		set
		{
			mPanel.enabled = value;
		}
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
		mPanel = GetComponentInChildren<UIPanel>();
		allButtons = new IFSysFontButton[answerButtons.Length + 1];
		allButtons[0] = questionButton;
		for(int i = 1; i < allButtons.Length; i++) {
			allButtons[i] = answerButtons[i - 1].GetComponent<IFSysFontButton>();
			allButtons[i].fontLabel.alpha = ButtonTextAlpha;
		}
	}
	
	
	public void ShuffleAnswers()
	{
		IFAnswerButton[] randomButtons = answerButtons.OrderBy(a => Guid.NewGuid()).ToArray();
		randomButtons[0].AnswerText = mQuestion.CorrectAnswer;
		randomButtons[0].IsCorrectAnswer = true;
		randomButtons[0].AnswerSelectionDelegate = gameController.AnswerWasSelected;
		
		IEnumerator wrongAnswerEnumerator = mQuestion.IncorrectAnswers.GetEnumerator();
		for(int i = 1; i < randomButtons.Length; i++) {
			wrongAnswerEnumerator.MoveNext();
			randomButtons[i].AnswerText = (string)wrongAnswerEnumerator.Current;
			randomButtons[i].IsCorrectAnswer = false;
			randomButtons[i].AnswerSelectionDelegate = gameController.AnswerWasSelected;
		}
	}
	
	void Update()
	{
		if(labelsNeedUpdate && mQuestion != null) {
			questionButton.Text = mQuestion.Text;
			ShuffleAnswers();
			labelsNeedUpdate = false;
		}
	}
	
	public void InvalidateAnswers()
	{
		foreach(IFAnswerButton button in answerButtons) {
			button.Invalidate();
		}
	}
	
	public void LightUpAllAnswers()
	{
		foreach(IFAnswerButton button in answerButtons) {
			button.TurnOnLight();
		}
	}
}
