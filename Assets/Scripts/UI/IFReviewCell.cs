// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;

public class IFReviewCell : MonoBehaviour {
	
	public UISysFontLabel questionLabel;
	public UISysFontLabel answerLabel;
	public UISprite correctIcon;
	public UISysFontLabel opponentAnswerLabel;
	public UISprite opponentCorrectIcon;
	public Vector2 answerRelativeOffsetSolo = new Vector2(-.05f, .16f);
	public Vector2 answerRelativeOffsetChallenge = new Vector2(-.05f, .24f);
	public GameObject moreInfoButton;
	
	public Action<IFAnswer> AnswerMoreInfoSelectionDelegate;

	private UIAnchor correctIconAnchor;
	private bool needsUpdate = true;
	
	private IFAnswer mAnswer;
	public IFAnswer Answer
	{
		get
		{
			return mAnswer;
		}
		set
		{
			if(mAnswer == null || !mAnswer.Equals(value)) {
				mAnswer = value;
				needsUpdate = true;
			}
		}
	}

	private IFAnswer mOpponentAnswer = null;
	public IFAnswer OpponentAnswer
	{
		get
		{
			return mOpponentAnswer;
		}
		set
		{
			if(mOpponentAnswer == null || !mOpponentAnswer.Equals(value)) {
				mOpponentAnswer = value;
				needsUpdate = true;
			}
		}
	}
	
	private string mPlayerUsername;
	public string PlayerUsername
	{
		get
		{
			return mPlayerUsername;
		}
		set
		{
			mPlayerUsername = value;
			needsUpdate = true;
		}
	}

	private string mOpponentUsername;
	public string OpponentUsername
	{
		get
		{
			return mOpponentUsername;
		}
		set
		{
			mOpponentUsername = value;
			needsUpdate = true;
		}
	}

	
	void Start()
	{
		correctIconAnchor = correctIcon.GetComponent<UIAnchor>();
		UIEventListener.Get(moreInfoButton).onClick += (sender) => {
			AnswerMoreInfoSelectionDelegate(Answer);
		};
	}
	
	void Update()
	{
		if(needsUpdate) {
			UpdateCell();
		}
	}
	
	void UpdateCell()
	{
		questionLabel.Text = Answer.Question.Text;
		correctIcon.spriteName = Answer.Correct ? "IconCorrect" : "IconIncorrect";
		if(string.IsNullOrEmpty(Answer.Question.Hint)) {
			moreInfoButton.SetActive(false);
		} else {
			moreInfoButton.SetActive(true);
		}
		
		if(OpponentAnswer != null) {
			answerLabel.Text = PlayerUsername;
			opponentAnswerLabel.Text = OpponentUsername;
			opponentCorrectIcon.spriteName = OpponentAnswer.Correct ? "IconCorrect" : "IconIncorrect";
			opponentAnswerLabel.gameObject.SetActive(true);
			opponentCorrectIcon.gameObject.SetActive(true);
			correctIconAnchor.relativeOffset = answerRelativeOffsetChallenge;
		} else {
//			if(Answer.Correct && Answer.Game != null) {
//				answerLabel.Text = Mathf.RoundToInt((Answer.Game.Level.QuestionDuration - Answer.Duration) * Answer.Game.Level.Rank).ToString("G") + " points";
//			} else {
				answerLabel.Text = Answer.Question.CorrectAnswer;
//			}
			opponentAnswerLabel.gameObject.SetActive(false);
			opponentCorrectIcon.gameObject.SetActive(false);
			correctIconAnchor.relativeOffset = answerRelativeOffsetSolo;
		}

		needsUpdate = false;
	}
}
