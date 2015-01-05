// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class IFAnswer : object
{
	
	#region Properties
	
	public int Identifier { get; private set; }
	
	private bool mCorrect;
	public bool Correct
	{
		get
		{
			return mCorrect;
		}
		set
		{
			if(mCorrect != value) {
				mCorrect = value;
				isDirty = true;
			}
		}
	}
	
	private float mDuration;
	public float Duration
	{
		get
		{
			return mDuration;
		}
		set
		{
			if(mDuration != value) {
				mDuration = value;
				isDirty = true;
			}
		}
	}
	
	private IFQuestion mQuestion;
	public IFQuestion Question
	{
		get
		{
			return mQuestion;
		}
		set
		{
			if(mQuestion == null || !mQuestion.Equals(value)) {
				mQuestion = value;
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
			if(mGame == null && value == null) return;
			
			if(mGame == null || !mGame.Equals(value)) {
				mGame = value;
				isDirty = true;
			}
		}
	}
	
	private bool isDirty = false;
	
	#endregion
	
	public IFAnswer(int id, bool correct, float duration, IFQuestion question, IFGame game)
	{
		Identifier = id;
		Correct = correct;
		Duration = duration;
		Question = question;
		Game = game;
		isDirty = id == -1;
	}
	
	public IFAnswer(bool correct, float duration, IFQuestion question, IFGame game) : this(-1, correct, duration, question, game) {}
	
	public void Save()
	{
		if(isDirty) {
			if(Identifier < 0) {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@correct", Correct},
					{"@duration", Duration},
					{"@question_id", Question.Identifier},
					{"@game_id", Game.Identifier}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("INSERT INTO answers (correct, duration, question_id, game_id) VALUES (@correct, @duration, @question_id, @game_id)", parameters);
				object rowId = IFDatabase.SharedDatabase.ExecuteScalar("SELECT last_insert_rowid()");
				Identifier = Convert.ToInt32(rowId);
			} else {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@id", Identifier},
					{"@correct", Correct},
					{"@duration", Duration},
					{"@question_id", Question.Identifier},
					{"@game_id", Game.Identifier}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("UPDATE answers SET correct=@correct, duration=@duration, question_id=@question_id, game_id=@game_id where id=@id", parameters);
			}
			isDirty = false;
		}
	}
	
	public void Delete()
	{
		if(Identifier >= 0) {
			Dictionary<string, object> parameters = new Dictionary<string, object>() {
				{"@id", Identifier}
			};

			IFDatabase.SharedDatabase.ExecuteQuery("DELETE FROM answers WHERE id=@id", parameters);
			Identifier = -1;
			isDirty = true;
		}
	}
	
	public static HashSet<IFAnswer> AnswersForGame(IFGame game)
	{
		HashSet<IFAnswer> answers = new HashSet<IFAnswer>();
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@game_id", game.Identifier} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, correct, duration, question_id FROM answers WHERE game_id=@game_id", parameters)) {
			if(reader.HasRows) {
				while(reader.Read()) {
					IFQuestion question = IFQuestion.QuestionWithIdentifier(reader.GetInt32(3));
					IFAnswer answer = new IFAnswer(reader.GetInt32(0), reader.GetBoolean(1), reader.GetFloat(2), question, game);
					answers.Add(answer);
				}
			}
		}
		return answers;
	}
	
	public override bool Equals(object obj)
	{
		if(obj == null) {
			return false;
		}
		
		IFAnswer other = obj as IFAnswer;
		if(other == null) {
			return false;
		}
		
		return this.Equals(other);
	}
	
	public bool Equals(IFAnswer other)
	{
		return Identifier == other.Identifier &&
				Duration == other.Duration &&
				Correct == other.Correct &&
				Question.Equals(other.Question) &&
				Game.Equals(other.Game);
	}
	
	public override int GetHashCode()
	{
		return Identifier.GetHashCode() ^
				Duration.GetHashCode() ^
				Correct.GetHashCode() ^
				Question.GetHashCode() ^
				Game.GetHashCode();
	}

}
