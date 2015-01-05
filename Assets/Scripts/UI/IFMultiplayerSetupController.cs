// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

public class IFMultiplayerSetupController : MonoBehaviour {
	
	public UICheckbox[] options;
	public UISysFontLabel facebookButtonLabel;
	public UISysFontInput usernameInput;
	public UISysFontPopupList categoryPopupList;
	public UITableViewPanel friendTableView;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	private List<Friend> facebookFriends;
	
	public GameObject friendListWidget;
	public UIDraggablePanel friendScrollingPanel;
	public Color cellSelectionColor;
	private string selectedFacebookFriendId;
	private string selectedFacebookFriendName;
	private struct Friend {
		public string name;
		public string id;
		
		public Friend(string n, string i)
		{
			name = n;
			id = i;
		}
	}
	
	private UIPanel mPanel;
	
	public static IFMultiplayerSetupController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.MultiplayerSetupScreenPrefab == null) {
			return IFMultiplayerSetupController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.MultiplayerSetupScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFMultiplayerSetupController>();
	}
	
	public static IFMultiplayerSetupController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFMultiplayerSetupController>();
	}

	public static IFMultiplayerSetupController Create()
	{
		return Create("Multiplayer Setup Screen");
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
	
	void OnDisable()
	{
		IFActivityIndicator.DismissAll();
	}
	
	void Start()
	{
		mPanel = GetComponentInChildren<UIPanel>();
		List<string> categoryOptions = new List<string>();
		foreach(IFQuestionCategory category in IFQuestionCategory.AllCategories()) {
			categoryOptions.Add(category.Name);
		}
		categoryPopupList.items = categoryOptions;
		categoryPopupList.selection = categoryOptions[0];
		friendTableView.configureCellForRow = ConfigureFacebookFriendCellForRow;
		TouchScreenKeyboard.hideInput = true;
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}

	void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
	
	void FacebookButtonTapped()
	{
#if UNITY_EDITOR
		facebookFriends = new List<Friend>();
		facebookFriends.Add(new Friend("Nathan Eror", "12345"));
		facebookFriends.Add(new Friend("Cory Bohon", "123456"));
		facebookFriends.Add(new Friend("Alan Bradburne", "12347"));
		facebookFriends.Add(new Friend("Kyle Richter", "12348"));
		facebookFriends.Add(new Friend("Elliot Eror", "12349"));
		facebookFriends.Add(new Friend("Nathan Eror 2", "12345"));
		facebookFriends.Add(new Friend("Cory Bohon 2", "123456"));
		facebookFriends.Add(new Friend("Alan Bradburne 2", "12347"));
		facebookFriends.Add(new Friend("Kyle Richter 2", "12348"));
		facebookFriends.Add(new Friend("Elliot Eror 2", "12349"));
		facebookFriends.Add(new Friend("Nathan Eror 3", "12345"));
		facebookFriends.Add(new Friend("Cory Bohon 3", "123456"));
		facebookFriends.Add(new Friend("Alan Bradburne 3", "12347"));
		facebookFriends.Add(new Friend("Kyle Richter 3", "12348"));
		facebookFriends.Add(new Friend("Elliot Eror 3", "12349)"));
		ShowFacebookFriendTable();
		
#else
		if(facebookFriends != null) {
			ShowFacebookFriendTable();
		} else {
			if(!IFGameManager.UserIsRegistered) {
				IFAlertViewController.ShowAlert(Localization.Localize("You must have register an account to choose facebook friends"), Localization.Localize("Please Register"), Localization.Localize("OK"), (controller, okWasSelected) => {
					IFGameManager.SharedManager.TransitionToRegistrationScreen();
				});
			} else {
				Action FacebookFriendSearch = () => {
					if(IFFacebookBinding.isSessionValid()) {
						if(facebookFriends == null) {
							StartFacebookFriendSearch();
						}
					} else {
						FacebookManager.sessionOpenedEvent += () => {
							if(facebookFriends == null) {
								StartFacebookFriendSearch();
							}
						};
						string[] permissions = new string[] { "email", "user_relationships" };
						IFFacebookBinding.loginWithReadPermissions(permissions);
					}
				};
				IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
				indicator.color = Color.black;

				if(!IFFacebookBinding.IsReady) {
					IFFacebookBinding.init((success, error) => {
						indicator.Dismiss();
						if(success) {
							FacebookFriendSearch();
						} else {
							IFAlertViewController.ShowAlert(error.message, error.title);
						}
					});
				} else {
					IFFacebookBinding.CheckReachability((success, error) => {
						indicator.Dismiss();
						if(success) {
							FacebookFriendSearch();
						} else {
							IFAlertViewController.ShowAlert(error.message, error.title);
						}
					});
				}
			}
		}
			
#endif
	}

	void FacebookBasicCompletionHandler(string error, object result)
	{
		if(error != null && result != null) {
			IFAlertViewController.ShowAlert(error == null ? Localization.Localize("Error") : error, Localization.Localize("Can't find facebook friends"));
		} else {
			Hashtable resultHash = result as Hashtable;
			if(resultHash.ContainsKey("data")) {
				ArrayList friends = resultHash["data"] as ArrayList;
				facebookFriends = new List<Friend>();
				foreach(Hashtable friend in friends) {
					facebookFriends.Add(new Friend(friend["name"].ToString(), friend["id"].ToString()));
				}
				facebookFriends.Sort((x, y) => { return x.name.CompareTo(y.name); });
				ShowFacebookFriendTable();
			}
		}
		IFActivityIndicator.DismissAll();
	}
	
	void ConfigureFacebookFriendCellForRow(GameObject cell, int index)
	{
		Friend friend = facebookFriends[index];
		IFFriendCell friendCell = cell.GetComponent<IFFriendCell>();
		friendCell.FriendId = friend.id;
		friendCell.FriendName = friend.name;
		UIEventListener listener = UIEventListener.Get(cell);
		listener.onClick = FacebookFriendSelected;
	}
	
	void ShowFacebookFriendTable()
	{
		friendTableView.Reset();
		friendTableView.cellCount = facebookFriends.Count;
		friendTableView.configureCellForRow = ConfigureFacebookFriendCellForRow;
		
		TweenAlpha fadeIn = TweenAlpha.Begin(friendListWidget, .2f, 1f);
		fadeIn.from = 0f;
		fadeIn.method = UITweener.Method.EaseInOut;
		
		TweenScale fallIn = TweenScale.Begin(friendListWidget, .3f, Vector3.one);
		fallIn.from = new Vector3(1.8f, 1.8f, 1f);
		fallIn.method = UITweener.Method.EaseInOut;
		
		friendListWidget.SetActive(true);
	}
	
	void FacebookFriendSelected(GameObject sender)
	{
		IFFriendCell selected = sender.GetComponent<IFFriendCell>();
		selectedFacebookFriendId = selected.FriendId;
		selectedFacebookFriendName = selected.FriendName;
		facebookButtonLabel.Text = selectedFacebookFriendName;
		HideFacebookFriendList();
	}
	
	void HideFacebookFriendList()
	{
		TweenAlpha fadeOut = TweenAlpha.Begin(friendListWidget, .2f, 0f);
		fadeOut.from = 1f;
		fadeOut.method = UITweener.Method.EaseInOut;
		
		TweenAlpha fadeOut2 = TweenAlpha.Begin(friendScrollingPanel.gameObject, .3f, 0f);
		fadeOut2.from = 1f;
		fadeOut2.method = UITweener.Method.EaseInOut;
		
		TweenScale fallOut = TweenScale.Begin(friendListWidget, .3f, new Vector3(1.8f, 1.8f, 1f));
		fallOut.from = Vector3.one;
		fallOut.method = UITweener.Method.EaseInOut;
		fallOut.onFinished += (tween) => {
			friendListWidget.gameObject.SetActive(false);
			friendScrollingPanel.GetComponent<UIPanel>().alpha = 1f;
		};
	}

	void BeginButtonTapped()
	{
		foreach(UICheckbox option in options) {
			if(option.isChecked) {
				IFMultiplayerGameOpponentOption opponentOption = option.GetComponent<IFMultiplayerGameOpponentOption>();
				switch(opponentOption.opponentType)
				{
					case IFMultiplayerGameOpponentOption.OpponentType.Local:
						IFGame game = new IFGame(IFGameLevel.GetRandomLevel(), IFGame.GameMode.PassAndPlay);
						IFGameManager.SharedManager.StartGame(game, IFQuestionCategory.CategoryWithName(categoryPopupList.selection));
						break;
					case IFMultiplayerGameOpponentOption.OpponentType.Automatch:
						if(!IFGameManager.IsLoggedIn) {
							IFGameManager.SharedManager.CreateAnonymousUser((success, error) => {
								StartCoroutine(CreateChallenge(-1));
							});
						} else {
							StartCoroutine(CreateChallenge(-1));
						}
						break;
					case IFMultiplayerGameOpponentOption.OpponentType.Username:
						string username = usernameInput.text;
						if(!IFGameManager.IsLoggedIn) {
							IFGameManager.SharedManager.CreateAnonymousUser((success, error) => {
								StartCoroutine(FindUserIdForChallenge(username));
							});
						} else {
							StartCoroutine(FindUserIdForChallenge(username));
						}
						break;
					case IFMultiplayerGameOpponentOption.OpponentType.Facebook:
						if(selectedFacebookFriendId != null) {
							StartCoroutine(FindFacebookUserIdForChallenge(selectedFacebookFriendId));
						}
						break;
					default: break;
				}
			}
		}
	}
	
	void InfoButtonTapped()
	{
		TextAsset multiplayerInstructions = Resources.Load("multi_player_help") as TextAsset;
		IFAlertViewController.ShowAlert(multiplayerInstructions.text, Localization.Localize("Multiplayer Game Guide"), Localization.Localize("OK"), null);
	}
	
	void StartFacebookFriendSearch()
	{
		IFActivityIndicator.CreateFloatingActivityIndicator().color = Color.black;
		FacebookManager.sessionOpenedEvent += () => {
			Facebook.instance.getFriends(FacebookBasicCompletionHandler);
		};
		
		if(!IFFacebookBinding.isSessionValid()) {
			var permissions = new string[] { "email" };
			IFFacebookBinding.loginWithReadPermissions(permissions);
		} else {
			Facebook.instance.getFriends(FacebookBasicCompletionHandler);
		}
	}
	
	IEnumerator FindFacebookUserIdForChallenge(string facebookId)
	{
		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;
		
		WWW web = new WWW(IFGameManager.SharedManager.remoteURLs.Users + "/" + facebookId + "/facebook");
		yield return web;
		
		indicator.Dismiss();
		
		if(web.error == null) {
			Hashtable responseHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			
			if(responseHash != null && responseHash.ContainsKey("id")) {
				int userid = Convert.ToInt32(responseHash["id"]);
				StartCoroutine(CreateChallenge(userid));
			}
		} else {
			if(web.error.Contains("404")) {
				IFAlertViewController.ShowAlert(Localization.Localize("No Zing! account for")+" "+selectedFacebookFriendName, Localization.Localize("Facebook Error"));
			} else {
				IFAlertViewController.ShowAlert(Localization.Localize("Please try again."), Localization.Localize("Error finding facebook user user")+" "+selectedFacebookFriendName);
			}
		}
	}
	
	IEnumerator FindUserIdForChallenge(string username)
	{
		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;
		
		string url = IFGameManager.SharedManager.remoteURLs.Users + "/" + WWW.EscapeURL(username, Encoding.UTF8);
		WWW web = new WWW(url);
		yield return web;
		
		indicator.Dismiss();
		
		if(web.error == null) {
			Hashtable responseHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			
			if(responseHash != null && responseHash.ContainsKey("id")) {
				int userid = Convert.ToInt32(responseHash["id"]);
				StartCoroutine(CreateChallenge(userid));
			}
		} else {
			IFAlertViewController.ShowAlert(Localization.Localize("No Zing! account for")+" "+username, Localization.Localize("Can't Start Game"));
		}
	}
	
	IEnumerator CreateChallenge(int userId)
	{
		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;

		IFGame game = new IFGame(IFGameLevel.GetRandomLevel(), IFGame.GameMode.Challenge);
		
		Hashtable challengeCreationHash = new Hashtable();
		challengeCreationHash["access_token"] = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
		Hashtable headers = new Hashtable();
		headers["Content-Type"] = "application/json";
		headers["Accept"] = "application/json";
		
		Queue<int> localQuestionIdQueue = game.Level.GetRandomQuestionIdQueue(IFQuestionCategory.CategoryWithName(categoryPopupList.selection));
		if(localQuestionIdQueue.Count < game.Level.QuestionCount) {
			List<int> queue = localQuestionIdQueue.ToList();
			Queue<int> filler = game.Level.GetRandomQuestionIdQueue(game.Level.QuestionCount - localQuestionIdQueue.Count, true);
			queue.AddRange(filler);
			localQuestionIdQueue = new Queue<int>(queue);
		}
		int questionCount = localQuestionIdQueue.Count;
		
		int[] remoteQuestionIds = new int[questionCount];
		for(int i = 0; i < questionCount; i++) {
			remoteQuestionIds[i] = IFQuestion.RemoteIdForQuestionWithIdentifier(localQuestionIdQueue.Dequeue());
		}
		
		challengeCreationHash["question_ids"] = remoteQuestionIds;
		
		if(userId >= 0) {
			challengeCreationHash["opponent_id"] = userId;
		}		
		string jsonString = MiniJSON.jsonEncode(challengeCreationHash);
		
		WWW web = new WWW(IFGameManager.SharedManager.remoteURLs.Challenges, Encoding.UTF8.GetBytes(jsonString), headers);
		yield return web;
		
		if(web.error == null) {
			Hashtable responseHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			
			if(responseHash.ContainsKey("id")) {
				int challengeId = Convert.ToInt32(responseHash["id"]);
				IFChallenge challenge;
				if(IFChallenge.ChallengeWithRemoteIdExists(challengeId)) {
					challenge = IFChallenge.ChallengeWithRemoteId(challengeId);
				} else {
					challenge = new IFChallenge();
					challenge.RemoteId = challengeId;
				}
				
				string stateString = (string)responseHash["state"];
				if(stateString.Equals("open")) {
					challenge.State = IFChallenge.ChallengeState.Open;
				} else {
					challenge.State = IFChallenge.ChallengeState.Complete;
				}
				challenge.QuestionCount = Convert.ToInt32(responseHash["questions_count"]);
				challenge.WasCreator = true;
				challenge.DidWin = false;
				
				Hashtable userHash = (Hashtable)responseHash["user"];
				Hashtable opponentHash = (Hashtable)responseHash["opponent"];
				
				game.RemoteId = Convert.ToInt32(userHash["game_id"]);
				challenge.RemoteGameId = game.RemoteId;
				challenge.Game = game;
				game.Save();
				
				challenge.UserId = Convert.ToInt32(userHash["id"]);
				challenge.Username = (string)userHash["username"];
				challenge.UserScore = 0;
				challenge.UserAnswerCount = 0;
				
				challenge.OpponentUserId = Convert.ToInt32(opponentHash["id"]);
				challenge.OpponentUsername = (string)opponentHash["username"];
				challenge.OpponentScore = 0;
				challenge.OpponentAnswerCount = 0;
				
				challenge.RemoteQuestionIds = remoteQuestionIds;
				
				challenge.Save();
				game.challenge = challenge;
				IFGameManager.SharedManager.StartGame(game, IFQuestionCategory.CategoryWithName(categoryPopupList.selection));
			}
		} else {
			IFAlertViewController.ShowAlert(Localization.Localize("Please try again."), Localization.Localize("Error creating challenge"));
		}
		indicator.Dismiss();
	}
}
