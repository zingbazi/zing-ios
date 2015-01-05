// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System.Text;

[ExecuteInEditMode]
public class IFDatabase : MonoBehaviour {
	
	public static string databaseResourceName = "database";
	public static bool recreateDatabase = false;
	
	[SerializeField]
	public bool logQueries = false;
	
	[SerializeField]
	public bool logEvents = false;

	
	private SqliteConnection connection = null;
	private SqliteTransaction currentTransaction = null;
	
	private static IFDatabase mShared = null;
	
	public static IFDatabase SharedDatabase
	{
		get
		{
			if(mShared == null) {
				GameObject go = GameObject.FindGameObjectWithTag("Database Manager");
				if(go == null) {
					go = new GameObject("Database Manager");
					go.tag = "Database Manager";
					go.AddComponent<IFDatabase>();
				}
				mShared = go.GetComponent<IFDatabase>();
			}
			return mShared;
		}
	}
	
	public static void FreeResources()
	{
		if(mShared != null) {
			NGUITools.Destroy(mShared.gameObject);
			mShared = null;
		}
	}
	
	void OnDestroy()
	{
		if(currentTransaction != null) {
			currentTransaction.Commit();
			currentTransaction = null;
		}
		if(connection != null) {
			connection.Close();
			connection = null;
		}
		
		mShared = null;
	}
	
	void Awake()
	{
		if(mShared == null) {
			mShared = this;
		}

		string dbPath = Application.persistentDataPath + "/" + IFDatabase.databaseResourceName + ".sqlite";
		string dbURI = "URI=file:" + dbPath;
		
		FileInfo info = new FileInfo(dbPath);
		if(!info.Exists || IFDatabase.recreateDatabase) {
			TextAsset sqliteTemplateFile = Resources.Load(IFDatabase.databaseResourceName) as TextAsset;
			using(FileStream fs = info.OpenWrite()) {
				fs.Write(sqliteTemplateFile.bytes, 0, sqliteTemplateFile.bytes.Length);
				fs.Close();
			}
		}
		
		if(logEvents) Debug.Log("Connecting to db with URI: "+dbURI);
		connection = new SqliteConnection(dbURI);
#if UNITY_EDITOR
		connection.StateChange +=  (sender, e) => {
			if(logEvents) Debug.Log("DB Connection state changed from "+e.OriginalState.ToString()+" to "+e.CurrentState.ToString());
		};
		connection.Commit += delegate(object sender, CommitEventArgs e) {
			if(logEvents) Debug.Log("Commit event: "+e);
		};
		
		connection.Update += delegate(object sender, UpdateEventArgs e) {
			if(logEvents) Debug.Log("["+e.Database+"] "+e.Event.ToString() + " row " + e.RowId + " on " + e.Table);
		};
#endif
		connection.Open();
    }
	
	public void BeginTransaction()
	{
		if(currentTransaction != null) {
			currentTransaction.Commit();
		}
		currentTransaction = connection.BeginTransaction();
		if(logQueries) {
			Debug.Log("BEGIN TRANSACTION");
		}
	}
	
	public void RollbackTransaction()
	{
		if(currentTransaction != null) {
			currentTransaction.Rollback();
			currentTransaction = null;
			if(logQueries) {
				Debug.Log("ROLLBACK TRANSACTION");
			}
		}
	}
	
	public void CommitTransaction()
	{
		if(currentTransaction != null) {
			currentTransaction.Commit();
			currentTransaction = null;
			if(logQueries) {
				Debug.Log("COMMIT TRANSACTION");
			}
		}
	}
	
	public SqliteDataReader ExecuteQuery(string query)
	{
		return ExecuteQuery(query, null);
	}
	
	public SqliteDataReader ExecuteQuery(string query, Dictionary<string, object> parameters)
	{
		if(logQueries) {
			StringBuilder sb = new StringBuilder("DB QUERY: \"");
			sb.Append(query);
			if(parameters != null) {
				foreach(KeyValuePair<string, object> param in parameters) {
					string val = param.Value == null ? string.Empty : param.Value.ToString();
					sb.Replace(param.Key, val);
				}
			}
			sb.Append("\"");
			Debug.Log(sb.ToString());
		}

		SqliteDataReader reader = null;
	    using(SqliteCommand command = connection.CreateCommand()) {
	        command.CommandText = query;
			if(parameters != null) {
				foreach(KeyValuePair<string, object> param in parameters) {
					command.Parameters.AddWithValue(param.Key, param.Value);	
				}
			}
			reader = command.ExecuteReader();
		}
        return reader;
	}

	public int ExecuteNonQuery(string query)
	{
		return ExecuteNonQuery(query, null);
	}

	public int ExecuteNonQuery(string query, Dictionary<string, object> parameters)
	{
		if(logQueries) {
			StringBuilder sb = new StringBuilder("DB QUERY (NON QUERY): \"");
			sb.Append(query);
			if(parameters != null) {
				foreach(KeyValuePair<string, object> param in parameters) {
					string val = param.Value == null ? string.Empty : param.Value.ToString();
					sb.Replace(param.Key, val);
				}
			}
			sb.Append("\"");
			Debug.Log(sb.ToString());
		}
		int result;
		using(SqliteCommand command = connection.CreateCommand()) {
			command.CommandText = query;
			if(parameters != null) {
				foreach(string paramName in parameters.Keys) {
					command.Parameters.AddWithValue(paramName, parameters[paramName]);	
				}
			}
			result = command.ExecuteNonQuery();
		}
		return result;
	}
	
	public object ExecuteScalar(string query)
	{
		return ExecuteScalar(query, null);
	}
	
	public object ExecuteScalar(string query, Dictionary<string, object> parameters)
	{
		if(logQueries) {
			StringBuilder sb = new StringBuilder("DB QUERY (SCALAR): \"");
			sb.Append(query);
			if(parameters != null) {
				foreach(KeyValuePair<string, object> param in parameters) {
					string val = param.Value == null ? string.Empty : param.Value.ToString();
					sb.Replace(param.Key, val);
				}
			}
			sb.Append("\"");
			Debug.Log(sb.ToString());
		}
		object result;
		using(SqliteCommand command = connection.CreateCommand()) {
			command.CommandText = query;
			if(parameters != null) {
				foreach(string paramName in parameters.Keys) {
					command.Parameters.AddWithValue(paramName, parameters[paramName]);	
				}
			}
			result = command.ExecuteScalar();
		}
		return result;
	}
}
