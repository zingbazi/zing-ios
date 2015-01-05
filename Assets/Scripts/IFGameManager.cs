// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Text;

public class IFGameManager : MonoBehaviour
{
	public UIPanel mainPanel;
	public IFRemoteURL remoteURLs;
	
	private AudioSource mBackgroundMusic;
	public AudioSource BackgroundMusic { get { return mBackgroundMusic; } }
	
	public enum TransitionDirection
	{
		RightToLeft,
		LeftToRight,
		Default = LeftToRight
	};
	public delegate bool ShouldTransitionToDefault();
	
	private static IFGameManager mSharedInstance;
	public static IFGameManager SharedManager
	{
		get
		{
			if(mSharedInstance == null) {
				GameObject managerGO = GameObject.Find("Game Manager");
				if(managerGO == null) {
					managerGO = new GameObject("Game Manager");
					managerGO.AddComponent<IFGameManager>();
				}
				mSharedInstance = managerGO.GetComponent<IFGameManager>();	
			}
			return mSharedInstance;
		}
	}
	
	private static IFLoadableAssets mCachedLoadableAssets;
	public static IFLoadableAssets LoadableAssets
	{
		get
		{
			if(mCachedLoadableAssets == null) {
				mCachedLoadableAssets = SharedManager.GetComponent<IFLoadableAssets>();
			}
			return mCachedLoadableAssets;
		}
	}
	
	public static bool IsLoggedIn
	{
		get { return PlayerPrefs.HasKey(IFConstants.AccessTokenPrefsKey) && PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey).Length > 0; }
	}
	
	public static bool UserIsRegistered
	{
		get
		{
			string isRegistered = PlayerPrefs.GetString(IFConstants.UserIsRegistered, "no");
			return !isRegistered.Equals("no");
		}
		set
		{
			if(value) {
				PlayerPrefs.SetString(IFConstants.UserIsRegistered, "yes");
			} else {
				PlayerPrefs.SetString(IFConstants.UserIsRegistered, "no");	
			}
		}
	}
	
	void Awake()
	{
		Application.targetFrameRate = 60;
		Application.backgroundLoadingPriority = ThreadPriority.Normal;
		
#if UNITY_ANDROID && !UNITY_EDITOR
		WWW regFont = new WWW("jar:file://" + Application.dataPath + "!/assets/BNazanin.ttf");
		while(!regFont.isDone) {}
		File.WriteAllBytes(Application.persistentDataPath + "/BNazanin.ttf", regFont.bytes);
				
		WWW boldFont = new WWW("jar:file://" + Application.dataPath + "!/assets/bnazaninbold.ttf");
		while(!boldFont.isDone) {}
		File.WriteAllBytes(Application.persistentDataPath + "/BNazaninBold.ttf", boldFont.bytes);
#endif
		
		if(IFDatabase.SharedDatabase == null) {
			Debug.LogError("The database couldn't be loaded.");
		}
		if(IFUploadManager.SharedManager == null) {
			Debug.LogError("Problem while initializing the upload manager.");
		}
		DontDestroyOnLoad(gameObject);
		object obj = new MD5CryptoServiceProvider(); // Force this class to not be stripped from iOS builds
		obj.GetHashCode();

		IFHomeScreenController homeScreen = IFGameManager.LoadableAssets.cachedHomeScreenController;
		if(homeScreen == null) homeScreen = IFHomeScreenController.CreateFromPrefab();
		ActiveController = homeScreen;
		IFFacebookBinding.init((success, error) => {
			if(!success) {
				Debug.Log("unable to init facebook: "+error.message);
			} else {
				Debug.Log("Facebook initialized!");
			}
		});
	}

	IEnumerator UpdateQuestionsAndCategories()
	{
		yield return null;
		if(UserIsRegistered && IsLoggedIn) {
			string categoryUrl = remoteURLs.Categories + "?access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
			string questionUrl = remoteURLs.Questions +  "?access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);

			TextAsset initialSyncDate = Resources.Load("QuestionBakeDate") as TextAsset;
			string lastHighScoreSync = PlayerPrefs.GetString(IFConstants.LastHighScoreSync, initialSyncDate.text);
			questionUrl += "&since="+lastHighScoreSync;
			PlayerPrefs.SetString(IFConstants.LastHighScoreSync, DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture));

			WWW categoryRequest = new WWW(categoryUrl);
			yield return categoryRequest;

			if(categoryRequest.error == null) {
				IFDatabase.SharedDatabase.BeginTransaction();
				IFQuestionCategory.LoadCategoriesFromJSON(categoryRequest.text);

				WWW questionRequest = new WWW(questionUrl);
				yield return questionRequest;

				if(questionRequest.error == null) {
					IFQuestion.LoadQuestionsFromJSON(questionRequest.text);
				} else {
					Debug.LogError("Error updating questions: "+questionRequest.error);
				}

				IFDatabase.SharedDatabase.CommitTransaction();
			} else {
				Debug.LogError("Error updating categories: "+categoryRequest.error);
			}
		}
	}

	void OnApplicationPause(bool pause)
	{
		if(pause) {
			PlayerPrefs.Save();
			StartCoroutine(LogSessionEnd());
		} else {
			StartCoroutine(LogSessionStart());
		}
	}

	IEnumerator LogSessionStart()
	{
#if UNITY_EDITOR
		yield return null;
#else
		WWWForm query = new WWWForm();
		string accessToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey, null);
		if(!string.IsNullOrEmpty(accessToken)) {
			query.AddField("access_token", accessToken);
		}
		query.AddField("duid", SystemInfo.deviceUniqueIdentifier);
		query.AddField("event_type", "app-start");
		query.AddField("key", "platform");
		query.AddField("value", (Application.platform == RuntimePlatform.Android) ? "Android" : "iOS");
		
		WWW logSessionStart = new WWW(remoteURLs.EventLog,query);
		yield return logSessionStart;
