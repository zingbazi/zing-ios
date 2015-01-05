// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;

public class IFHighScoresController : MonoBehaviour
{
	public struct HighScore
	{
		public string username;
		public int score;
		public int rank;
		
		public static readonly HighScore none = new HighScore(null, 0, 0);
		
		public HighScore(string theUsername, int theScore, int theRank)
		{
			username = theUsername;
			score = theScore;
			rank = theRank;
		}
		
		public static bool operator ==(HighScore lhs, HighScore rhs) { return lhs.Equals(rhs);	}
		public static bool operator !=(HighScore lhs, HighScore rhs) { return !lhs.Equals(rhs);	}
		
		public override bool Equals(object obj)
		{
			if(object.ReferenceEquals(obj, null)) {
				return false;
			}
		
			if(!obj.GetType().Equals(typeof(HighScore))) {
				return false;
			}
			
			HighScore other = (HighScore)obj;
			bool uname = false;
			if(username == null) {
				uname = other.username == null;
			} else {
				if(other.username == null) uname = false;
				else uname = username.Equals(other.username);
			}
			return uname && score == other.score && rank == other.rank;
		}
		
		public override int GetHashCode()
		{
			return username.GetHashCode() ^ score.GetHashCode() ^ rank.GetHashCode();
		}
	};
	
	public int syncTimeoutSeconds = 60;
	public IFHighScoreCell userHighScoreCell;
	public IFGameManager.ShouldTransitionToDefault shouldTransitionToDefaultDelegate;
	
	public Color cellGray = new Color(.86f, .86f, .86f);
	public Color cellWhite = new Color(.97f, .97f, .97f);

	private UIPanel mPanel;
	public IFHighScoreCell[] cells;
	public IFHighScoreCell[] conditionalCells;
	public IFHighScoreCell smallLastCell;
	public IFHighScoreCell bigLastCell;
	public IFHighScoreCell bottomCell;
	public Transform tableBackground;
	public int startIndex = 0;

	private HighScore currentUserScore;
	private List<HighScore> allTimeScores = new List<HighScore>();
	private List<HighScore> todayScores = new List<HighScore>();
	private List<HighScore> thisWeekScores = new List<HighScore>();
	private List<HighScore> selectedScores;
	
	enum SelectedList { AllTime, Today, ThisWeek };
	SelectedList selected;
	
	private bool needsTableUpdate = true;
	private bool needsDataUpdate = true;
	private bool isFetchingScores = false;
	private bool lastCacheIsDisplayed = false;

	public static string CacheFilePath
	{
		get
		{
			return Path.Combine(Application.temporaryCachePath, "highscores.json");
		}
	}
	
	public static IFHighScoresController CreateFromPrefab()
	{
		if(IFGameManager.LoadableAssets.HighScoresScreenPrefab == null) {
			return IFHighScoresController.Create();
		}
		GameObject go = Instantiate(IFGameManager.LoadableAssets.HighScoresScreenPrefab) as GameObject;
		UIPanel p = go.GetComponent<UIPanel>();
		if(p != null) {
			NGUITools.Destroy(p);	
		}

		return go.GetComponent<IFHighScoresController>();
	}

	public static IFHighScoresController Create(string name)
	{
		GameObject go = new GameObject(name); 
		return go.AddComponent<IFHighScoresController>();
	}

