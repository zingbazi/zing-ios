// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

public class IFGame : object
{
	public enum GameMode { Normal = 1, Challenge = 2, PassAndPlay = 3 };
	public int Identifier { get; private set; }
	
	#region Properties
	
	private int mRemoteId;
	public int RemoteId
	{
		get
		{
			return mRemoteId;
		}
		set
		{
			if(mRemoteId != value) {
				mRemoteId = value;
				isDirty = true;
			}
		}
	}

	
	private IFGameLevel mLevel;
	public IFGameLevel Level
	{ 
		get
		{
			return mLevel;
		}
		set
		{
			if(!ReferenceEquals(mLevel, value)) {
				mLevel = value;
				isDirty = true;
			}
		}
	}
	
	private GameMode mMode;
	public GameMode Mode
	{
		get
		{
			return mMode;
		}
		set
		{
			if(mMode != value) {
				mMode = value;
				isDirty = true;
			}
		}
	}
	
	private int mScore;
	public int Score
	{
		get
		{
			return mScore;
		}
		set
		{
			if(mScore != value) {
				mScore = value;
				isDirty = true;
			}
		}
	}
	
	private DateTime mDate;
	public DateTime Date
	{ 
		get
		{
			return mDate;
		}
		set
		{
			if(!mDate.Equals(value)) {
				mDate = value;
				isDirty = true;
			}
		}
	}
	
	private bool mUploaded;
	public bool Uploaded
	{
		get
		{
			if(isDirty) {
				mUploaded = false;
			}
			return mUploaded;
		}
		set
		{
			if(mUploaded != value) {
				mUploaded = value;
				isDirty = true;
			}
		}
	}
	
	private bool mCompleted;
	public bool Completed
	{
		get
		{
			return mCompleted;
		}
		set
		{
			if(mCompleted != value) {
				mCompleted = value;
				isDirty = true;
			}
		}
	}
	
	private int mPauseCount;
	public int PauseCount
	{
		get
		{
			return mPauseCount;
		}
		set
		{
			if(mPauseCount != value) {
				mPauseCount = value;
				isDirty = true;
			}
		}
	}

	private HashSet<IFAnswer> mAnswers;
	public HashSet<IFAnswer> Answers
	{
		get
		{
			return mAnswers;		
		}
		private set
		{
			if(mAnswers != null && mAnswers.Count > 0) {
				IFDatabase.SharedDatabase.BeginTransaction();
				foreach(IFAnswer answer in mAnswers) {
					answer.Delete();
				}
				IFDatabase.SharedDatabase.CommitTransaction();
			}
			mAnswers = value;
			isDirty = true;
		}
	}
	
	public void AddAnswer(IFAnswer answer)
	{
		if(Answers.Add(answer)) {
			answer.Game = this;
			isDirty = true;
		}
	}
	
	public void RemoveAnswer(IFAnswer answer)
	{
		if(Answers.Remove(answer)) {
			answer.Game = null;
			isDirty = true;
		}
	}
	
	public IFChallenge challenge;
	public bool forfeit = false;
	
	private bool isDirty = false;
	
	#endregion
	
	public IFGame(int id, IFGameLevel level, int score, GameMode mode, DateTime date, bool uploaded, int remoteId, bool completed, int pauseCount)
	{
		Identifier = id;
		Level = level;
		Mode = mode;
		Score = score;
		Date = date;
		Uploaded = uploaded;
		RemoteId = remoteId;
		Completed = completed;
		PauseCount = pauseCount;
		Answers = new HashSet<IFAnswer>();
		isDirty = id == -1;
	}
	
	public IFGame(IFGameLevel level, GameMode mode) : this(-1, level, 0, mode, DateTime.UtcNow, false, -1, false, 2) {}
	
	public void Save()
	{
		if(isDirty) {
			IFDatabase.SharedDatabase.BeginTransaction();
			if(Identifier < 0) {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@score", Score},
					{"@date", Date},
					{"@level", Level.Identifier},
					{"@mode", (int)Mode},
					{"@uploaded", Uploaded},
					{"@remote_id", RemoteId},
					{"@completed", Completed},
					{"@pause_count", PauseCount}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("INSERT INTO games (score, date, level, mode, uploaded, remote_id, completed, pause_count) VALUES (@score, @date, @level, @mode, @uploaded, @remote_id, @completed, @pause_count)", parameters);
				object rowId = IFDatabase.SharedDatabase.ExecuteScalar("SELECT last_insert_rowid()");
				Identifier = Convert.ToInt32(rowId);
			} else {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@id", Identifier},
					{"@score", Score},
					{"@date", Date},
					{"@level", Level.Identifier},
					{"@mode", (int)Mode},
					{"@uploaded", Uploaded},
					{"@remote_id",RemoteId},
					{"@completed", Completed},
					{"@pause_count", PauseCount}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("UPDATE games SET score=@score, date=@date, level=@level, mode=@mode, uploaded=@uploaded, remote_id=@remote_id, completed=@completed, pause_count=@pause_count where id=@id", parameters);
			}
			
			foreach(IFAnswer answer in Answers) {
				answer.Save();
			}
			IFDatabase.SharedDatabase.CommitTransaction();
			isDirty = false;
		}
	}
	