#endif
	}
	
	IEnumerator LogSessionEnd()
	{
#if UNITY_EDITOR
		yield return null;
#else
		WWWForm query = new WWWForm();
		string accessToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey, null);
		if(!string.IsNullOrEmpty(accessToken)) {
			query.AddField("access_token", accessToken);
		}
		query.AddField("duid", SystemInfo.deviceUniqueIdentifier);
		query.AddField("event_type", "app-stop");
		query.AddField("key", "duration");
		query.AddField("value", Time.time.ToString("F0"));
		
		WWW logSessionEnd = new WWW(remoteURLs.EventLog, query);
		yield return logSessionEnd;
#endif
	}
	
	void OnApplicationQuit()
	{
		PlayerPrefs.Save();
		IEnumerator coroutine = LogSessionEnd();
		while(coroutine.MoveNext()) {
			if(!(coroutine.Current is WWW)) {
				continue;
			}
			WWW www = (WWW)coroutine.Current;
			float timeout = Time.realtimeSinceStartup + 10;
			
			while(!www.isDone) {
				if(Time.realtimeSinceStartup > timeout) break;
			}
		}
	}
	
	IEnumerator CheckSurveyStatus()
	{
		if(UserIsRegistered && PlayerPrefs.GetString(IFConstants.SurveyStatus, "pending").Equals("pending")) {
			string url = remoteURLs.Survey + "?access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
			WWW web = new WWW(url);
			yield return web;
			if(web.responseHeaders["STATUS"].Contains("404")) {
				PlayerPrefs.SetString(IFConstants.SurveyStatus, "pending");
			}
			else if(web.responseHeaders["STATUS"].Contains("200")) {
				PlayerPrefs.SetString(IFConstants.SurveyStatus, "complete");
			}
		}
	}
	
	void Start()
	{
		if(remoteURLs == null) {
			remoteURLs = GetComponent<IFRemoteURL>();
		}
		mBackgroundMusic = GetComponent<AudioSource>();
		UpdateSoundConfiguration(false);
		StartCoroutine(LogSessionStart());
		StartCoroutine(CheckSurveyStatus());
		StartCoroutine(UpdateQuestionsAndCategories());
	}
	
	public void UpdateSoundConfiguration(bool ignoreMusicPreference)
	{
		if(!ignoreMusicPreference) {
			string pref = PlayerPrefs.GetString(IFConstants.BackgroundMusicOnPreference, "on");
			if(pref.Equals("on")) {
				if(!mBackgroundMusic.isPlaying || mBackgroundMusic.volume <= 0f) {
					mBackgroundMusic.Play();
					TweenVolume.Begin(gameObject, 1f, 1f);
				}
			}
		}
		
		string soundEffectPref = PlayerPrefs.GetString(IFConstants.SoundEffectOnPreference, "on");
		UIButtonSound[] sounds = mainPanel.GetComponentsInChildren<UIButtonSound>();
		foreach(UIButtonSound sound in sounds) {
			if(soundEffectPref.Equals("on")) {
				sound.volume = 1f;
			} else {
				sound.volume = 0f;
			}
		}
	}
	
	public void PauseBackgroundMusic()
	{
		if(mBackgroundMusic.isPlaying || mBackgroundMusic.volume > 0f) {
			TweenVolume.Begin(gameObject, 1f, 0f).onFinished += (tween) => {
				mBackgroundMusic.enabled = true;
			};
		}
	}
	
	private MonoBehaviour mActiveController = null;
	private MonoBehaviour ActiveController
	{
		get
		{
			return mActiveController;
		}
		set
		{
			mActiveController = value;
			if(mActiveController == null) {
				return;
			}
			GameObject go = mActiveController.gameObject;
			go.layer = mainPanel.gameObject.layer;
			go.SetActive(true);
			
			Transform activeTransform = mActiveController.transform;
			activeTransform.parent = mainPanel.cachedTransform;
			activeTransform.localScale = Vector3.one;
			activeTransform.localPosition = Vector3.zero;
			activeTransform.localRotation = Quaternion.identity;
		}
	}
	
	IFGameController GetGameController()
	{
		IFGameController gc = IFGameManager.LoadableAssets.cachedGameController;
		if(gc == null) {
			gc = IFGameController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedGameController = gc;
		}
		return gc;
	}
	
	public void StartSoloPlayGame(IFQuestionCategory category)
	{
		StartGame(new IFGame(IFGameLevel.GetLevelWithMinimumRank(), IFGame.GameMode.Normal), category);
	}
	
	public void GameDidEnd(IFGameController gameController, bool finished)
	{
		gameController.GameDidEndEvent -= GameDidEnd;
		switch(gameController.Game.Mode) {
			case IFGame.GameMode.Normal:
				if(finished) {
					TransitionToReviewForGame(gameController.Game);
				} else {
					TransitionToHomeScreen();
				}
				break;
			case IFGame.GameMode.Challenge:
				if(finished) {
					TransitionToReviewForChallenge(gameController.Game.challenge);
				} else {
					TransitionToHomeScreen();
				}
				break;
			default:
				TransitionToHomeScreen();
				break;
		}
	}
	
	public void StartGame(IFGame game)
	{
		StartGame(game, IFQuestionCategory.GetRandomCategory());
	}
	
	public void StartGame(IFGame game, IFQuestionCategory category)
	{
		if(UserIsRegistered) {
			string surveyStatus = PlayerPrefs.GetString(IFConstants.SurveyStatus, "pending");
			if(surveyStatus.Equals("pending")) {
				int totalSurveyRequests = PlayerPrefs.GetInt(IFConstants.TotalSurveyRequests, 0);
				if(++totalSurveyRequests <= 3) {
					int playsUntilNextSurvey = PlayerPrefs.GetInt(IFConstants.PlaysUntilNextSurvey, 16);
					if(--playsUntilNextSurvey == 0) {
						string url = IFGameManager.SharedManager.remoteURLs.NewSurvey + "?access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
						PlayerPrefs.SetInt(IFConstants.TotalSurveyRequests, totalSurveyRequests);
						PlayerPrefs.SetInt(IFConstants.PlaysUntilNextSurvey, 30);
						IFUtils.ShowNativeWebViewWithURL(url,null);
						StartCoroutine(DelayedGameStartBecauseOfSurveyBug(game, category));
						return;
					} else {
						PlayerPrefs.SetInt(IFConstants.PlaysUntilNextSurvey, playsUntilNextSurvey);
					}
				} else {
					PlayerPrefs.SetString(IFConstants.SurveyStatus, "declined");
				}
			}
		}
		DoGameStart(game, category);
	}

	public IEnumerator DelayedGameStartBecauseOfSurveyBug(IFGame game, IFQuestionCategory category)
	{
		yield return null;
		StartCoroutine(CheckSurveyStatus());
		DoGameStart(game, category);
	}
	
	public void DoGameStart(IFGame game, IFQuestionCategory category)
	{
		IFGameController gameController = GetGameController();
		gameController.categoryLabel.Text = category.Name;
		gameController.GameDidEndEvent += GameDidEnd;
		TransitionToController(gameController, TransitionDirection.LeftToRight, true, () => {
			PauseBackgroundMusic();
			UpdateSoundConfiguration(true);
			gameController.StartGame(game, category);
		});
	}
	
	public static bool GameIsPaused
	{
		get
		{
			return Time.timeScale == 0f;
		}
		set
		{
			if(value) {
				Time.timeScale = 0f;
			} else {
				Time.timeScale = 1f;
			}
		}
	}
	
	public void ToggleGamePause()
	{
		bool isPaused = IFGameManager.GameIsPaused;
		IFGameManager.GameIsPaused = !isPaused;
		NGUITools.Broadcast("GamePauseWasToggled", IFGameManager.GameIsPaused);
	}
	
	#region Controller Transitions
	
	public void TransitionToController(MonoBehaviour toController, TransitionDirection direction, bool destroy, Action completionCallback)
	{
		TransitionToController(toController, ActiveController, direction, destroy, completionCallback);
	}
	
	public void TransitionToController(MonoBehaviour toController, MonoBehaviour fromController, TransitionDirection direction, bool destroy, Action completionCallback)
	{
		float duration = .3f;
		
		toController.enabled = false;
		fromController.enabled = false;
		ActiveController = toController;
		
		Transform fromTransform = fromController.transform;
		Transform toTransform = toController.transform;

		toTransform.parent = mainPanel.cachedTransform;
		toTransform.localScale = Vector3.one;
		toTransform.localPosition = Vector3.zero;
		toTransform.localRotation = Quaternion.identity;
		
		Bounds fromControllerBounds = NGUIMath.CalculateRelativeWidgetBounds(fromTransform);
		Vector3 offscreenFromPosition = fromTransform.localPosition;
		
		Bounds toControllerBounds = NGUIMath.CalculateRelativeWidgetBounds(toTransform);
		Vector3 offscreenToPosition = toTransform.localPosition;
		
		if(direction == TransitionDirection.RightToLeft) {
			offscreenFromPosition.x -= fromControllerBounds.max.x * 2f;
			offscreenToPosition.x += toControllerBounds.max.x * 2f;
		}
		else if(direction == TransitionDirection.LeftToRight) {
			offscreenFromPosition.x += fromControllerBounds.max.x * 2f;
			offscreenToPosition.x -= toControllerBounds.max.x * 2f;
		}
		
		fromController.SendMessage("ControllerWillDisappear", destroy, SendMessageOptions.DontRequireReceiver);
		toController.SendMessage("ControllerWillAppear", destroy, SendMessageOptions.DontRequireReceiver);
		
		TweenPosition moveOut = TweenPosition.Begin(fromController.gameObject, duration, offscreenFromPosition);
		moveOut.method = UITweener.Method.EaseInOut;
		TweenAlpha fadeOut = TweenAlpha.Begin(fromController.gameObject, duration, 0f);
		fadeOut.method = UITweener.Method.EaseInOut;
		
		TweenPosition moveIn = TweenPosition.Begin(toController.gameObject, duration, Vector3.zero);
		moveIn.method = UITweener.Method.EaseInOut;
		moveIn.from = offscreenToPosition;
		moveIn.onFinished = (tween) => {
			toController.enabled = true;
			fromController.enabled = true;
			
			fromController.SendMessage("ControllerDidDisappear", destroy, SendMessageOptions.DontRequireReceiver);
			toController.SendMessage("ControllerDidAppear", destroy, SendMessageOptions.DontRequireReceiver);
			
			fromController.gameObject.SetActive(false);
			if(destroy) {
				IFGameManager.LoadableAssets.ClearCache();
			}
			if(completionCallback != null) {
				completionCallback();
			}
		};
	}
	
	public void TransitionToHomeScreen()
	{
		IFHomeScreenController homeScreen = IFGameManager.LoadableAssets.cachedHomeScreenController;
		if(homeScreen == null) {
			homeScreen = IFHomeScreenController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedHomeScreenController = homeScreen;
		}
		TransitionToController(homeScreen, TransitionDirection.RightToLeft, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToHighScoresScreen() { TransitionToHighScoresScreen(TransitionDirection.Default, null); }
	public void TransitionToHighScoresScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFHighScoresController highScores = IFGameManager.LoadableAssets.cachedHighScoreController;
		if(highScores == null) {
			highScores = IFHighScoresController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedHighScoreController = highScores;
		}
		highScores.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(highScores, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToLoginScreen() { TransitionToLoginScreen(TransitionDirection.Default, null); }
	public void TransitionToLoginScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFAccountController accountController = IFGameManager.LoadableAssets.cachedAccountController;
		if(accountController == null) {
			accountController = IFAccountController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedAccountController = accountController;
		}
		
		accountController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(accountController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToRegistrationScreen() { TransitionToRegistrationScreen(TransitionDirection.Default, null); }
	public void TransitionToRegistrationScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFRegistrationController registrationController = IFGameManager.LoadableAssets.cachedRegistrationController;
		if(registrationController == null) {
			registrationController = IFRegistrationController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedRegistrationController = registrationController;
		}
		
		registrationController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(registrationController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	public void TransitionToMultiplayerSetupScreen() { TransitionToMultiplayerSetupScreen(TransitionDirection.Default, null); }
	public void TransitionToMultiplayerSetupScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFMultiplayerSetupController multiplayerSetupController = IFGameManager.LoadableAssets.cachedMultiplayerSetupController;
		if(multiplayerSetupController == null) {
			multiplayerSetupController = IFMultiplayerSetupController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedMultiplayerSetupController = multiplayerSetupController;
		}
		TransitionToController(multiplayerSetupController, TransitionDirection.LeftToRight, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToMyGamesScreen(){ TransitionToMyGamesScreen(IFGameManager.TransitionDirection.Default, null); }
	public void TransitionToMyGamesScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFMyGamesController myGamesController = IFGameManager.LoadableAssets.cachedMyGamesController;
		if(myGamesController == null) {
			myGamesController = IFMyGamesController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedMyGamesController = myGamesController;
		}
		
		myGamesController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(myGamesController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToAboutScreen() { TransitionToAboutScreen(TransitionDirection.Default, null); }
	public void TransitionToAboutScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFAboutScreenController aboutController = IFGameManager.LoadableAssets.cachedAboutScreenController;
		if(aboutController == null) {
			aboutController = IFAboutScreenController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedAboutScreenController = aboutController;
		}
		
		aboutController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(aboutController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToSettingsScreen() { TransitionToSettingsScreen(TransitionDirection.Default, null); }
	public void TransitionToSettingsScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFSettingsController settingsController = IFGameManager.LoadableAssets.cachedSettingsController;
		if(settingsController == null) {
			settingsController = IFSettingsController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedSettingsController = settingsController;
		}
		
		settingsController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(settingsController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToReviewForChallenge(IFChallenge challenge) { TransitionToReviewForChallenge(challenge, TransitionDirection.Default, null); }
	public void TransitionToReviewForChallenge(IFChallenge challenge, TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFGameReviewController myReviewController = IFGameManager.LoadableAssets.cachedGameReviewController;
		if(myReviewController == null) {
			myReviewController = IFGameReviewController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedGameReviewController = myReviewController;
		}
		myReviewController.challenge = challenge;
		myReviewController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(myReviewController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToReviewForGame(IFGame game) { TransitionToReviewForGame(game, TransitionDirection.Default, null); }
	public void TransitionToReviewForGame(IFGame game, TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFGameReviewController myReviewController = IFGameManager.LoadableAssets.cachedGameReviewController;
		if(myReviewController == null) {
			myReviewController = IFGameReviewController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedGameReviewController = myReviewController;
		}
		myReviewController.game = game;
		myReviewController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(myReviewController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}

	public void TransitionToFeedbackScreen() { TransitionToFeedbackScreen(TransitionDirection.Default, null); }
	public void TransitionToFeedbackScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFFeedbackController feedbackController = IFGameManager.LoadableAssets.cachedFeedbackController;
		if(feedbackController == null) {
			feedbackController = IFFeedbackController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedFeedbackController = feedbackController;
		}

		feedbackController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(feedbackController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}

	public void TransitionToPasswordResetScreen() { TransitionToPasswordResetScreen(TransitionDirection.Default, null); }
	public void TransitionToPasswordResetScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFPasswordResetController passwordResetController = IFGameManager.LoadableAssets.cachedPasswordResetController;
		if(passwordResetController == null) {
			passwordResetController = IFPasswordResetController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedPasswordResetController = passwordResetController;
		}

		passwordResetController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(passwordResetController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}

	public void TransitionToProfileScreen() { TransitionToProfileScreen(TransitionDirection.Default, null); }
	public void TransitionToProfileScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFProfileController profileController = IFGameManager.LoadableAssets.cachedProfileController;
		if(profileController == null) {
			profileController = IFProfileController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedProfileController = profileController;
		}

		profileController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(profileController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}
	
	public void TransitionToHelpScreen() { TransitionToHelpScreen(TransitionDirection.Default, null); }
	public void TransitionToHelpScreen(TransitionDirection direction, ShouldTransitionToDefault transitionDelegate)
	{
		IFHelpScreenController helpController = IFGameManager.LoadableAssets.cachedHelpScreenController;
		if(helpController == null) {
			helpController = IFHelpScreenController.CreateFromPrefab();
			IFGameManager.LoadableAssets.cachedHelpScreenController = helpController;
		}
		
		helpController.shouldTransitionToDefaultDelegate = transitionDelegate;
		TransitionToController(helpController, direction, true, () => {
			UpdateSoundConfiguration(false);
		});
	}


	
	#endregion
	
	#region Network Coroutines
	
	public void StartGameResultUpload(IFGame game)
	{
		if(IsLoggedIn) {
			StartCoroutine(game.UploadScore());	
		} else {
			CreateAnonymousUser((success, error) => {
				if(success) {
					StartCoroutine(game.UploadScore());
				}
			});
		}
	}
	
	public void UpdateSettingsForUser(Hashtable settingsData, Action<bool, IFError> callback)
	{
		StartCoroutine(DoRegistrationOrSettingsUpdate(settingsData, false, callback));
	}
	
	public void RegisterNewUser(Hashtable registrationData, Action<bool, IFError> callback)
	{
		StartCoroutine(DoRegistrationOrSettingsUpdate(registrationData, true, callback));
	}

	public void CreateAnonymousUser(Action<bool, IFError> callback)
	{
		if(RegistrationOrSettingsUpdateIsInProgress) {
			StartCoroutine(WaitForSettingsUpdateToComplete(() => {
				callback(true, IFError.Null);
			}));
			return;
		}

		string username = "user" + IFUtils.GetRandomIntInRange(100, 1000000).ToString("D6");
		Hashtable data = new Hashtable();
		data["username"] = username;

		StartCoroutine(DoRegistrationOrSettingsUpdate(data, true, (success, error) => {
			IFGameManager.UserIsRegistered = false;
			callback(success, error);
		}));
	}

	private bool mRegistrationOrSettingsUpdateIsInProgress = false;
	public bool RegistrationOrSettingsUpdateIsInProgress {
		get
		{
			return mRegistrationOrSettingsUpdateIsInProgress;
		}
		private set
		{
			mRegistrationOrSettingsUpdateIsInProgress = value;
		}
	}



	IEnumerator WaitForSettingsUpdateToComplete(Action callback)
	{
		while(RegistrationOrSettingsUpdateIsInProgress) {
			yield return null;
		}
		callback();
	}

	IEnumerator DoRegistrationOrSettingsUpdate(Hashtable data, bool newRegistration, Action<bool, IFError> callback)
	{
		while(RegistrationOrSettingsUpdateIsInProgress) {
			yield return null;
		}

		RegistrationOrSettingsUpdateIsInProgress = true;
		if(newRegistration) {
			data["duid"] = SystemInfo.deviceUniqueIdentifier;	
		}

		string accessToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey, null);
		if(accessToken != null) {
			data["access_token"] = accessToken;
		}

		string remoteUserId = null;
		if(PlayerPrefs.HasKey(IFConstants.RemoteUserIdKey)) {
			remoteUserId = PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey).ToString();
		}

		Hashtable headers = new Hashtable();
		headers["Content-Type"] = "application/json";
		headers["Accept"] = "application/json";
		
		string jsonString = MiniJSON.jsonEncode(data);
		
		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;
		
		string url;
		if(remoteUserId != null && accessToken != null) {
			url = IFGameManager.SharedManager.remoteURLs.Users + "/" + remoteUserId;
		} else {
			url = IFGameManager.SharedManager.remoteURLs.SignUp;
		}
		
		WWW web = new WWW(url, Encoding.UTF8.GetBytes(jsonString), headers);
		yield return web;
		
		indicator.Dismiss();
		
		if(web.error == null) {
			Hashtable responseHash = MiniJSON.jsonDecode(web.text) as Hashtable;
			
			if(responseHash != null && responseHash.ContainsKey("error")) {
				string title = responseHash["error"] as string;
				StringBuilder sb = new StringBuilder();
				if(responseHash.ContainsKey("error_messages")) {
					ArrayList errorMessages = (ArrayList)responseHash["error_messages"];
					foreach(Hashtable msg in errorMessages) {
						foreach(string key in msg.Keys) {
							sb.AppendLine(IFUtils.SafeLocalize(key + " " + msg[key]));	
						}
					}
					callback(false, new IFError(IFUtils.SafeLocalize(title), sb.ToString()));
				}
			} else {
				PlayerPrefs.DeleteAll();
				IFGame.ClearAllGames();
				IFChallenge.ClearAllChallenges();
				IFGameManager.UserIsRegistered = true;
				
				Hashtable hash = data;
				if(responseHash != null) {
					foreach(string key in responseHash.Keys) {
						data[key] = responseHash[key];
					}
				}

				if(hash != null) {
					if(hash.ContainsKey("access_token")) {
						PlayerPrefs.SetString(IFConstants.AccessTokenPrefsKey, hash["access_token"] as string);
					}
					
					if(hash.ContainsKey("id")) {
						PlayerPrefs.SetInt(IFConstants.RemoteUserIdKey, Convert.ToInt32(hash["id"]));
					} else if(remoteUserId != null) {
						PlayerPrefs.SetInt(IFConstants.RemoteUserIdKey, Convert.ToInt32(remoteUserId));
					}
					
					if(hash.ContainsKey("facebook_id")) {
						PlayerPrefs.SetString(IFConstants.FacebookId, hash["facebook_id"] as string);
					}
					
					if(hash.ContainsKey("email")) {
						PlayerPrefs.SetString(IFConstants.EmailAddressPrefsKey, hash["email"] as string);
					}
	
					if(hash.ContainsKey("username")) {
						PlayerPrefs.SetString(IFConstants.UsernamePrefsKey, hash["username"] as string);
					}
					
					if(hash.ContainsKey("gender")) {
						PlayerPrefs.SetString(IFConstants.UserGenderPrefsKey, hash["gender"] as string);
					}
					
					if(hash.ContainsKey("location")) {
						PlayerPrefs.SetString(IFConstants.UserLocationPrefsKey, hash["location"] as string);
					}
					
					if(hash.ContainsKey("age_range")) {
						PlayerPrefs.SetString(IFConstants.UserAgeRangePrefsKey, hash["age_range"] as string);
					}
				}
				PlayerPrefs.Save();
				callback(true, IFError.Null);
			}
		} else {
			if(web.responseHeaders["STATUS"].Contains("422") || web.responseHeaders["STATUS"].Contains("500")) {
				bool was422 = web.responseHeaders["STATUS"].Contains("422");
				callback(false, new IFError(newRegistration ? Localization.Localize("Registration Error") : Localization.Localize("Setting Update Error"), Localization.Localize("Could not save your account information.") + " (" +(was422 ? "422" : "500")+")."));
			} else {
				callback(false, new IFError(newRegistration ? Localization.Localize("Registration Error") : Localization.Localize("Setting Update Error"), Localization.Localize("Please try again.")));
			}

		}
		RegistrationOrSettingsUpdateIsInProgress = false;
	}

	public void QueueFeedbackComment(string comment)
	{
		Hashtable data = new Hashtable();
		string accessToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey, null);
		if(!string.IsNullOrEmpty(accessToken)) {
			data["access_token"] = accessToken;
		}
		data["comment"] = comment;

		string jsonString = MiniJSON.jsonEncode(data);

		IFUploadManager.QueueUploadDataForEndpoint(IFGameManager.SharedManager.remoteURLs.feedbackPath, jsonString);
		IFUploadManager.SharedManager.FlushNow();
//		StartCoroutine(SubmitFeedbackComment(comment));
	}

//	IEnumerator SubmitFeedbackComment(string comment)
//	{
//		Hashtable headers = new Hashtable();
//		headers["Content-Type"] = "application/json";
//		headers["Accept"] = "application/json";
//
//		Hashtable data = new Hashtable();
//		string accessToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey, null);
//		if(!string.IsNullOrEmpty(accessToken)) {
//			data["access_token"] = accessToken;
//		}
//		data["comment"] = comment;
//
//		string jsonString = MiniJSON.jsonEncode(data);
//
//		WWW web = new WWW(IFGameManager.SharedManager.remoteURLs.Feedback, Encoding.UTF8.GetBytes(jsonString), headers);
//		yield return web;
//
//		if(web.error != null) {
//			Debug.Log("Feedback not saved: "+web.error);
//		}
//	}

	#endregion
	
}
