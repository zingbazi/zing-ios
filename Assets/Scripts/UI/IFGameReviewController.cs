// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;

public class IFGameReviewController : MonoBehaviour {

	private UIPanel mPanel;
	public UISortableGrid grid;
	public UIDraggablePanel draggablePanel;
	public UIPanel scrollingPanel;
	public IFGame game;
	public IFChallenge challenge;
	public IFPageControl pageControl;
	public UISysFontLabel pageCountIndicator;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	private Transform gridTransform;
	private bool isFetchingChallenges;
	private bool needsTableUpdate;
	private bool needsDataUpdate;
	private IFAnswer[] answers;
	private IFAnswer[] opponentAnswers;
	private Transform dragPanelTransform;
	private int lastPageIndex = -1;
	private int lastPageCount = 0;
	
	public static IFGameReviewController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.ReviewScreenPrefab == null) {
			return IFGameReviewController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.ReviewScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFGameReviewController>();
	}
	
	public static IFGameReviewController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFGameReviewController>();
	}

	public static IFGameReviewController Create()
	{
		return Create("Review Screen");
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
		dragPanelTransform = draggablePanel.panel.cachedTransform;
		needsDataUpdate = true;
		float screenWidth = Screen.width * UIRoot.GetPixelSizeAdjustment(panel.cachedGameObject);
		grid.cellWidth = screenWidth;
		Vector4 clipRange = draggablePanel.panel.clipRange;
		clipRange.z = screenWidth;
		draggablePanel.panel.clipRange = clipRange;
	}
	
	void OnDisable()
	{
		IFActivityIndicator.DismissAll();
	}
	
	void OnEnable()
	{
		needsDataUpdate = true;
	}
	
	int PageCount
	{
		get
		{
			if(answers != null) {
				return  Mathf.CeilToInt((float)answers.Length / (float)IFReviewPage.maxAnswerCount);
			}
			return 1;
		}
	}
	
	void Update()
	{
		if(needsDataUpdate) {
			UpdateData();
		}
		if(needsTableUpdate) {
			UpdateTable();
		}
		UpdatePageIndicators();		
	}
	
	void UpdatePageIndicators()
	{
		int pageCount = PageCount;
		float xPos = dragPanelTransform.localPosition.x;
		int pageIndex = Mathf.FloorToInt(Mathf.Abs(xPos) / grid.cellWidth);
		
		pageControl.selectedPageIndex = pageIndex;
		pageControl.pageCount = pageCount;
		
		if(lastPageIndex != pageIndex || lastPageCount != pageCount) {
			lastPageIndex = pageIndex;
			lastPageCount = pageCount;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(Localization.Localize("Page"));
			sb.Append((pageIndex + 1).ToString("D"));
			sb.Append("/");
			sb.Append(pageCount.ToString("D"));
			pageCountIndicator.Text = sb.ToString();
		}
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}
	
	void UpdateData()
	{	
		answers = null;
		
		if(challenge != null && challenge.Game != null) {
			game = challenge.Game;
		}
		
		if(game != null) {
			HashSet<IFAnswer> answerSet = game.Answers;
			if(answerSet == null || answerSet.Count == 0) {
				answerSet = IFAnswer.AnswersForGame(game);
			}
			answers = answerSet.Where((answer) => !answer.Correct).ToArray();
			needsTableUpdate = true;
		} else if(challenge != null) {
			StartCoroutine(FetchDetailedChallenge());
		}
		needsDataUpdate = false;
	}
	
	IFAnswer[] AnswersFromAnswerHashes(ArrayList answerHashes, bool onlyIncorrect, IFGame relatedGame)
	{
		if(answerHashes == null) {
			return null;
		}
		List<IFAnswer> answerList = new List<IFAnswer>();
		for(int i = 0; i < answerHashes.Count; i++) {
			Hashtable answerHash = answerHashes[i] as Hashtable;
			answerList.Add(new IFAnswer(Convert.ToBoolean(answerHash["correct"]), 0f, IFQuestion.QuestionWithRemoteIdentifier(Convert.ToInt32(answerHash["question_id"])), relatedGame));
		}
		if(onlyIncorrect) {
			return answerList.Where((answer) => !answer.Correct).ToArray();
		}
		return answerList.ToArray();
	}
	
	IEnumerator FetchDetailedChallenge()
	{
		UriBuilder uriBuilder = new UriBuilder(IFGameManager.SharedManager.remoteURLs.Challenges + "/" + challenge.RemoteId + "/results" );
		string authToken = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
		if(authToken != null) {
			uriBuilder.Query = "access_token="+PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);	
		}

		IFActivityIndicator indicator = IFActivityIndicator.CreateFloatingActivityIndicator();
		indicator.color = Color.black;

		WWW web = new WWW(uriBuilder.Uri.AbsoluteUri);
		yield return web;
		
		if(web.error == null) {
			Hashtable responseHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			Hashtable userHash, opponentHash;
			if(challenge.WasCreator) {
				userHash = responseHash["user"] as Hashtable;
				opponentHash = responseHash["opponent"] as Hashtable;
			} else {
				userHash = responseHash["opponent"] as Hashtable;
				opponentHash = responseHash["user"] as Hashtable;
			}
			ArrayList userAnswerHashes = userHash["questions"] as ArrayList;
			ArrayList opponentAnswerHashses = opponentHash["questions"] as ArrayList;
			
			IFGame userGame = IFGame.GameWithRemoteId(Convert.ToInt32(userHash["game_id"]));
			
			opponentAnswers = AnswersFromAnswerHashes(opponentAnswerHashses, false, null);
			answers = AnswersFromAnswerHashes(userAnswerHashes, (opponentAnswers == null || opponentAnswers.Length == 0),  userGame);
			
			
			needsDataUpdate = false;
			needsTableUpdate = true;
		} else {
			needsDataUpdate = true;
			IFAlertViewController.ShowAlert(Localization.Localize("Please try again."), Localization.Localize("Error fetching challenge details"));
		}
		indicator.Dismiss();
	}
	
	void AnswerMoreInfoWasSelected(IFAnswer answer)
	{
		IFHintController hintController = IFHintController.PresentControllerWithQuestionAndHintText(answer.Question.Text, answer.Question.Hint, null);
		hintController.Title = Localization.Localize("More Info");
	}
	
	void UpdateTable()
	{
		scrollingPanel.widgetsAreStatic = false;
		if(answers != null || opponentAnswers != null) {
			for(int i = 0; i < PageCount; i++) {
				GameObject pageGO = Instantiate(IFGameManager.LoadableAssets.ReviewPagePrefab) as GameObject;
				NGUITools.Destroy(pageGO.GetComponent<UIPanel>());
				
				Transform t = pageGO.transform;
				t.parent = gridTransform;
				t.localScale = Vector3.one;
				t.localRotation = Quaternion.identity;
				t.localPosition = Vector3.zero;
				
				IFReviewPage page = pageGO.GetComponent<IFReviewPage>();
				
				int pageAnswerCount = IFReviewPage.maxAnswerCount;
				int answerOffset = (i * pageAnswerCount);
				if((answerOffset + 2) >= answers.Length) {
					pageAnswerCount = answers.Length - answerOffset;
				}
				
				IFAnswer[] pageAnswers = new IFAnswer[pageAnswerCount];
				for(int j = 0; j < pageAnswerCount; j++) {
					pageAnswers[j] = answers[j + answerOffset];
				}
				page.Answers = pageAnswers;
				
				if(opponentAnswers != null && opponentAnswers.Length > 0) {
					if((answerOffset + 2) >= opponentAnswers.Length) {
						pageAnswerCount = opponentAnswers.Length - answerOffset;
					}
					
					if(pageAnswerCount > 0) {
						IFAnswer[] opponentPageAnswers = new IFAnswer[pageAnswerCount];
						for(int j = 0; j < pageAnswerCount; j++) {
							opponentPageAnswers[j] = opponentAnswers[j + answerOffset];
						}
						page.OpponentAnswers = opponentPageAnswers;
					}
				}
				
				if(challenge != null) {
					if(challenge.WasCreator) {
						page.PlayerUsername = challenge.Username;
						page.OpponentUsername = challenge.OpponentUsername;
					} else {
						page.PlayerUsername = challenge.OpponentUsername;
						page.OpponentUsername = challenge.Username;
					}
				}
				
				foreach(IFReviewCell cell in page.cells) {
					cell.AnswerMoreInfoSelectionDelegate += AnswerMoreInfoWasSelected;
				}
			}
		}
		grid.Reposition();
		draggablePanel.ResetPosition();
		scrollingPanel.widgetsAreStatic = true;
		
		// Recenter the scrolling panel
		Vector4 clip = draggablePanel.panel.clipRange;
		clip.x = 0f;
		draggablePanel.panel.clipRange = clip;
		draggablePanel.transform.localPosition = Vector3.zero;

		needsTableUpdate = false;
	}

	public void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
}
