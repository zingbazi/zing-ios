// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using Mono.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

public class IFChallenge : object
{
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
	
	private int mRemoteGameId;
	public int RemoteGameId
	{
		get
		{
			return mRemoteGameId;
		}
		set
		{
			if(mRemoteGameId != value) {
				mRemoteGameId = value;
				isDirty = true;
			}
		}
	}
	
	private IFGame mGame;
	public IFGame Game
	{
		get
		{
			return mGame;
		}
		set
		{
			if(mGame == null || !mGame.Equals(value)) {
				mGame = value;
				isDirty = true;
			}
		}
	}
	
	public enum ChallengeState { Open = 1, Complete = 2};
	private ChallengeState mState;
	public ChallengeState State
	{
		get
		{
			return mState;
		}
		set
		{
			if(mState != value) {
				mState = value;
				isDirty = true;
			}
		}
	}
	
	private int mQuestionCount;
	public int QuestionCount
	{
		get
		{
			return mQuestionCount;
		}
		set
		{
			if(mQuestionCount != value) {
				mQuestionCount = value;
				isDirty = true;
			}
		}
	}

	
	private bool mWasCreator;
	public bool WasCreator
	{
		get
		{
			return mWasCreator;
		}
		set
		{
			if(mWasCreator != value) {
				mWasCreator = value;
				isDirty = true;
			}
		}
	}
	
	private bool mDidWin;
	public bool DidWin
	{
		get
		{
			return mDidWin;
		}
		set
		{
			if(mDidWin != value) {
				mDidWin = value;
				isDirty = true;
			}
		}
	}

	private int mUserId;
	public int UserId
	{
		get
		{
			return mUserId;
		}
		set
		{
			if(mUserId != value) {
				mUserId = value;
				isDirty = true;
			}
		}
	}
	
	private string mUsername;
	public string Username
	{
		get
		{
			return mUsername;
		}
		set
		{
			if(mUsername == null || !mUsername.Equals(value)) {
				mUsername = value;
				isDirty = true;
			}
		}
	}
	
	private int mUserScore;
	public int UserScore
	{
		get
		{
			return mUserScore;
		}
		set
		{
			if(mUserScore != value) {
				mUserScore = value;
				isDirty = true;
			}
		}
	}
	
	private ChallengeState mUserState;
	public ChallengeState UserState
	{
		get
		{
			return mUserState;
		}
		set
		{
			if(mUserState != value) {
				mUserState = value;
				isDirty = true;
			}
		}
	}

	private int mUserAnswerCount;
	public int UserAnswerCount
	{
		get
		{
			return mUserAnswerCount;
		}
		set
		{
			if(mUserAnswerCount != value) {
				mUserAnswerCount = value;
				isDirty = true;
			}
		}
	}
	
	private int mOpponentUserId;
	public int OpponentUserId
	{
		get
		{
			return mOpponentUserId;
		}
		set
		{
			if(mOpponentUserId != value) {
				mOpponentUserId = value;
				isDirty = true;
			}
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
			if(mOpponentUsername == null || !mOpponentUsername.Equals(value)) {
				mOpponentUsername = value;
				isDirty = true;
			}
		}
	}
	
	private int mOpponentAnswerCount;
	public int OpponentAnswerCount
	{
		get
		{
			return mOpponentAnswerCount;
		}
		set
		{
			if(mOpponentAnswerCount != value) {
				mOpponentAnswerCount = value;
				isDirty = true;
			}
		}
	}
	
	private int mOpponentScore;
	public int OpponentScore
	{
		get
		{
			return mOpponentScore;
		}
		set
		{
			if(mOpponentScore != value) {
				mOpponentScore = value;
				isDirty = true;
			}
		}
	}
	
	private ChallengeState mOpponentState;
	public ChallengeState OpponentState
	{
		get
		{
			return mOpponentState;
		}
		set
		{
			if(mOpponentState != value) {
				mOpponentState = value;
				isDirty = true;
			}
		}
	}
	
	private int mOpponentRemoteGameId;
	public int OpponentRemoteGameId
	{
		get
		{
			return mOpponentRemoteGameId;
		}
		set
		{
			if(mOpponentRemoteGameId != value) {
				mOpponentRemoteGameId = value;
				isDirty = true;
			}
		}
	}
	
