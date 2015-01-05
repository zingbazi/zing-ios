// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Linq;

public class IFGameLevel : object
{
	public int Identifier { get; private set; }
	public int Rank { get; set; }
	public int QuestionCount { get; set; }
	public float Duration { get; set; }
	public float QuestionDuration { get; set; }
	
	public IFGameLevel(int id, int rank, int questionCount, float duration, float questionDuration)
	{
		Identifier = id;
		Rank = rank;
		QuestionCount = questionCount;
		Duration = duration;
		QuestionDuration = questionDuration;
	}
	
	public IFGameLevel(int rank, int questionCount, float duration) : this(-1, rank, questionCount, duration, duration/questionCount) {}
	
	public static IFGameLevel LevelWithIdentifier(int id)
	{
		IFGameLevel level;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@id", id} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("select id, rank, question_count, duration, question_duration from levels where id=@id", parameters)) {
			reader.Read();
			level = new IFGameLevel(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetFloat(3),reader.GetFloat(4));
		}
		return level;
	}
	
	public static IFGameLevel LevelWithRank(int rank)
	{
		IFGameLevel level;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@rank", rank} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("select id, rank, question_count, duration, question_duration from levels where rank=@rank", parameters)) {
			reader.Read();
			level = new IFGameLevel(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetFloat(3),reader.GetFloat(4));
		}
		return level;
	}
	
	public Queue<int> GetRandomQuestionIdQueue()
	{
		return GetRandomQuestionIdQueue(QuestionCount);
	}
	
	public Queue<int> GetRandomQuestionIdQueue(int count)
	{
		return GetRandomQuestionIdQueue(count, false);
	}
	
	public Queue<int> GetRandomQuestionIdQueue(int count, bool useMaxRank)
	{
		Queue<int> idQueue = new Queue<int>();
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@rank", Rank}, {"@count", count} };
		string queryString;
		if(useMaxRank) {
			queryString = "SELECT id FROM questions WHERE level<=@rank ORDER BY RANDOM() LIMIT @count";
		} else {
			queryString ="SELECT id FROM questions WHERE level=@rank ORDER BY RANDOM() LIMIT @count";
		}
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery(queryString, parameters)) {
			while(reader.Read()) {
				idQueue.Enqueue(reader.GetInt32(0));
			}
		}
		return idQueue;
	}
	
	public static IFGameLevel GetLevelWithMinimumRank()
	{
		object rank = IFDatabase.SharedDatabase.ExecuteScalar("SELECT MIN(level) FROM questions");
		return IFGameLevel.LevelWithRank(Convert.ToInt32(rank));
	}
	
	public static IFGameLevel GetRandomLevel()
	{
		object rank = IFDatabase.SharedDatabase.ExecuteScalar("SELECT rank FROM levels ORDER BY RANDOM() LIMIT 1");
		return IFGameLevel.LevelWithRank(Convert.ToInt32(rank));
	}

	public Queue<int> GetRandomQuestionIdQueue(IFQuestionCategory category)
	{
		return GetRandomQuestionIdQueue(QuestionCount, category);
	}
	
	public Queue<int> GetRandomQuestionIdQueue(int count, IFQuestionCategory category)
	{
		Queue<int> idQueue = new Queue<int>();
		Dictionary<string, object> parameters = new Dictionary<string, object>() {
			{"@rank", Rank},
			{"@count", count},
			{"@category_id", category.RemoteId}
		};
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id FROM questions WHERE level=@rank AND category_id=@category_id ORDER BY RANDOM() LIMIT @count", parameters)) {
			while(reader.Read()) {
				idQueue.Enqueue(reader.GetInt32(0));
			}
		}
		return idQueue;
	}
	
	public int GetCountOfAvailableQuestions()
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@rank", Rank} };
		object count = IFDatabase.SharedDatabase.ExecuteScalar("select count (*) from questions where level=@rank", parameters);
		return Convert.ToInt32(count);
	}
	
	public static int MaxRank
	{
		get
		{
			object maxRank = IFDatabase.SharedDatabase.ExecuteScalar("SELECT MAX(rank) FROM levels");
			return Convert.ToInt32(maxRank);
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
	
	public bool Equals(IFGameLevel other)
	{
		return Identifier == other.Identifier && Rank == other.Rank;
	}
	
	public override int GetHashCode()
	{
		return Identifier.GetHashCode() ^ Rank.GetHashCode();
	}
}
