// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class IFQuestion : object
{
	#region JSON Parsing
	public static void LoadQuestionsFromJSON(string json)
	{
		ArrayList decodedJson = MiniJSON.jsonDecode(json) as ArrayList;
		if(decodedJson != null) {
			foreach(Hashtable item in decodedJson) {
				ArrayList incorrectAnswers = item["incorrect"] as ArrayList;
				string[] incorrect = new string[incorrectAnswers.Count];
				for(int i = 0; i < incorrectAnswers.Count; i++) {
					incorrect[i] = (string)incorrectAnswers[i];
				}

				int level = Convert.ToInt32(item["level"]);
				float weight = Convert.ToSingle(item["weight"]);
				int remoteId = Convert.ToInt32(item["id"]);
				int categoryId = Convert.ToInt32(item["category_id"]);
				string text = item["text"] as string;
				string correct = item["correct"] as string;
				string hint = item["hint"] as string;

				IFQuestionCategory category = IFQuestionCategory.CategoryWithRemoteId(categoryId);
				IFQuestion question = IFQuestion.QuestionWithRemoteIdentifier(remoteId);
				if(question == null) {
					question = new IFQuestion(remoteId, level, weight, text, incorrect, correct, hint, category);
				} else {
					question.Level = level;
					question.Weight = weight;
					question.Text = text;
					question.IncorrectAnswers = incorrect;
					question.CorrectAnswer = correct;
					question.Hint = hint;
					question.Category = category;
				}
				question.Save();
			}
		}
	}
	#endregion
	
	#region Database
	
	public static IFQuestion QuestionFromDataReaderRow(SqliteDataReader reader)
	{
		string[] incorrect = new string[3] {reader.GetValue(3) as string, reader.GetValue(4) as string, reader.GetValue(5) as string};
		IFQuestionCategory category = IFQuestionCategory.CategoryWithRemoteId(reader.GetInt32(12));
		DateTime updatedAt = DateTime.Parse(reader.GetString(10));
		string questionText = reader.GetValue(2) as string;
		string correctAnswer = reader.GetValue(6) as string;
		string hint = reader.GetValue(9) as string;
		IFQuestion question = new IFQuestion(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(7), reader.GetFloat(8), questionText, incorrect, correctAnswer, hint, category, updatedAt, reader.GetInt32(11));
		return question;
	}
	
	public static IFQuestion QuestionWithIdentifier(int id)
	{
		IFQuestion question = null;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@id",id} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT q.id, q.remote_id, q.text, q.incorrect1, q.incorrect2, q.incorrect3, q.correct, q.level, q.weight, q.hint, q.updated_at, q.version, c.remote_id FROM questions AS q LEFT JOIN categories AS c ON q.category_id=c.remote_id WHERE q.id=@id", parameters)) {
			if(reader.HasRows) {
				reader.Read();
				question = QuestionFromDataReaderRow(reader);
			}
		}
		return question;
	}
	
	public static IFQuestion QuestionWithRemoteIdentifier(int remoteId)
	{
		IFQuestion question = null;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@remote_id", remoteId} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT q.id, q.remote_id, q.text, q.incorrect1, q.incorrect2, q.incorrect3, q.correct, q.level, q.weight, q.hint, q.updated_at, q.version, c.remote_id FROM questions AS q LEFT JOIN categories AS c ON q.category_id=c.remote_id WHERE q.remote_id=@remote_id", parameters)) {
			if(reader.HasRows) {
				reader.Read();
				question = QuestionFromDataReaderRow(reader);
			}
		}
		return question;
	}
	
	public static int RemoteIdForQuestionWithIdentifier(int id)
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@id",id} };
		object idObj = IFDatabase.SharedDatabase.ExecuteScalar("SELECT remote_id FROM questions WHERE id=@id", parameters);
		return Convert.ToInt32(idObj);
	}
	
	public static HashSet<IFQuestion> QuestionsWithIdentifiers(int[] ids)
	{
		HashSet<IFQuestion> questions = new HashSet<IFQuestion>();
		string[] idStrings = ids.Select(id => id.ToString()).ToArray();
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT q.id, q.text, q.incorrect1, q.incorrect2, q.incorrect3, q.correct, q.level, q.weight, q.hint, c.remote_id FROM questions AS q LEFT JOIN categories AS c ON q.category_id=c.remote_id WHERE q.id IN ("+string.Join(",", idStrings)+")")) {
			reader.Read();
			questions.Add(QuestionFromDataReaderRow(reader));
		}
		return questions;
	}
	
	#endregion
	
	#region Model properties and methods
	
	public IFQuestion(int id, int remoteID, int level, float weight, string text, string[] incorrectAnswers, string correctAnswer, string hint, IFQuestionCategory category, DateTime updatedAt, int version)
	{
		Identifier = id;
		RemoteId = remoteID;
		Level = level;
		Weight = weight;
		Text = text;
		IncorrectAnswers = incorrectAnswers;
		CorrectAnswer = correctAnswer;
		Hint = hint;
		Category = category;
		UpdatedAt = updatedAt;
		Version = version;
	}
	
	public IFQuestion(int remoteId, int level, float weight, string text, string[] incorrectAnswers, string correctAnswer, string hint, IFQuestionCategory category) : 
		this(-1, remoteId, level, weight, text, incorrectAnswers, correctAnswer, hint, category, DateTime.UtcNow, 0)
	{}

	public int Identifier { get; private set; }
	
	public int RemoteId { get; private set; }
	
	public int Level { get; private set; }
	
	public float Weight { get; private set; }
	
	public string Text { get; private set; }
	
	public string[] IncorrectAnswers { get; private set; }
	
	public string CorrectAnswer { get; private set; }
	
	public string Hint { get; private set; }
	
	public DateTime UpdatedAt { get; private set; }
	
	public int Version { get; private set; }
	
	private IFQuestionCategory mCategory;
	public IFQuestionCategory Category
	{
		get { return mCategory; }
		set
		{
			mCategory = value;
		}
	}

	private void Save()
	{
		if(Identifier < 0) {
			Dictionary<string, object> parameters = new Dictionary<string, object>() {
				{"@remote_id",RemoteId},
				{"@level",Level},
				{"@weight",Weight},
				{"@text",Text},
				{"@incorrect1",IncorrectAnswers[0]},
				{"@incorrect2",IncorrectAnswers[1]},
				{"@incorrect3",IncorrectAnswers[2]},
				{"@correct",CorrectAnswer},
				{"@hint",Hint},
				{"@updated_at",UpdatedAt},
				{"@category_id",Category.RemoteId},
				{"@version",Version}
			};
			IFDatabase.SharedDatabase.ExecuteQuery("INSERT INTO questions (text, incorrect1, incorrect2, incorrect3, correct, level, weight, hint, category_id, remote_id, updated_at, version) VALUES (@text, @incorrect1, @incorrect2, @incorrect3, @correct, @level, @weight, @hint, @category_id, @remote_id, @updated_at, @version)", parameters);
			object rowId = IFDatabase.SharedDatabase.ExecuteScalar("SELECT last_insert_rowid()");
			Identifier = Convert.ToInt32(rowId);
		} else {
			Dictionary<string, object> parameters = new Dictionary<string, object>() {
				{"@id",Identifier},
				{"@remote_id",RemoteId},
				{"@level",Level},
				{"@weight",Weight},
				{"@text",Text},
				{"@incorrect1",IncorrectAnswers[0]},
				{"@incorrect2",IncorrectAnswers[1]},
				{"@incorrect3",IncorrectAnswers[2]},
				{"@correct",CorrectAnswer},
				{"@hint",Hint},
				{"@updated_at",UpdatedAt},
				{"@category_id",Category.RemoteId},
				{"@version",Version}
			};
			IFDatabase.SharedDatabase.ExecuteQuery("UPDATE questions SET text=@text, incorrect1=@incorrect1, incorrect2=@incorrect2, incorrect3=@incorrect3, correct=@correct, level=@level, weight=@weight, hint=@hint, category_id=@category_id, remote_id=@remote_id, updated_at=@updated_at, version=@version WHERE id=@id", parameters);

		}
	}
	
	public override bool Equals(object obj)
	{
		if(obj == null) {
			return false;
		}
		
		IFQuestion other = obj as IFQuestion;
		if(other == null) {
			return false;
		}
		
		return this.Equals(other);
	}
	
	public bool Equals(IFQuestion other)
	{
		return Identifier == other.Identifier &&
				Level == other.Level &&
				Weight == other.Weight &&
				Text.Equals(other.Text) &&
				IncorrectAnswers.Equals(other.IncorrectAnswers) &&
				CorrectAnswer.Equals(other.CorrectAnswer) &&
				Hint.Equals(other.Hint) &&
				UpdatedAt.Equals(other.UpdatedAt) &&
				Version == other.Version;
	}
	
	public override int GetHashCode()
	{
		int hintHash = (Hint == null) ? "".GetHashCode() : Hint.GetHashCode();
		return Identifier.GetHashCode() ^ Level.GetHashCode() ^
				Weight.GetHashCode() ^
				Text.GetHashCode() ^
				IncorrectAnswers.GetHashCode() ^
				CorrectAnswer.GetHashCode() ^
				hintHash ^
				UpdatedAt.GetHashCode() ^
				Version.GetHashCode();
	}
	#endregion
}