	private int[] mRemoteQuestionIds;
	public int[] RemoteQuestionIds
	{
		get
		{
			return mRemoteQuestionIds;
		}
		set
		{
			mRemoteQuestionIds = value;
			isDirty = true;
		}
	}
	
	private bool isDirty = false;
	
	#endregion
	
	
	public IFChallenge(int id, int remoteId, int remoteGameId, ChallengeState state, int questionCount, bool wasCreator, bool didWin, int userId, string username, int userScore, ChallengeState userCompleted, int userAnswerCount, int opponentUserId, string opponentUsername, int opponentScore, ChallengeState opponentCompleted, int opponentAnswerCount, int opponentRemoteGameId, int[] questionIds)
	{
		Identifier = id;
		RemoteId = remoteId;
		RemoteGameId = remoteGameId;
		State = state;
		QuestionCount = questionCount;
		WasCreator = wasCreator;
		DidWin = didWin;
		UserId = userId;
		Username = username;
		UserScore = userScore;
		UserState = userCompleted;
		UserAnswerCount = userAnswerCount;
		OpponentUserId = opponentUserId;
		OpponentUsername = opponentUsername;
		OpponentScore = opponentScore;
		OpponentState = opponentCompleted;
		OpponentAnswerCount = opponentAnswerCount;
		OpponentRemoteGameId = opponentRemoteGameId;
		RemoteQuestionIds = questionIds;
		isDirty = id == -1;
	}

	public IFChallenge(int remoteGameId, int userId, string username, int opponentUserId, string opponentUsername) : this(-1, -1, remoteGameId, ChallengeState.Open, 0, true, false, userId, username, 0, ChallengeState.Open, 0, opponentUserId, opponentUsername, 0, ChallengeState.Open, 0, -1, null) {}
	
	public IFChallenge() : this(-1, -1, -1, ChallengeState.Open, 0, true, false, -1, null, 0, ChallengeState.Open, 0, -1, null, 0, ChallengeState.Open, 0, -1, null) {}
	
	public void Save()
	{
		if(isDirty) {
			IFDatabase.SharedDatabase.BeginTransaction();
			if(Identifier < 0) {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@remote_id", RemoteId},
					{"@remote_game_id", RemoteGameId},
					{"@state", Convert.ToInt32(State)},
					{"@question_count", QuestionCount},
					{"@creator", WasCreator},
					{"@won", DidWin},
					{"@user_remote_id", UserId},
					{"@username", Username},
					{"@user_score", UserScore},
					{"@user_state", UserState},
					{"@user_answer_count", UserAnswerCount},
					{"@opponent_user_id",OpponentUserId},
					{"@opponent_username", OpponentUsername},
					{"@opponent_score", OpponentScore},
					{"@opponent_state", OpponentState},
					{"@opponent_answer_count", OpponentAnswerCount},
					{"@opponent_remote_game_id", OpponentRemoteGameId}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("INSERT INTO challenges (remote_id, remote_game_id, state, question_count, creator, won, user_remote_id, username, user_score, user_state, user_answer_count, opponent_user_id, opponent_username, opponent_score, opponent_state, opponent_answer_count, opponent_remote_game_id) VALUES (@remote_id, @remote_game_id, @state, @question_count, @creator, @won, @user_remote_id, @username, @user_score, @user_state, @user_answer_count, @opponent_user_id, @opponent_username, @opponent_score, @opponent_state, @opponent_answer_count, @opponent_remote_game_id)", parameters);
				object rowId = IFDatabase.SharedDatabase.ExecuteScalar("SELECT last_insert_rowid()");
				Identifier = Convert.ToInt32(rowId);
			} else {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@id", Identifier},
					{"@remote_id", RemoteId},
					{"@remote_game_id", RemoteGameId},
					{"@state", Convert.ToInt32(State)},
					{"@question_count", QuestionCount},
					{"@creator", WasCreator},
					{"@won", DidWin},
					{"@user_remote_id", UserId},
					{"@username", Username},
					{"@user_score", UserScore},
					{"@user_state", UserState},
					{"@user_answer_count", UserAnswerCount},
					{"@opponent_user_id",OpponentUserId},
					{"@opponent_username", OpponentUsername},
					{"@opponent_score", OpponentScore},
					{"@opponent_state", OpponentState},
					{"@opponent_answer_count", OpponentAnswerCount},
					{"@opponent_remote_game_id", OpponentRemoteGameId}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("UPDATE challenges SET remote_id=@remote_id, remote_game_id=@remote_game_id, state=@state, question_count=@question_count, creator=@creator, won=@won, user_remote_id=@user_remote_id, username=@username, user_score=@user_score, user_state=@user_state, user_answer_count=@user_answer_count, opponent_user_id=@opponent_user_id, opponent_username=@opponent_username, opponent_score=@opponent_score, opponent_state=@opponent_state, opponent_answer_count=@opponent_answer_count, opponent_remote_game_id=@opponent_remote_game_id WHERE id=@id", parameters);
			}
			Dictionary<string, object> challengeParams = new Dictionary<string, object>() { {"@challenge_id", Identifier} };
			IFDatabase.SharedDatabase.ExecuteQuery("DELETE FROM challenge_questions WHERE challenge_id=@challenge_id", challengeParams);
			if(RemoteQuestionIds != null) {
				for(int i = 0; i < RemoteQuestionIds.Length; i++) {
					Dictionary<string, object> questionParams = new Dictionary<string, object>() {
						{"@challenge_id", Identifier},
						{"@remote_question_id", RemoteQuestionIds[i]},
						{"@question_order", i}
					};
					IFDatabase.SharedDatabase.ExecuteQuery("INSERT INTO challenge_questions (challenge_id, remote_question_id, question_order) VALUES (@challenge_id, @remote_question_id, @question_order)", questionParams);
				}
			}
			IFDatabase.SharedDatabase.CommitTransaction();
			isDirty = false;
		}
	}
	
