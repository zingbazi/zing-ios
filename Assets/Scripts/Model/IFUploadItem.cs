// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class IFUploadItem : object
{
	#region Properties

	public int Identifier { get; private set; }

	private DateTime mDateAdded;
	public DateTime DateAdded
	{ 
		get
		{
			return mDateAdded;
		}
		set
		{
			if(!mDateAdded.Equals(value)) {
				mDateAdded = value;
				isDirty = true;
			}
		}
	}

	private DateTime mLastAttempt;
	public DateTime LastAttempt
	{ 
		get
		{
			return mLastAttempt;
		}
		set
		{
			if(!mLastAttempt.Equals(value)) {
				mLastAttempt = value;
				isDirty = true;
			}
		}
	}

	private string mEndpoint;
	public string Endpoint
	{
		get
		{
			return mEndpoint;
		}
		set
		{
			if(mEndpoint == null || !mEndpoint.Equals(value)) {
				mEndpoint = value;
				isDirty = true;
			}
		}
	}

	private string mMethod;
	public string Method
	{
		get
		{
			return mMethod;
		}
		set
		{
			if(mMethod == null || !mMethod.Equals(value)) {
				mMethod = value;
				isDirty = true;
			}
		}
	}

	private string mData;
	public string Data
	{
		get
		{
			return mData;
		}
		set
		{
			if(mData == null || !mData.Equals(value)) {
				mData = value;
				isDirty = true;
			}
		}
	}

	private bool isDirty = false;

	#endregion

	public IFUploadItem(int id, DateTime dateAdded, DateTime lastAttempt, string endpoint, string method, string data)
	{
		Identifier = id;
		DateAdded = dateAdded;
		LastAttempt = lastAttempt;
		Endpoint = endpoint;
		Method = method;
		Data = data;
	}

	public IFUploadItem(DateTime dateAdded, DateTime lastAttempt, string endpoint, string method, string data) : this(-1, dateAdded, lastAttempt, endpoint, method, data) {}

	public void Save()
	{
		if(isDirty) {
			if(Identifier < 0) {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@date_added", DateAdded},
					{"@last_attempt", LastAttempt},
					{"@endpoint", Endpoint},
					{"@method", Method},
					{"@data", Data}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("INSERT INTO upload_queue (date_added, last_attempt, endpoint, method, data) VALUES (@date_added, @last_attempt, @endpoint, @method, @data)", parameters);
				object rowId = IFDatabase.SharedDatabase.ExecuteScalar("SELECT last_insert_rowid()");
				Identifier = Convert.ToInt32(rowId);
			} else {
				Dictionary<string, object> parameters = new Dictionary<string, object>() {
					{"@id", Identifier},
					{"@date_added", DateAdded},
					{"@last_attempt", LastAttempt},
					{"@endpoint", Endpoint},
					{"@method", Method},
					{"@data", Data}
				};
				IFDatabase.SharedDatabase.ExecuteQuery("UPDATE upload_queue SET date_added=@date_added, last_attempt=@last_attempt, endpoint=@endpoint, method=@method, data=@data WHERE id=@id", parameters);
			}
			isDirty = false;
		}
	}

	public bool Delete()
	{
		Dictionary<string, object> parameters = new Dictionary<string, object>() { {"@id", Identifier} };
		int result = IFDatabase.SharedDatabase.ExecuteNonQuery("DELETE FROM upload_queue WHERE id=@id", parameters);
		return result == 1;
	}

	public static IFUploadItem CreateQueueItemForDataAtEndpoint(string endpoint, string data)
	{
		IFUploadItem item = new IFUploadItem(DateTime.UtcNow, DateTime.MinValue, endpoint, "POST", data);
		item.Save();
		return item;
	}

	public static IFUploadItem GetNextItem()
	{
		IFUploadItem item = null;
		using(SqliteDataReader reader = IFDatabase.SharedDatabase.ExecuteQuery("SELECT id, date_added, last_attempt, endpoint, method, data FROM upload_queue ORDER BY date_added ASC, last_attempt ASC LIMIT 1")) {
			if(reader.HasRows) {
				while(reader.Read()) {
					item = new IFUploadItem(reader.GetInt32(0), reader.GetDateTime(1), reader.GetDateTime(2), reader.GetValue(3).ToString(), reader.GetValue(4).ToString(), reader.GetValue(5).ToString());
				}
			}
		}
		return item;
	}

	public static int CountOfItemsInQueue()
	{
		object count = IFDatabase.SharedDatabase.ExecuteScalar("SELECT COUNT(*) FROM upload_queue");
		return Convert.ToInt32(count);
	}
}
