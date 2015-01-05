// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class IFGameController : MonoBehaviour
{
	public delegate void SoloPlayGameDidEnd(IFGameController gameController, bool finished);
	public event SoloPlayGameDidEnd GameDidEndEvent;
	public int strikes = 3;
	public GameObject hintButton;
	public float interQuestionPauseSeconds = .5f;
	public UISysFontLabel categoryLabel;
	public GameObject roundLabel;
	public GameObject roundNumber;
	public UIWidget titleBackground;
	public UIImageButton pauseButton;
	
	public IFGame Game { get; private set; }
	
	private int currentPlayerNumber = 1;
	private float currentQuestionTimeRemaining = 10f;
	private float currentRoundTimeRemaining = 60f;
	private bool questionIsActive = false;
	private Queue<int> currentLevelIdQueue;
	private Queue<int> challengeQuestionRemoteIdQueue;
	private bool hintWasShown = false;

	public IFQuestionController questionController;
	public IFQuestionHUDController questionHUDController;
	
	public static IFGameController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFGameController>();
	}
	
	public static IFGameController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.GamePrefab == null) {
			return IFGameController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.GamePrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFGameController>();
	}
	
	public static IFGameController Create()
	{
		return Create("Game Screen");
	}
	
	void OnDisable()
	{
		if(questionController != null) {
			questionController.enabled = false;
		}
		if(questionHUDController != null) {
			questionHUDController.enabled = false;
		}
	}

	void OnEnable()
	{
		if(questionController != null) {
			questionController.enabled = true;
		}
		if(questionHUDController != null) {
			questionHUDController.enabled = true;
		}
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}
	
	void Update()
	{
		if(questionIsActive) {
			if(currentQuestionTimeRemaining > 0f) {
				currentQuestionTimeRemaining -= Time.deltaTime;
				questionHUDController.questionPercentageTimeElapsed = 1f - Mathf.Clamp01(currentQuestionTimeRemaining / Game.Level.QuestionDuration);
			} else {
				questionIsActive = false;
				questionController.LightUpAllAnswers();
				Game.AnswerQuestion(questionController.Question, false, Game.Level.QuestionDuration);
				StartCoroutine(CallAfterSeconds(interQuestionPauseSeconds * 2f, IncorrectAnswerWasSelected));
			}
			if(currentRoundTimeRemaining > 0f) {
				currentRoundTimeRemaining -= Time.deltaTime;
//				questionHUDController.questionPercentageTimeRemaining = Mathf.Clamp01(currentRoundTimeRemaining / Game.Level.Duration);
			}
		}
	}
	
	IEnumerator CallAfterSeconds(float seconds, Action method)
	{
		yield return new WaitForSeconds(seconds);
		method();
	}
	
	public IEnumerator ProceedToNextQuestion()
	{
		yield return new WaitForSeconds(interQuestionPauseSeconds);
		
		questionController.InvalidateAnswers();
		
		if(Game.Mode == IFGame.GameMode.PassAndPlay) {
			questionController.ButtonTextAlpha = 0f;
			
			string alertTitle = Localization.Localize("Please pass the device.");
			string alertMessage = Localization.Localize("It is time for your opponent to take their turn. Please pass the device to the other player.");
			string alertButtonText = Localization.Localize("Ok, I'm Ready");
			
			if(currentPlayerNumber == 1) {
				IFAlertViewController alert = IFAlertViewController.ShowAlert(alertMessage, alertTitle, alertButtonText);
				alert.AlertViewDidCloseEvent += PassAndPlayPlayerTwoNextQuestionAlertViewDidClose;
			} else {
				if(HasNextQuestion) {
					IFAlertViewController alert = IFAlertViewController.ShowAlert(alertMessage, alertTitle, alertButtonText);
					alert.AlertViewDidCloseEvent += PassAndPlayPlayerOneNextQuestionAlertViewDidClose;
				} else {
					EndGame();
				}
			}
		} else {
			NextQuestion();	
		}
	}
	
	public void StrikeOutAlertViewDidClose(IFAlertViewController controller, bool okWasSelected)
	{
		controller.AlertViewDidCloseEvent -= StrikeOutAlertViewDidClose;
		EndGame();
	}
	
	public void AnswerWasSelected(IFAnswerButton sendingButton, bool isCorrectAnswer)
	{
		if(hintButton.gameObject.activeSelf) {
			TweenAlpha alphaT = TweenAlpha.Begin(hintButton.gameObject, .25f, 0f);
			alphaT.from = 1f;
			alphaT.to = 0f;
			alphaT.onFinished += (tween) => {
				hintButton.gameObject.SetActive(false);	
			};
		}
		if(!questionIsActive) {
			sendingButton.Invalidate();
		} else {
			questionController.isEnabled = false;
			questionIsActive = false;
			Game.AnswerQuestion(questionController.Question, isCorrectAnswer, Game.Level.QuestionDuration - currentQuestionTimeRemaining);
			if(isCorrectAnswer) {
				CorrectAnswerWasSelected();
			} else {
				IncorrectAnswerWasSelected();
			}
		}
	}
	
	void IncorrectAnswerWasSelected()
	{
		if(Game.Mode == IFGame.GameMode.Normal) {
			questionHUDController.RemainingStrikeCount = --strikes;	
		}
		
		if(strikes < 0) {
			IFAlertViewController alert = IFAlertViewController.ShowAlert(Localization.Localize("Too many wrong answers."), Localization.Localize("Game Over!"));
			alert.AlertViewDidCloseEvent += StrikeOutAlertViewDidClose;
		} else {
			if(strikes == 0 && !string.IsNullOrEmpty(questionController.Question.Hint) && !hintWasShown) {
				hintButton.gameObject.SetActive(true);
				TweenAlpha alphaT = TweenAlpha.Begin(hintButton.gameObject, .25f, 0f);
				alphaT.from = 0f;
				alphaT.to = 1f;
			}
			StartCoroutine(ProceedToNextQuestion());
		}
	}
	
	void CorrectAnswerWasSelected()
	{
		if(Game.Mode == IFGame.GameMode.PassAndPlay) {
			int scoreForQuestion = Mathf.RoundToInt(currentQuestionTimeRemaining * Game.Level.Rank);
			if(currentPlayerNumber == 1) {
				questionHUDController.score += scoreForQuestion;
			} else {
				questionHUDController.playerTwoScore += scoreForQuestion;
			}
		} else {
			Game.Score += Mathf.RoundToInt(currentQuestionTimeRemaining);
			questionHUDController.score = Game.Score;
		}
		StartCoroutine(ProceedToNextQuestion());
	}
	
	private int mNextQuestionIndex = 0;
	public int NextQuestionIndex
	{
		get
		{
			return mNextQuestionIndex;
		}
		private set
		{
			if(mNextQuestionIndex != value) {
				mNextQuestionIndex = value;
				if(questionHUDController != null) {
					questionHUDController.questionIndex = mNextQuestionIndex;	
				}
			}
		}
	}

	private int mQuestionCount = 0;
	public int QuestionCount
	{
		get
		{
			return mQuestionCount;
		}
		private set
		{
			if(mQuestionCount != value) {
				mQuestionCount = value;
				if(questionHUDController != null) {
					questionHUDController.questionCount = mQuestionCount;
				}
			}
		}
			
	}
	
	private bool HasNextQuestion
	{
		get
		{
			if(Game.Mode == IFGame.GameMode.Challenge) {
				return challengeQuestionRemoteIdQueue != null && challengeQuestionRemoteIdQueue.Count > 0;
			} else {
				return currentLevelIdQueue != null && currentLevelIdQueue.Count > 0;	
			}
		}
	}
	
	private void NextQuestion()
	{
		if(HasNextQuestion) {
			IFQuestion nextQuestion;
			if(Game.Mode == IFGame.GameMode.Challenge) {
				nextQuestion = IFQuestion.QuestionWithRemoteIdentifier(challengeQuestionRemoteIdQueue.Dequeue());
			} else {
				nextQuestion = IFQuestion.QuestionWithIdentifier(currentLevelIdQueue.Dequeue());
			}
			questionController.Question = nextQuestion;
			questionController.isEnabled = true;
			questionHUDController.Reset(false);
			currentQuestionTimeRemaining = Game.Level.QuestionDuration;
			questionIsActive = true;
			NextQuestionIndex++;
		} else {
			if(Game.Mode == IFGame.GameMode.Normal && Game.Level.Rank < IFGameLevel.MaxRank) {
				Game.Level = IFGameLevel.LevelWithRank(Game.Level.Rank + 1);
				Game.Save();
				StartLevel(IFQuestionCategory.GetRandomCategory());
			} else {
				EndGame();	
			}
		}
	}
	
	public void StartLevel(IFQuestionCategory category)
	{
		questionController.ButtonTextAlpha = 0f;
		questionHUDController.playerTwoUI.SetActive(Game.Mode != IFGame.GameMode.Normal);
		
		QuestionCount = Game.Level.QuestionCount;
		NextQuestionIndex = 0;
		questionHUDController.roundNumberLabel.text = Game.Level.Rank.ToString();
		if(Game.Mode == IFGame.GameMode.Challenge) {
			IFChallenge challenge = Game.challenge;
			int previouslyAnswered;
			if(challenge.WasCreator) {
				previouslyAnswered = challenge.UserAnswerCount;
			} else {
				previouslyAnswered = challenge.OpponentAnswerCount;
			}
			
			IEnumerable<int> remainingQuestionIds = challenge.RemoteQuestionIds.Skip(previouslyAnswered);
			
			challengeQuestionRemoteIdQueue = new Queue<int>(remainingQuestionIds);
			currentLevelIdQueue = null;
		} else {
			if(category != null) {
				currentLevelIdQueue = Game.Level.GetRandomQuestionIdQueue(category);
			} else {
				currentLevelIdQueue = Game.Level.GetRandomQuestionIdQueue(QuestionCount);
			}
			challengeQuestionRemoteIdQueue = null;
		}
		
		currentRoundTimeRemaining = Game.Level.Duration;
		questionHUDController.Reset(true);
		
		if(Game.Mode == IFGame.GameMode.PassAndPlay) {
			currentPlayerNumber = 1;
			IFAlertViewController alert = IFAlertViewController.ShowAlert(Localization.Localize("Start with player one."));
			alert.AlertViewDidCloseEvent += PassAndPlayPlayerOneNextQuestionAlertViewDidClose;
		} else {
			questionController.ButtonTextAlpha = 1f;
			IFGameManager.GameIsPaused = false;
			NextQuestion();	
		}
	}
	
	void PassAndPlayPlayerOneNextQuestionAlertViewDidClose(IFAlertViewController controller, bool ok)
	{
		currentPlayerNumber = 1;
		controller.AlertViewDidCloseEvent -= PassAndPlayPlayerOneNextQuestionAlertViewDidClose;
		IFGameManager.GameIsPaused = false;
		NextQuestion();
		questionController.ButtonTextAlpha = 1f;
	}
	
	void PassAndPlayPlayerTwoNextQuestionAlertViewDidClose(IFAlertViewController controller, bool ok)
	{
		currentPlayerNumber = 2;
		controller.AlertViewDidCloseEvent -= PassAndPlayPlayerTwoNextQuestionAlertViewDidClose;
		questionController.ShuffleAnswers();
		questionController.ButtonTextAlpha = 1f;
		questionController.isEnabled = true;
		questionHUDController.Reset(false);
		currentQuestionTimeRemaining = Game.Level.QuestionDuration;
		questionIsActive = true;
	}
	
	public void StartGame(IFGame game, IFQuestionCategory category)
	{
		Game = game;
		questionHUDController.playerTwoScore = 0;
		questionHUDController.score = 0;
		questionHUDController.RemainingStrikeCount = strikes = 3;
		questionHUDController.usernameLabel.Text = PlayerPrefs.GetString(IFConstants.UsernamePrefsKey, Localization.Localize("Player 1"));
		if(Game.Mode == IFGame.GameMode.PassAndPlay) {
			questionHUDController.secondPlayerUsernameLabel.Text = Localization.Localize("Player 2");
		}
		else if(Game.Mode == IFGame.GameMode.Challenge) {
			if(game.challenge.WasCreator) {
				questionHUDController.secondPlayerUsernameLabel.Text = game.challenge.OpponentUsername;
				questionHUDController.playerTwoScore = game.challenge.OpponentScore;
				questionHUDController.usernameLabel.Text = game.challenge.Username;
				questionHUDController.score = game.challenge.UserScore;
			} else {
				questionHUDController.secondPlayerUsernameLabel.Text = game.challenge.Username;
				questionHUDController.playerTwoScore = game.challenge.UserScore;
				questionHUDController.usernameLabel.Text = game.challenge.OpponentUsername;
				questionHUDController.score = game.challenge.OpponentScore;
			}
		}
		
		if(Game.Mode != IFGame.GameMode.Normal) {
			roundNumber.SetActive(false);
			roundLabel.SetActive(false);
			categoryLabel.gameObject.SetActive(true);
//			UIAnchor anchor = categoryLabel.GetComponent<UIAnchor>();
//			anchor.side = UIAnchor.Side.Center;
//			anchor.relativeOffset = Vector2.zero;
//			anchor.widgetContainer = titleBackground;
//			categoryLabel.pivot = UIWidget.Pivot.Center;
//			IFContainSysFontLabel contain = categoryLabel.gameObject.AddComponent<IFContainSysFontLabel>();
//			contain.container = categoryLabel.transform.parent;
//			contain.padding = new Vector2(20f, 10f);
		} else {
			categoryLabel.gameObject.SetActive(false);
			Bounds backgroundBounds = NGUIMath.CalculateRelativeWidgetBounds(titleBackground.cachedTransform.parent);
			IFContainSysFontLabel contain = categoryLabel.gameObject.AddComponent<IFContainSysFontLabel>();
			contain.container = categoryLabel.transform.parent;
			contain.padding = new Vector2((backgroundBounds.size.x * .6f) / 2f, 10f);
		}
		
		if(!IFGameManager.UserIsRegistered) {
			int playsUntilNextNag = PlayerPrefs.GetInt(IFConstants.PlaysUntilNextNag, 3);
			if(playsUntilNextNag > 0) {
				PlayerPrefs.SetInt(IFConstants.PlaysUntilNextNag, --playsUntilNextNag);
				StartLevel(category);
			} else {
				PlayerPrefs.SetInt(IFConstants.PlaysUntilNextNag, 3);
				IFRegisterNagWindowController nag = IFRegisterNagWindowController.CreateFromPrefab();
				nag.Show(() => {
					StartLevel(category);
				});
			}
		} else {
			StartLevel(category);	
		}
	}
	
	public void StartNewGame(IFGameLevel level, IFGame.GameMode mode)
	{
		StartGame(new IFGame(level, mode), IFQuestionCategory.GetRandomCategory());
	}
	
	public void EndGame()
	{
		bool noMoreQuestions;
		if(Game.Mode == IFGame.GameMode.Normal) {
			noMoreQuestions = (!HasNextQuestion && (Game.Level.Rank == IFGameLevel.MaxRank));	
		} else {
			noMoreQuestions = !HasNextQuestion;
		}
		
		bool outOfStrikes = (strikes <= 0);
		
		bool finished = noMoreQuestions || outOfStrikes;
		Game.Completed = finished;
		Game.Save();
		
		if(finished && Game.Mode == IFGame.GameMode.PassAndPlay) {
			string title = null, message = null;
			if(questionHUDController.score == questionHUDController.playerTwoScore) {
				message = Localization.Localize("It's a tie");
			} else if(questionHUDController.score > questionHUDController.playerTwoScore) {
				title = Localization.Localize("WINNER!");
				message = PlayerPrefs.GetString(IFConstants.UsernamePrefsKey, Localization.Localize("Player 1"));
			}  else {
				title = Localization.Localize("WINNER!");
				message = Localization.Localize("Player 2");
			}
			IFAlertViewController alert;
			if (title != null) {
				alert = IFAlertViewController.ShowAlert(message, title);
			} else {
				alert = IFAlertViewController.ShowAlert(message);
			}
			alert.AlertViewDidCloseEvent += PassAndPlayEndGameAlertViewDidClose;
		} else {
			IFGameManager.SharedManager.StartGameResultUpload(Game);
			questionHUDController.score = 0;
			questionHUDController.playerTwoScore = 0;
			if(GameDidEndEvent != null) {
				GameDidEndEvent(this, finished);
			}
		}
		questionHUDController.Reset(true);
		questionHUDController.RemainingStrikeCount = 3;
		NextQuestionIndex = 0;
		questionIsActive = false;
	}
	
	void PassAndPlayEndGameAlertViewDidClose(IFAlertViewController controller, bool ok)
	{
		controller.AlertViewDidCloseEvent -= PassAndPlayEndGameAlertViewDidClose;
		questionController.ButtonTextAlpha = 1f;

		questionHUDController.score = 0;
		questionHUDController.playerTwoScore = 0;
		if(GameDidEndEvent != null) {
			GameDidEndEvent(this, true);
		}
	}

	public void PauseGame()
	{
		if(!IFGameManager.GameIsPaused) {
			IFGameManager.SharedManager.ToggleGamePause();
			questionController.ButtonTextAlpha = 0f;
			Game.PauseCount -= 1;
			IFAlertViewController.ShowAlert(Localization.Localize("When you are ready to resume, you will be presented with a new question with no time or score penalty."), Localization.Localize("Game is Paused"), Localization.Localize("Resume"), (controller, okWasSelected) => {
				IFGameManager.SharedManager.ToggleGamePause();
				questionController.ButtonTextAlpha = 1f;
				if(Game.PauseCount <= 0) {
					pauseButton.isEnabled = false;
					pauseButton.target.color = new Color(.862745098f, .862745098f, .862745098f);
				}
			});
		}
	}
	
	void ShowForfeitOptions(GameObject sender)
	{
		UIButtonMessage buttonMessageComp = sender.GetComponent<UIButtonMessage>();
		buttonMessageComp.enabled = false;
		IFGameManager.SharedManager.ToggleGamePause();
		questionController.ButtonTextAlpha = 0f;
		IFForfeitWindowController.CreateFromPrefab().Show((IFForfeitWindowController.ButtonSelection selectedButton) =>
		{
			IFGameManager.SharedManager.ToggleGamePause();
			buttonMessageComp.enabled = true;
			questionController.ButtonTextAlpha = 1f;
			switch(selectedButton) 
			{
				case IFForfeitWindowController.ButtonSelection.Resume: break;
				case IFForfeitWindowController.ButtonSelection.CurrentGames:
				{
					if(Game.challenge != null) {
						if(Game.challenge.WasCreator) {
							Game.challenge.UserAnswerCount = Game.Answers.Count;
						} else {
							Game.challenge.OpponentAnswerCount = Game.Answers.Count;
						}
					}
					if(Game.challenge != null) {
						Game.challenge.Save();
					}
					Game.Save();
					IFGameManager.SharedManager.StartGameResultUpload(Game);
					IFGameManager.SharedManager.TransitionToMyGamesScreen();
					break;
				}
				case IFForfeitWindowController.ButtonSelection.Forfeit:
				{
					if(Game.challenge != null) {
						Game.forfeit = true;
						if(Game.challenge.WasCreator) {
							Game.challenge.UserState = IFChallenge.ChallengeState.Complete;
						} else {
							Game.challenge.OpponentState = IFChallenge.ChallengeState.Complete;
						}
						IFGameManager.SharedManager.StartGameResultUpload(Game);
					}
					IFGameManager.SharedManager.TransitionToHomeScreen();
					break;
				}
			}
		});
	}
	
	public void HintButtonTapped()
	{
		IFHintController.PresentControllerWithQuestionAndHintText(questionController.Question.Text, questionController.Question.Hint, () => {
			hintWasShown = true;
			TweenAlpha alphaT = TweenAlpha.Begin(hintButton.gameObject, .25f, 0f);
			alphaT.from = 1f;
			alphaT.to = 0f;
			alphaT.onFinished += (tween) => {
				hintButton.gameObject.SetActive(false);	
			};
		});
	}
}
