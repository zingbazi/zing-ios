// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class IFHomeScreenController : MonoBehaviour
{
	public IFSysFontButton myGamesButton;
	public UILabel gameCountLabel;
	public IFSysFontButton loginButton;
	public UITableViewPanel categoryTableView;

	public UIDraggablePanel categoryScrollingPanel;

	public GameObject categoryListWidget;
	
	private IFLocalize loginButtonLocalization;
	private UIPanel mPanel;
	private bool isSyncingChallenges;
	private List<IFQuestionCategory> categories;

	public static IFHomeScreenController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.HomeScreenPrefab == null) {
			return IFHomeScreenController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.HomeScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFHomeScreenController>();
	}
	
	public static IFHomeScreenController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFHomeScreenController>();
	}

	public static IFHomeScreenController Create()
	{
		return Create("Home Screen");
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
		
		loginButtonLocalization = loginButton.GetComponentInChildren<IFLocalize>();
		
		if(IFGameManager.IsLoggedIn && IFGameManager.UserIsRegistered) {
			loginButtonLocalization.key = "Log Out";
			loginButtonLocalization.Localize();
		} else {
			loginButtonLocalization.key = "Log In";
			loginButtonLocalization.Localize();
		}
		
		gameCountLabel.text = IFChallenge.CountOfPlayableChallenges().ToString();
//		string lastSyncTimeString = PlayerPrefs.GetString(IFConstants.LastChallengeSync);
//		if(string.IsNullOrEmpty(lastSyncTimeString)) {
			StartCoroutine(FetchAndSyncChallenges());
//		} else {
//			DateTime lastSyncTime = DateTime.Parse(lastSyncTimeString);
//			if(lastSyncTime.Subtract(IFUtils.TimeAtStartup).TotalMilliseconds < 0) {
//				StartCoroutine(FetchAndSyncChallenges());
//			}
//		}
		
	}
	
	void OnDisable()
	{
		IFActivityIndicator.DismissAll();
	}

	void ShowCategorySelectionTable()
	{
		categories = IFQuestionCategory.AllCategories();
		categoryTableView.Reset();
		categoryTableView.cellCount = categories.Count;
		categoryTableView.configureCellForRow = ConfigureCategoryCellForRow;

		TweenAlpha fadeIn = TweenAlpha.Begin(categoryListWidget, .2f, 1f);
		fadeIn.from = 0f;
		fadeIn.method = UITweener.Method.EaseInOut;

		TweenScale fallIn = TweenScale.Begin(categoryListWidget, .3f, Vector3.one);
		fallIn.from = new Vector3(1.8f, 1.8f, 1f);
		fallIn.method = UITweener.Method.EaseInOut;

		categoryListWidget.SetActive(true);
	}

	void ConfigureCategoryCellForRow(GameObject cell, int index)
	{
		IFCategoryCell categoryCell = cell.GetComponent<IFCategoryCell>();
		categoryCell.Category = categories[index];
		UIEventListener listener = UIEventListener.Get(cell);
		listener.onClick = CategorySelected;
	}

	void CategorySelected(GameObject sender)
	{
		IFCategoryCell selected = sender.GetComponent<IFCategoryCell>();
		IFGameManager.SharedManager.StartSoloPlayGame(selected.Category);
		HideCategorySelectionList();
	}

	void HideCategorySelectionList()
	{
		TweenAlpha fadeOut = TweenAlpha.Begin(categoryListWidget, .2f, 0f);
		fadeOut.from = 1f;
		fadeOut.method = UITweener.Method.EaseInOut;

		TweenAlpha fadeOut2 = TweenAlpha.Begin(categoryScrollingPanel.gameObject, .3f, 0f);
		fadeOut2.from = 1f;
		fadeOut2.method = UITweener.Method.EaseInOut;

		TweenScale fallOut = TweenScale.Begin(categoryListWidget, .3f, new Vector3(1.8f, 1.8f, 1f));
		fallOut.from = Vector3.one;
		fallOut.method = UITweener.Method.EaseInOut;
		fallOut.onFinished += (tween) => {
			categoryListWidget.gameObject.SetActive(false);
			categoryScrollingPanel.GetComponent<UIPanel>().alpha = 1f;
		};
	}


	IEnumerator FetchAndSyncChallenges()
	{
		yield return null;
		if(!isSyncingChallenges && IFGameManager.IsLoggedIn) {
			isSyncingChallenges = true;

			UriBuilder uriBuilder = new UriBuilder(IFGameManager.SharedManager.remoteURLs.Challenges);
			string authToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
			if(authToken != null) {
				uriBuilder.Query = "access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);	
			}
			
			Transform countLabelTransform = gameCountLabel.cachedTransform;
			IFActivityIndicator activityIndicator = IFActivityIndicator.CreateFloatingActivityIndicator();
			activityIndicator.color = Color.white;
			activityIndicator.cachedTransform.position = countLabelTransform.position;
			UISprite sprite = activityIndicator.GetComponentInChildren<UISprite>();
			sprite.MakePixelPerfect();
			Vector3 spinnerSize = sprite.cachedTransform.localScale;
			activityIndicator.cachedTransform.localScale = new Vector3(countLabelTransform.localScale.x/spinnerSize.x, countLabelTransform.localScale.y/spinnerSize.y, 1f);			
			
			gameCountLabel.gameObject.SetActive(false);

			WWW web = new WWW(uriBuilder.Uri.AbsoluteUri);
			yield return web;
			
			if(web.error == null) {
				ArrayList responseList = (ArrayList)MiniJSON.jsonDecode(web.text);
				IFDatabase.SharedDatabase.BeginTransaction();
				foreach(Hashtable challengeHash in responseList) {
					yield return null;
					IFChallenge.MergeRemoteChallengeItem(challengeHash);
				}
				IFDatabase.SharedDatabase.CommitTransaction();
			}
			isSyncingChallenges = false;
			PlayerPrefs.SetString(IFConstants.LastChallengeSync, DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
			gameCountLabel.text = IFChallenge.CountOfPlayableChallenges().ToString();
			gameCountLabel.gameObject.SetActive(true);
			activityIndicator.Dismiss();
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
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}
	
	public void SoloPlayButtonTapped()
	{
		IFGameManager.SharedManager.StartSoloPlayGame(IFQuestionCategory.BiggestCategory());
	}
	
	public void ChallengeButtonTapped()
	{
		IFGameManager.SharedManager.TransitionToMultiplayerSetupScreen();
	}
	
	public void HighScoresButtonTapped()
	{
		IFGameManager.SharedManager.TransitionToHighScoresScreen();
	}
	
	public void LoginButtonTapped()
	{
		if(IFGameManager.IsLoggedIn && IFGameManager.UserIsRegistered) {
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			IFChallenge.ClearAllChallenges();
			IFGame.ClearAllGames();
			IFAlertViewController.ShowAlert(Localization.Localize("You have been logged out."), Localization.Localize("Logged out"), Localization.Localize("OK"), (controller, okWasSelected) => {
				loginButtonLocalization.key = "Log In";
				loginButtonLocalization.Localize();
			});
		} else {
			IFGameManager.SharedManager.TransitionToLoginScreen();
		}
	}
	
	public void MyGamesButtonTapped()
	{
		IFGameManager.SharedManager.TransitionToMyGamesScreen();
	}
	
	public void AboutButtonTapped()
	{
		IFGameManager.SharedManager.TransitionToAboutScreen();
	}
	
	public void SettingsButtonTapped()
	{
		IFGameManager.SharedManager.TransitionToProfileScreen();
	}
}