	public static IFHighScoresController Create()
	{
		return Create("High Scores Screen");
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
	
	void OnEnable()
	{
		needsDataUpdate = true;
	}
	
	void OnDisable()
	{
		IFActivityIndicator.DismissAll();
	}
	
	void ControllerWillDisappear()
	{
		IFUtils.SetEnabledAllCollidersInChildren(gameObject, false);
	}
	
	void Start()
	{
		mPanel = GetComponentInChildren<UIPanel>();
		selected = SelectedList.AllTime;

		UIAnchor anchor = bottomCell.GetComponent<UIAnchor>();
//		AspectRatio ratio = AspectRatios.GetAspectRatio();
//		if(ratio == AspectRatio.Aspect10by16 || ratio == AspectRatio.Aspect9by16 || ratio == AspectRatio.Aspect2by3) {
			foreach(IFHighScoreCell c in conditionalCells) {
				c.gameObject.SetActive(true);
			}
			anchor.widgetContainer = bigLastCell.background;
			tableBackground.localScale = new Vector3(600f, 616f, 1f);
//		} else {
//			foreach(IFHighScoreCell c in conditionalCells) {
//				c.gameObject.SetActive(false);
//			}
//			anchor.widgetContainer = smallLastCell.background;
//			tableBackground.localScale = new Vector3(600f, 440f, 1f);
//		}
	}
	
	private List<HighScore> HighScoreListFromHashList(ArrayList hashList)
	{
		List<HighScore> scoreList = new List<HighScore>();
		foreach(Hashtable score in hashList) {
			scoreList.Add(new HighScore((string)score["username"], Convert.ToInt32(score["highscore"]), Convert.ToInt32(score["rank"])));
		}
		return scoreList;
	}
	
	private void LoadDataFromJSON(string jsonText)
	{
		Hashtable dataHash = MiniJSON.jsonDecode(jsonText) as Hashtable;
		if(dataHash.ContainsKey("all")) {
			allTimeScores = HighScoreListFromHashList(dataHash["all"] as ArrayList);
		}
		
		if(dataHash.ContainsKey("today")) {
			todayScores = HighScoreListFromHashList(dataHash["today"] as ArrayList);
		}
		
		if(dataHash.ContainsKey("week")) {
			thisWeekScores = HighScoreListFromHashList(dataHash["week"] as ArrayList);
		}
		if(dataHash.ContainsKey("current_user")) {
			Hashtable score = dataHash["current_user"] as Hashtable;
			if(score == null) {
				currentUserScore = HighScore.none;
			} else {
				currentUserScore = new HighScore(score["username"] as string, Convert.ToInt32(score["highscore"]), Convert.ToInt32(score["rank"]));
			}
		} else {
			currentUserScore = HighScore.none;
		}
		needsTableUpdate = true;
	}
	
	IEnumerator FetchHighScores()
	{
		if(!isFetchingScores && needsDataUpdate) {
			isFetchingScores = true;
			Hashtable headers = new Hashtable();
			headers["Content-Type"] = "application/json";
			headers["Accept"] = "application/json";

			UriBuilder uriBuilder = new UriBuilder(IFGameManager.SharedManager.remoteURLs.HighScores);
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
				LoadDataFromJSON(web.text);
				
				PlayerPrefs.SetString(IFConstants.LastHighScoreSync, DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
				
				FileInfo cacheFile = new FileInfo(IFHighScoresController.CacheFilePath);
				using(FileStream fs = cacheFile.Open(FileMode.Create)) {
					byte[] jsonBytes = UTF8Encoding.UTF8.GetBytes(web.text);
					fs.Write(jsonBytes, 0, jsonBytes.Length);
				}
				lastCacheIsDisplayed = false;
			} else {
				IFAlertViewController.ShowAlert(Localization.Localize("Please try again."), Localization.Localize("Error fetching High Scores"));
			}
			isFetchingScores = false;
			needsDataUpdate = false;
		}
	}
	
	void Update()
	{
		if(needsDataUpdate) {
			string lastSyncString = PlayerPrefs.GetString(IFConstants.LastHighScoreSync, DateTime.UtcNow.Subtract(new TimeSpan(1, 0, 0)).ToString());
			DateTime lastSync = DateTime.Parse(lastSyncString);
			TimeSpan span = DateTime.UtcNow.Subtract(lastSync);
			bool syncTimedOut = span.TotalSeconds > syncTimeoutSeconds;
			if(!isFetchingScores && syncTimedOut) {
				StartCoroutine(FetchHighScores());		
			} else if(!lastCacheIsDisplayed) {
				FileInfo cacheFile = new FileInfo(IFHighScoresController.CacheFilePath);
				if(cacheFile.Exists) {
					using(StreamReader reader = cacheFile.OpenText()) {
						LoadDataFromJSON(reader.ReadToEnd());
					}
					lastCacheIsDisplayed = true;
				}
			}
		}
		if(needsTableUpdate) {
			needsTableUpdate = false;
			if(selected == SelectedList.ThisWeek) {
				selectedScores = thisWeekScores;
			} else if(selected == SelectedList.Today) {
				selectedScores = todayScores;
			} else {
				selectedScores = allTimeScores;
			}
			
			if(currentUserScore != HighScore.none) {
				userHighScoreCell.Score = currentUserScore.score;
				userHighScoreCell.Rank = currentUserScore.rank;
				userHighScoreCell.Username = currentUserScore.username;
			} else {
				userHighScoreCell.Score = int.MaxValue;
				userHighScoreCell.Rank = int.MaxValue;
				userHighScoreCell.Username = Localization.Localize("Not Logged In");
			}

			IFHighScoreCell[] activeCells = cells.Where((cell) => { return cell.gameObject.activeSelf; }).ToArray();
			
			for(int i = 0; i < activeCells.Length; i++) {
				if(i + startIndex < selectedScores.Count) {
					activeCells[i].Score = selectedScores[i + startIndex].score;
					activeCells[i].Rank = selectedScores[i + startIndex].rank;
					activeCells[i].Username = selectedScores[i + startIndex].username;
				} else {
					activeCells[i].Score = int.MaxValue;
					activeCells[i].Rank = int.MaxValue;
					activeCells[i].Username = null;
				}
			}
		}
	}
	
	void LoadMoreButtonTapped()
	{
		int max = cells.Where((cell) => { return cell.gameObject.activeSelf; }).Count();
		int maxIndex = Mathf.Max(selectedScores.Count - 1, 0);
		if(startIndex + max > maxIndex) {
			startIndex = 0;
		} else {
			startIndex += max;
		}
		
		startIndex = Mathf.Clamp(startIndex, 0, maxIndex);
		needsTableUpdate = true;
	}
	
	public void AllTimeButtonTapped()
	{
		selected = SelectedList.AllTime;
		startIndex = 0;
		needsTableUpdate = true;
	}
	

	public void ThisWeekButtonTapped()
	{
		selected = SelectedList.ThisWeek;
		startIndex = 0;
		needsTableUpdate = true;
	}

	public void TodayButtonTapped()
	{
		selected = SelectedList.Today;
		startIndex = 0;
		needsTableUpdate = true;
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

	public void BackButtonWasTapped(GameObject sender)
	{
		sender.GetComponent<UIButtonMessage>().enabled = false;
		if(shouldTransitionToDefaultDelegate == null || shouldTransitionToDefaultDelegate()) {
			IFGameManager.SharedManager.TransitionToHomeScreen();
		}
	}
}