	public static int CountOfOpenChallenges()
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>() {
			{"@state", ChallengeState.Open}
		};
		object count = IFDatabase.SharedDatabase.ExecuteScalar("SELECT COUNT (*) FROM challenges WHERE state=@state", parameters);
		return Convert.ToInt32(count);
	}
	
	public static int CountOfPlayableChallenges()
	{
		List<IFChallenge> openChallenges = IFChallenge.GetOpenChallenges();
		IEnumerable<IFChallenge> playable = openChallenges.Where( c => {
			if(c.WasCreator) {
				return c.UserState == ChallengeState.Open;
			} else {
				return c.OpponentState == ChallengeState.Open;
			}
		});
		return playable.Count();
	}
	
	private static IFChallenge ChallengeFromCurrentDataReaderRow(SqliteDataReader reader)
	{
		int questionCount = reader.GetInt32(4);
		int identifier = reader.GetInt32(0);
		int[] remoteQuestionIds = new int[questionCount];
		int index = 0;
		
		Dictionary<string, object> dbParams = new Dictionary<string, object>() { {"@challenge_id", identifier} };
		using(SqliteDataReader dbReader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT remote_question_id FROM challenge_questions WHERE challenge_id=@challenge_id ORDER BY question_order ASC", dbParams)) {
			if(dbReader.HasRows) {
				while(dbReader.Read()) {
					remoteQuestionIds[index++] = dbReader.GetInt32(0);
				}
			}
		}
		
		int remoteId = reader.GetInt32(1);
		int remoteGameId = reader.GetInt32(2);
		ChallengeState state = (ChallengeState)reader.GetInt32(3);
		bool wasCreator = reader.GetBoolean(5);
		bool didWin = reader.GetBoolean(6);
		int userRemoteId = reader.GetInt32(7);
		string username = reader.GetValue(8).ToString();
		int userScore = reader.GetInt32(9);
		ChallengeState userState = (ChallengeState)reader.GetInt32(10);
		int userAnswerCount = reader.GetInt32(11);
		int opponentUserId = reader.GetInt32(12);
		string opponentUsername = reader.GetValue(13).ToString();
		int opponentScore = reader.GetInt32(14);
		ChallengeState opponentState = (ChallengeState)reader.GetInt32(15);
		int opponentAnswerCount = reader.GetInt32(16);
		int opponentGameId = reader.GetInt32(17);
		
		IFChallenge challenge = new IFChallenge(identifier, remoteId, remoteGameId, state, questionCount, wasCreator, didWin, userRemoteId, username, userScore, userState, userAnswerCount, opponentUserId, opponentUsername, opponentScore, opponentState, opponentAnswerCount, opponentGameId, remoteQuestionIds);
		return challenge;
	}
	
	public static List<IFChallenge> GetOpenChallenges()
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@state", ChallengeState.Open} };
		
		List<IFChallenge> challenges = new List<IFChallenge>();
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, remote_id, remote_game_id, state, question_count, creator, won, user_remote_id, username, user_score, user_state, user_answer_count, opponent_user_id, opponent_username, opponent_score, opponent_state, opponent_answer_count, opponent_remote_game_id FROM challenges WHERE state=@state", parameters)) {
			if(reader.HasRows) {
				while(reader.Read()) {
					challenges.Add(ChallengeFromCurrentDataReaderRow(reader));
				}
			}
		}
		return challenges;
	}
	
	public static List<IFChallenge> GetAllChallenges()
	{
		if(!PlayerPrefs.HasKey(IFConstants.RemoteUserIdKey)) {
			return null;
		}
		
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@user_remote_id", PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey)} };
		
		List<IFChallenge> challenges = new List<IFChallenge>();
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, remote_id, remote_game_id, state, question_count, creator, won, user_remote_id, username, user_score, user_state, user_answer_count, opponent_user_id, opponent_username, opponent_score, opponent_state, opponent_answer_count, opponent_remote_game_id FROM challenges WHERE user_remote_id=@user_remote_id OR opponent_user_id=@user_remote_id ORDER BY remote_id DESC", parameters)) {
			if(reader.HasRows) {
				while(reader.Read()) {
					challenges.Add(ChallengeFromCurrentDataReaderRow(reader));
				}
			}
		}
		return challenges;
	}
	
	public static IFChallenge ChallengeWithRemoteId(int remoteId)
	{
		IFChallenge challenge = null;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@remote_id", remoteId} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, remote_id, remote_game_id, state, question_count, creator, won, user_remote_id, username, user_score, user_state, user_answer_count, opponent_user_id, opponent_username, opponent_score, opponent_state, opponent_answer_count, opponent_remote_game_id FROM challenges WHERE remote_id=@remote_id", parameters)) {
			if(reader.HasRows) {
				while(reader.Read()) {
					challenge = ChallengeFromCurrentDataReaderRow(reader);
				}
			}
		}
		return challenge;
	}
	
	public static void MergeRemoteChallengeItem(Hashtable challengeHash)
	{
		int challengeId = Convert.ToInt32(challengeHash["id"]);
		IFChallenge challenge = IFChallenge.ChallengeWithRemoteId(challengeId);
		if(challenge == null) {
			challenge = new IFChallenge();
			challenge.RemoteId = challengeId;
			challenge.Save();
		}
		
		string stateString = challengeHash["state"] as string;
		if(stateString.Equals("open")) {
			challenge.State = IFChallenge.ChallengeState.Open;
		} else {
			challenge.State = IFChallenge.ChallengeState.Complete;
		}
		challenge.QuestionCount = Convert.ToInt32(challengeHash["questions_count"]);
		
		Hashtable userHash = (Hashtable)challengeHash["user"];
		Hashtable opponentHash = (Hashtable)challengeHash["opponent"];
		
		challenge.UserId = Convert.ToInt32(userHash["id"]);
		challenge.Username = (string)userHash["username"];
		challenge.UserScore = Convert.ToInt32(userHash["score"]);;
		challenge.UserAnswerCount = Convert.ToInt32(userHash["answers_count"]);
		challenge.RemoteGameId = Convert.ToInt32(userHash["game_id"]);
		string userStateString = userHash["state"] as string;
		if(userStateString.Equals("open")) {
			challenge.UserState = IFChallenge.ChallengeState.Open;
		} else {
			challenge.UserState = IFChallenge.ChallengeState.Complete;
		}
		
		challenge.OpponentUserId = Convert.ToInt32(opponentHash["id"]);
		challenge.OpponentUsername = (string)opponentHash["username"];
		challenge.OpponentScore = Convert.ToInt32(opponentHash["score"]);;
		challenge.OpponentAnswerCount = Convert.ToInt32(opponentHash["answers_count"]);
		challenge.OpponentRemoteGameId = Convert.ToInt32(opponentHash["game_id"]);
		string opponentStateString = opponentHash["state"] as string;
		if(opponentStateString.Equals("open")) {
			challenge.OpponentState = IFChallenge.ChallengeState.Open;
		} else {
			challenge.OpponentState = IFChallenge.ChallengeState.Complete;
		}
		
		challenge.WasCreator = (challenge.UserId == PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey));
		if(challenge.State == IFChallenge.ChallengeState.Complete) {
			int winnerId = Convert.ToInt32(challengeHash["winner_user_id"]);
			challenge.DidWin = (winnerId == PlayerPrefs.GetInt(IFConstants.RemoteUserIdKey));
		}
		challenge.Save();
		
		ArrayList questionIdObjects = (ArrayList)challengeHash["question_ids"];
		
		int[] remoteQuestionIds = new int[questionIdObjects.Count];
		for(int i = 0; i < remoteQuestionIds.Length; i++) {
			remoteQuestionIds[i] = Convert.ToInt32(questionIdObjects[i]);
		}
		
		challenge.RemoteQuestionIds = remoteQuestionIds;
		challenge.Save();
	}
	
	public static bool MergeRemoteChallengeList(ArrayList responseList)
	{
		if(responseList == null || responseList.Count == 0) return false;
		
		IFDatabase.SharedDatabase.BeginTransaction();
		foreach(Hashtable challengeHash in responseList) {
			MergeRemoteChallengeItem(challengeHash);
		}
		IFDatabase.SharedDatabase.CommitTransaction();
		return true;
	}
	
	public static bool ChallengeWithRemoteIdExists(int remoteId)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@remote_id", remoteId} };
		object countObject = IFDatabase.SharedDatabase.ExecuteScalar("SELECT COUNT (*) FROM challenges WHERE remote_id=@remote_id", parameters);
		int count = Convert.ToInt32(countObject);
		return count > 0;
	}
	
	public static void ClearAllChallenges()
	{
		IFDatabase.SharedDatabase.ExecuteQuery("DELETE FROM challenges");
	}
	
	public override bool Equals(object obj)
	{
		if(obj == null) {
			return false;
		}
		
		IFChallenge other = obj as IFChallenge;
		if(other == null) {
			return false;
		}
		
		return this.Equals(other);
	}
	
	public bool Equals(IFChallenge other)
	{
		return Identifier == other.Identifier && 
				RemoteId == other.RemoteId &&
				RemoteGameId == other.RemoteGameId &&
				State == other.State &&
				QuestionCount == other.QuestionCount &&
				WasCreator == other.WasCreator && 
				DidWin == other.DidWin && 
				UserId == other.UserId &&
				Username.Equals(other.Username) &&
				UserScore == other.UserScore &&
				UserState == other.UserState &&
				UserAnswerCount == other.UserAnswerCount &&
				OpponentUserId == other.OpponentUserId && 
				OpponentUsername.Equals(other.OpponentUsername) && 
				OpponentScore == other.OpponentScore &&
				OpponentState == other.OpponentState &&
				OpponentAnswerCount == other.OpponentAnswerCount &&
				OpponentRemoteGameId == other.OpponentRemoteGameId;
	}
	
	public override int GetHashCode()
	{
		return Identifier.GetHashCode() ^ 
				RemoteId.GetHashCode() ^ 
				RemoteGameId.GetHashCode() ^
				State.GetHashCode() ^
				QuestionCount.GetHashCode() ^
				WasCreator.GetHashCode() ^
				DidWin.GetHashCode() ^
				UserId.GetHashCode() ^
				Username.GetHashCode() ^
				UserScore.GetHashCode() ^
				UserState.GetHashCode() ^
				UserAnswerCount.GetHashCode() ^
				OpponentUserId.GetHashCode() ^
				OpponentUsername.GetHashCode() ^
				OpponentScore.GetHashCode() ^
				OpponentState.GetHashCode() ^
				OpponentAnswerCount.GetHashCode() ^
				OpponentRemoteGameId.GetHashCode();
	}
}
