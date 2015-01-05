// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Data;
using Mono.Data.Sqlite;

public class IFQuestionCategory : object
{
	private static HashSet<IFQuestionCategory> instances = new HashSet<IFQuestionCategory>();
	
	public int Identifier { get; private set; }

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

	private string mName;
	public string Name
	{
		get
		{
			return mName;
		}
		set
		{
			if(mName == null || !mName.Equals(value)) {
				mName = value;
				isDirty = true;
			}
		}
	}
	
	private bool isDirty = false;


	#region JSON Parsing
	public static void LoadCategoriesFromJSON(string json)
	{
		ArrayList decodedJson = MiniJSON.jsonDecode(json) as ArrayList;
		if(decodedJson != null) {
			foreach(Hashtable item in decodedJson) {
				string name = item["name"] as string;
				int remoteId = Convert.ToInt32(item["id"]);
				IFQuestionCategory category = IFQuestionCategory.CategoryWithRemoteId(remoteId);
				if(category == null) {
					category = new IFQuestionCategory(-1, remoteId, name);
				} else {
					category.Name = name;
				}
				category.Save();
			}
		}
	}
	#endregion

	
	public IFQuestionCategory(string name) : this(-1, -1, name) {}
	
	public IFQuestionCategory(int id, int remoteId, string name)
	{
		Identifier = id;
		RemoteId = remoteId;
		Name = name;
		isDirty = id == -1;
	}

	public static IFQuestionCategory CategoryNamed(string name)
	{
		IFQuestionCategory instance = CategoryWithName(name);
		if(instance == null) {
			instance = new IFQuestionCategory(name);
			instance.Save();
		}
		return instance;
	}
	
	public static IFQuestionCategory CategoryWithName(string name)
	{
		IFQuestionCategory category = instances.FirstOrDefault((cat) => { return cat.Name.Equals(name); });
		if(category == null) {
			Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@name",name} };
			using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, remote_id, name FROM categories WHERE name=@name", parameters)) {
				if(reader.HasRows) {
					reader.Read();
					category = new IFQuestionCategory(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2));
					instances.Add(category);
				}
			}
		}
		return category;
	}
	
	public static IFQuestionCategory CategoryWithRemoteId(int remoteId)
	{
		IFQuestionCategory category = instances.FirstOrDefault((cat) => { return cat.RemoteId == remoteId; });
		if(category == null) {
			Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@remote_id",remoteId} };
			using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, remote_id, name FROM categories WHERE remote_id=@remote_id", parameters)) {
				if(reader.HasRows)
				{
					reader.Read();
					category = new IFQuestionCategory(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2));
					instances.Add(category);
				}
			}
		}
		return category;
	}

	public static IFQuestionCategory BiggestCategory()
	{
		object remoteId = IFDatabase.SharedDatabase.ExecuteScalar("SELECT q.category_id FROM questions AS q LEFT JOIN categories AS c ON q.category_id=c.remote_id GROUP BY q.category_id ORDER BY COUNT(q.category_id) DESC LIMIT 1;");
		return CategoryWithRemoteId(Convert.ToInt32(remoteId));
	}

	public static IFQuestionCategory GetRandomCategory()
	{
		IFQuestionCategory category;
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@count",1} };
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, remote_id, name FROM categories ORDER BY RANDOM() LIMIT @count", parameters)) {
			reader.Read();
			int id = reader.GetInt32(0);
			category = instances.FirstOrDefault((cat) => { return cat.Identifier == id; });
			if(category == null) {
				category = new IFQuestionCategory(id, reader.GetInt32(1), reader.GetString(2));
				instances.Add(category);
			}
		}
		return category;
	}
	
	public static List<IFQuestionCategory> AllCategories()
	{
		string[] cachedIds = instances.Select(cat => cat.Identifier.ToString()).ToArray();
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, remote_id, name FROM categories WHERE id NOT IN ("+string.Join(",", cachedIds)+")")) {
			if(reader.HasRows) {
				while(reader.Read()) {
					instances.Add(new IFQuestionCategory(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2)));
				}
			}
		}
		return instances.ToList();
	}

	public static int CountOfAllCategories()
	{
		object countObject = IFDatabase.SharedDatabase.ExecuteScalar ("SELECT COUNT(*) FROM categories;");
		return Convert.ToInt32 (countObject);
	}
	
	public void Save()
	{
		if(isDirty) {
			if(Identifier < 0) {
				Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@name",Name}, {"@remote_id",RemoteId} };
				IFDatabase.SharedDatabase.ExecuteQuery("INSERT INTO categories (name, remote_id) VALUES (@name, @remote_id)", parameters);
				object rowId = IFDatabase.SharedDatabase.ExecuteScalar("SELECT last_insert_rowid()");
				Identifier = Convert.ToInt32(rowId);
			} else {
				Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@name",Name}, {"@remote_id",RemoteId}, {"@id", Identifier} };
				IFDatabase.SharedDatabase.ExecuteQuery("UPDATE categories SET name=@name, remote_id=@remote_id WHERE id=@id", parameters);
			}
			isDirty = false;
		}
	}
	
	public override bool Equals(object obj)
	{
		if(obj == null) {
			return false;
		}
		
		IFQuestionCategory other = obj as IFQuestionCategory;
		if(other == null) {
			return false;
		}
		
		return this.Equals(other);
	}
	
	public bool Equals(IFQuestionCategory other)
	{
		return Identifier == other.Identifier && RemoteId == other.RemoteId && Name.Equals(other.Name);
	}
	
	public override int GetHashCode()
	{
		return Identifier.GetHashCode() ^ RemoteId.GetHashCode() ^ Name.GetHashCode();
	}
}

