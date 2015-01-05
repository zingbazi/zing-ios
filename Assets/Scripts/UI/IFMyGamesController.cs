// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class IFMyGamesController : MonoBehaviour {
	
	private UIPanel mPanel;
	public UISortableGrid grid;
	public UIDraggablePanel draggablePanel;
	public UIPanel scrollingPanel;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	public GameObject noGamesMessage;
	private Transform gridTransform;
	private bool isFetchingChallenges;
	private bool needsTableUpdate;
	private bool needsDataUpdate;
	private bool isPreparingGame = false;
	
	public static IFMyGamesController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.MyGamesScreenPrefab == null) {
			return IFMyGamesController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.MyGamesScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFMyGamesController>();
	}
	
	public static IFMyGamesController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFMyGamesController>();
	}

	public static IFMyGamesController Create()
	{
		return Create("My Games Screen");
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
		gridTransform = grid.GetComponent<Transform>();
		needsDataUpdate = true;
		needsTableUpdate = true;
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
		IFActivityIndicator.DismissAll();
	}
	
	void Update()
	{
		if(needsTableUpdate) {
			UpdateTable();
		}
		if(needsDataUpdate) {
			if(!IFGameManager.IsLoggedIn) {
				needsDataUpdate = false;
				IFGameManager.SharedManager.CreateAnonymousUser((success, error) => {
					needsDataUpdate = true;
					StartCoroutine(FetchChallenges());
				});
			} else {
				StartCoroutine(FetchChallenges());	
			}
		}

	}

	IEnumerator FetchChallenges()
	{
		if(!isFetchingChallenges && needsDataUpdate) {
			isFetchingChallenges = true;

			UriBuilder uriBuilder = new UriBuilder(IFGameManager.SharedManager.remoteURLs.Challenges);
			string authToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
			if(authToken != null) {
				uriBuilder.Query = "access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);	
			}

			IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
			indicator.color = Color.black;

			WWW web = new WWW(uriBuilder.Uri.AbsoluteUri);
			yield return web;
			indicator.Dismiss();
			
			if(web.error == null) {
				ArrayList responseList = MiniJSON.jsonDecode(web.text) as ArrayList;
				needsTableUpdate = IFChallenge.MergeRemoteChallengeList(responseList);
			} else {
				IFAlertViewController.ShowAlert(Localization.Localize("Please try again."), Localization.Localize("Error fetching challenges"));
			}
			isFetchingChallenges = false;
			needsDataUpdate = false;
		}
	}
	
	void UpdateTable()
	{
		scrollingPanel.widgetsAreStatic = false;
		List<IFChallenge> challenges = IFChallenge.GetAllChallenges();
		if(challenges == null || challenges.Count == 0) {
			noGamesMessage.SetActive(true);
			IFActivityIndicator.DismissAll();
		} else {
			noGamesMessage.SetActive(false);
			IFGameCell[] reusableCells = grid.GetComponentsInChildren<IFGameCell>();
			for(int i = 0; i < challenges.Count; i++) {
				GameObject cellGO;
				IFGameCell cell;
	
				if(i < reusableCells.Length) {
					cellGO = reusableCells[i].gameObject;
					cell = reusableCells[i];
				} else {
					cellGO = Instantiate(IFGameManager.LoadableAssets.ChallengeGameCellPrefab) as GameObject;
					NGUITools.Destroy(cellGO.GetComponent<UIPanel>());
					
					cellGO.transform.parent = gridTransform;
					cellGO.transform.localScale = Vector3.one;
					
					cell = cellGO.GetComponent<IFGameCell>();
				}
				cell.DraggablePanel = draggablePanel;
				cell.Challenge = challenges[i];
				cell.CellWasSelectedDelegate = ChallengeCellWasSelected;
			}
			grid.Reposition();
			draggablePanel.ResetPosition();
		}
		
		scrollingPanel.widgetsAreStatic = true;
		needsTableUpdate = false;
	}

	public void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
	
	public void ChallengeCellWasSelected(IFGameCell cell, IFChallenge challenge)
	{
		bool userComplete = challenge.WasCreator ? challenge.UserAnswerCount == challenge.QuestionCount : challenge.OpponentAnswerCount == challenge.QuestionCount;
		if(userComplete || challenge.State == IFChallenge.ChallengeState.Complete) {;
			IFGameManager.SharedManager.TransitionToReviewForChallenge(challenge, IFGameManager.TransitionDirection.Default, () => {
				IFGameManager.SharedManager.TransitionToMyGamesScreen(IFGameManager.TransitionDirection.RightToLeft, null);
				return false;
			});
		} else if(!isPreparingGame) {
			IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
			indicator.color = Color.black;

			isPreparingGame = true;
			IFGame game = IFGame.GameWithRemoteId(challenge.RemoteGameId);
			if(game == null) {
				indicator.Dismiss();
				StartCoroutine(FetchRemoteGameForChallenge(challenge, (remoteGame) => {
					indicator.Dismiss();
					IFGameManager.SharedManager.StartGame(remoteGame);
				}));
			} else {
				indicator.Dismiss();
				game.challenge = challenge;
				isPreparingGame = false;
				IFGameManager.SharedManager.StartGame(game);
			}
			
		}
	}
	
	IEnumerator FetchRemoteGameForChallenge(IFChallenge challenge, Action<IFGame> callback)
	{
		int remoteGameId = challenge.RemoteGameId;
		if(challenge.UserId != PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey)) {
			remoteGameId = challenge.OpponentRemoteGameId;
		}

		UriBuilder uriBuilder = new UriBuilder(IFGameManager.SharedManager.remoteURLs.Games + "/" + remoteGameId.ToString());
		string authToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
		if(authToken != null) {
			uriBuilder.Query = "access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);	
		}
		
		WWW web = new WWW(uriBuilder.Uri.AbsoluteUri);
		yield return web;
		
		if(web.error == null) {
			Hashtable responseHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			IFGame game = new IFGame(IFGameLevel.LevelWithRank(1), IFGame.GameMode.Challenge);
			game.RemoteId = remoteGameId;
			
			object scoreObject = responseHash["score"];
			if(scoreObject != null) {
				game.Score = Convert.ToInt32(scoreObject);
			}
			
			string dateString = (string)responseHash["date"];
			if(dateString != null) {
				game.Date = DateTime.Parse(dateString);
			}
			
			string state = (string)responseHash["state"];
			if(state == null || state.Equals("open")) {
				game.Completed = false;
			} else {
				game.Completed = true;
			}
			
			game.Uploaded = true;
			game.Save();
			game.challenge = challenge;
			callback(game);
		} else {
			IFAlertViewController.ShowAlert(Localization.Localize("Please try again."), "Error fetching game");
			callback(null);
		}
	}
}