	public IFAnswer AnswerQuestion(IFQuestion question, bool correct, float duration)
	{
		IFAnswer answer = new IFAnswer(correct, duration, question, this);
		AddAnswer(answer);
		return answer;
	}
	
	private static IFGame GameFromCurrentDataReaderRow(SqliteDataReader reader)
	{
		IFGameLevel level = IFGameLevel.LevelWithIdentifier(reader.GetInt32(1));
		IFGame game = new IFGame(reader.GetInt32(0), level, reader.GetInt32(2), (GameMode)reader.GetInt32(3), reader.GetDateTime(4), reader.GetBoolean(5), reader.GetInt32(6), reader.GetBoolean(7), reader.GetInt32(8));
		return game;
	}
	
	public static IFGame[] GetLocalTopGames(int count)
	{ 
		List<IFGame> gameList = new List<IFGame>();
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@count", count} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, level, score, mode, date, uploaded, remote_id, completed, pause_count FROM games ORDER BY score DESC LIMIT @count", parameters)) {
			if(reader.HasRows) {
				while(reader.Read()) {
					gameList.Add(GameFromCurrentDataReaderRow(reader));
				}
			}
		}
		return gameList.ToArray();
	}
	
	public static IFGame GameWithIdentifier(int id)
	{
		IFGame game = null;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@id", id} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, level, score, mode, date, uploaded, remote_id, completed, pause_count FROM games WHERE id=@id", parameters)) {
			if(reader.HasRows) {
				while(reader.Read()) {
					game = GameFromCurrentDataReaderRow(reader);
				}
			}
		}
		return game;
	}
	
	public static IFGame GameWithRemoteId(int remoteId)
	{
		IFGame game = null;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@remote_id", remoteId} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, level, score, mode, date, uploaded, remote_id, completed, pause_count FROM games WHERE remote_id=@remote_id", parameters)) {
			if(reader.HasRows) {
				while(reader.Read()) {
					game = GameFromCurrentDataReaderRow(reader);
				}
			}
		}
		return game;
	}
	
	public static void ClearAllGames()
	{
		IFDatabase.SharedDatabase.ExecuteQuery("DELETE FROM games");
	}

	private Hashtable DataHashForAnswer(IFAnswer answer)
	{
		Hashtable data = new Hashtable();
		data["question_id"] = answer.Question.RemoteId;
		data["question_version"] = answer.Question.Version;
		data["correct"] = answer.Correct;
		data["duration"] = answer.Duration;
		data["hint_shown"] = false;
		return data;
	}
	
	public Hashtable DataHashForUpload()
	{
		Hashtable data = new Hashtable();
		data["access_token"] = PlayerPrefs.GetString(IFConstants.AccessTokenPrefsKey);
		data["date"] = DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture);
		data["score"] = Score;
		data["mode"] = Mode.ToString().ToLower();
		
		int userid = PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey);
		string hashString = userid.ToString("D") + ":3qtx-9ntf!E2YUgk:" + Score.ToString("D");
		MD5 md5 = MD5.Create();
		byte[] md5Data = md5.ComputeHash(Encoding.UTF8.GetBytes(hashString));
		StringBuilder builder = new StringBuilder();
		
		for(int i = 0; i < md5Data.Length; i++) {
			builder.Append(md5Data[i].ToString("x2"));
		}
		
		data["hash"] = builder.ToString();

		
		ArrayList answerList = new ArrayList();
		foreach(IFAnswer answer in Answers) {
			answerList.Add(DataHashForAnswer(answer));
		}
		data["answers"] = answerList;
		
		if(challenge != null) {
			data["challenge_id"] = challenge.RemoteId;
			data["forfeit"] = forfeit;
		}
		
		return data;
	}
	
	public IEnumerator UploadScore()
	{
		Hashtable headers = new Hashtable();
		headers["Content-Type"] = "application/json";
		headers["Accept"] = "application/json";
		
		string jsonString = MiniJSON.jsonEncode(DataHashForUpload());
		
		string url = IFGameManager.SharedManager.remoteURLs.Games;
		
		if(RemoteId > 0) {
			url += "/"+RemoteId;
		}
		
		WWW web = new WWW(url, Encoding.UTF8.GetBytes(jsonString), headers);
		yield return web;
		
		if(web.error == null) {
			Hashtable resposneHash = (Hashtable)MiniJSON.jsonDecode(web.text);
			if(resposneHash != null && resposneHash.ContainsKey("id")) {
				RemoteId = Convert.ToInt32(resposneHash["id"]);
			}
			Uploaded = true;
			Save();
		}
	}
	
	public override bool Equals(object obj)
	{
		if(obj == null) {
			return false;
		}
		
		IFGame other = obj as IFGame;
		if(other == null) {
			return false;
		}
		
		return this.Equals(other);
	}
	
	public bool Equals(IFGame other)
	{
		return Identifier == other.Identifier && Level.Equals(other.Level) && Score == other.Score && Date == other.Date && Completed == other.Completed && PauseCount == other.PauseCount;
	}
	
	public override int GetHashCode()
	{
		return Identifier.GetHashCode() ^ Level.GetHashCode() ^ Score.GetHashCode() ^ Date.GetHashCode() ^ Completed.GetHashCode() ^ PauseCount.GetHashCode();
	}
}
