// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFQuestionHUDController : MonoBehaviour
{
	private UIPanel mPanel;
	
	public Transform questionTimer;
	
	public UILabel scoreLabel;
	public UILabel secondPlayerScoreLabel;
	public UISysFontLabel usernameLabel;
	public UISysFontLabel secondPlayerUsernameLabel;
	public UISprite userInfoBackground;
	public UISprite secondPlayerUserInfoBackgorund;
	public float usernameLabelScaleFactor = .45f;
	
	public UILabel roundNumberLabel;
	public GameObject roundNumberDisplay;
	public UISysFontLabel roundLabel;
	public UISysFontLabel categoryLabel;

	public UISprite[] strikeLights;

	public GameObject playerOneUI;
	public GameObject playerTwoUI;

	public float questionPercentageTimeElapsed = 0f;
	public int score = 0;
	public int playerTwoScore = 0;
	public int questionIndex = 0;
	public int questionCount = 0;
	private int lastScore = -1;
	private int lastPlayerTwoScore = -1;
	private Quaternion questionTimerStartingRotation = Quaternion.AngleAxis(24f, Vector3.back);
	private Quaternion questionTimerEndingRoation = Quaternion.AngleAxis(161f, Vector3.back);
	private TweenAlpha questionTimerFlashTween;
	private UISprite questionTimerSprite;

	public IFGameController gameController;

	public static IFQuestionHUDController CreateFromPrefab()
	{
		return IFQuestionHUDController.Create();
	}

	public static IFQuestionHUDController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFQuestionHUDController>();
	}

	public static IFQuestionHUDController Create()
	{
		return Create("Question HUD");
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
		questionTimerFlashTween = questionTimer.GetComponentInChildren<TweenAlpha>();
		questionTimerSprite = questionTimer.GetComponentInChildren<UISprite>();
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
	
	void OnEnable()
	{
		float scaleFactor = Mathf.Clamp01(usernameLabelScaleFactor);
		usernameLabel.LocalClipWidth = userInfoBackground.cachedTransform.localScale.x * scaleFactor;
		secondPlayerUsernameLabel.LocalClipWidth = secondPlayerUserInfoBackgorund.cachedTransform.localScale.x * scaleFactor;
	}
	
	
	public void Reset(bool shouldResetRound)
	{
		if(shouldResetRound) {
			questionTimer.rotation = questionTimerStartingRotation;
		}
	}

	private int mRemainingStrikeCount;
	public int RemainingStrikeCount
	{
		get
		{
			return mRemainingStrikeCount;
		}
		set
		{
			if(mRemainingStrikeCount != value) {
				mRemainingStrikeCount = value;
				for(int i = 0; i < strikeLights.Length; i++) {
					if(i < mRemainingStrikeCount) {
						strikeLights[i].alpha = 1f;
					} else {
						TweenAlpha.Begin(strikeLights[i].gameObject,.25f, 0f);
					}
				}
			}
		}
	}

	void Update()
	{
		if(questionPercentageTimeElapsed < 1f) {
			questionTimer.localRotation = Quaternion.Lerp(questionTimerStartingRotation, questionTimerEndingRoation, questionPercentageTimeElapsed);
		} else {
			questionTimer.localRotation = questionTimerEndingRoation;
		}
		
		if(questionPercentageTimeElapsed > .55f) {
			questionTimerFlashTween.enabled = true;
		} else {
			questionTimerFlashTween.enabled = false;
			questionTimerSprite.alpha = 1f;
		}
		
		if(lastScore != score) {
			lastScore = score;
			scoreLabel.text = score.ToString();
		}
		if(gameController.Game.Mode != IFGame.GameMode.Normal) {
			if(lastPlayerTwoScore != playerTwoScore) {
				lastPlayerTwoScore = playerTwoScore;
				secondPlayerScoreLabel.text = playerTwoScore.ToString();
			}
		}
	}

	public void PauseButtonWasTapped()
	{
//		IFGameManager.SharedManager.ToggleGamePause();
		gameController.PauseGame();
	}

//	public void BackButtonWasTapped()
//	{
//		if(gameController != null) {
//			gameController.EndGame();	
//		}
//	}
}
