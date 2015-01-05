// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class IFUploadManager : MonoBehaviour {

	public float queueFlushTimeoutSeconds = 10f;
	private float secondsSinceLastFlush = 0f;
	private bool isUploadInProgress = false;

	private static IFUploadManager mShared = null;
	public static IFUploadManager SharedManager
	{
		get
		{
			if(mShared == null) {
				GameObject go = GameObject.FindGameObjectWithTag("Upload Manager");
				if(go == null) {
					go = new GameObject("Upload Manager");
					go.tag = "Upload Manager";
					go.AddComponent<IFUploadManager>();
				}
				mShared = go.GetComponent<IFUploadManager>();
			}
			return mShared;
		}
	}

	void OnDestroy()
	{
		mShared = null;
	}

	public void FlushNow()
	{
		secondsSinceLastFlush = float.MaxValue;
	}

	void Awake()
	{
		if(mShared == null) {
			mShared = this;
		}
		AddDatabaseTableIfNeeded();
		secondsSinceLastFlush = queueFlushTimeoutSeconds;
	}

	void Update()
	{
		secondsSinceLastFlush += Time.deltaTime;
		if(secondsSinceLastFlush < queueFlushTimeoutSeconds) {
			return;
		}

		if(!isUploadInProgress && IFUploadItem.CountOfItemsInQueue() > 0) {
			isUploadInProgress = true;
			StartCoroutine(UploadNextQueuedItem((item, success) => {
				isUploadInProgress = false;
				secondsSinceLastFlush = 0f;
				if(success) {
					item.Delete();
				} else {
					item.LastAttempt = DateTime.UtcNow;
					item.Save();
				}
			}));
		}

		secondsSinceLastFlush = 0f;
	}

	IEnumerator UploadNextQueuedItem(Action<IFUploadItem, bool> completion)
	{
		IFUploadItem item = IFUploadItem.GetNextItem();
		if(item == null) {
			completion(item, false);
			yield break;
		}

		Hashtable headers = new Hashtable();
		headers["Content-Type"] = "application/json";
		headers["Accept"] = "application/json";
		WWW web = new WWW(IFGameManager.SharedManager.remoteURLs.baseURL + item.Endpoint, Encoding.UTF8.GetBytes(item.Data), headers);
		yield return web;

		bool success = (web.error == null);
		completion(item, success);
	}

	void AddDatabaseTableIfNeeded()
	{
		object exists = IFDatabase.SharedDatabase.ExecuteScalar("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND tbl_name='upload_queue'");
		if(Convert.ToInt32(exists) == 0) {
			TextAsset schema = Resources.Load("upload_queue_schema_sql") as TextAsset;
			IFDatabase.SharedDatabase.ExecuteNonQuery(schema.text);
		}
	}

	public static void QueueUploadDataForEndpoint(string endpoint, string data)
	{
		IFUploadItem.CreateQueueItemForDataAtEndpoint(endpoint, data);
	}
	
	public void SubmitPasswordResetRequest(string email, Action<bool> callback)
	{
		StartCoroutine(DoPasswordReset(email, callback));
	}
	
	public IEnumerator DoPasswordReset(string email, Action<bool> callback)
	{
		WWWForm form = new WWWForm();
		form.AddField("email", email);
		
		WWW web = new WWW(IFGameManager.SharedManager.remoteURLs.PasswordReset, form);
		yield return web;
		
		callback(web.error == null);
	}

	public static void CanReachURLAsync(string url, Action<bool, IFError> callback, float timeout = 10f)
	{
		SharedManager.StartCoroutine(SharedManager.CanReachURLAsyncCoroutine(url, callback, timeout));
	}

	public IEnumerator CanReachURLAsyncCoroutine(string url, Action<bool, IFError> callback, float timeout = 10f)
	{
		float accumulatedTime = 0f;
		IFError preformattedErrorMessage = new IFError(Localization.Localize("Internet Connection Failed"), Localization.Localize("Cannot access url") + " " + url + " " + Localization.Localize("on the Internet. Please check your connection."));

		WWW www = new WWW(url);
		while(!www.isDone) {
			accumulatedTime += Time.deltaTime;
			if(accumulatedTime >= timeout) {
				if(callback != null) callback(false, preformattedErrorMessage);
				Debug.LogError("URL fetch timeout ("+url+")");
				yield break;
			}
			yield return null;
		}
		if(string.IsNullOrEmpty(www.error)) {
			if(callback != null) callback(true, IFError.Null);
		} else {
			Debug.LogError("WWW Error: "+www.error);
			if(callback != null) callback(false, preformattedErrorMessage);
		}
	}

	public static void CanReachDomainAsync(string domain, Action<bool, IFError> callback, float timeout = 10f)
	{
		SharedManager.StartCoroutine(SharedManager.CanReachDomainAsyncCoroutine(domain, callback, timeout));
	}

	public IEnumerator CanReachDomainAsyncCoroutine(string domain, Action<bool, IFError> callback, float timeout = 10f)
	{
		float accumulatedTime = 0f;
		IFError preformattedErrorMessage = new IFError(Localization.Localize("Internet Connection Failed"), Localization.Localize("Cannot access host") + " " + domain + " " + Localization.Localize("on the Internet. Please check your connection."));

		IAsyncResult asyncResult = Dns.BeginGetHostAddresses(domain, null, null);
		while(!asyncResult.IsCompleted) {
			accumulatedTime += Time.deltaTime;
			if(accumulatedTime >= timeout) {
				if(callback != null) callback(false, preformattedErrorMessage);
				yield break;
			}
			yield return null;
		}
		try {
			IPAddress[] addresses = Dns.EndGetHostAddresses(asyncResult);
			if(addresses.Length > 0) {
				StartCoroutine(CanPingIPAsync(addresses[0].ToString(), callback, timeout));
			} else {
				if(callback != null) callback(false, preformattedErrorMessage);
			}
		} catch (SocketException e) {
			Debug.LogError("Can't resolve DNS for "+domain+": "+e.Message);
			if(callback != null) callback(false, preformattedErrorMessage);
		}
	}

	IEnumerator CanPingIPAsync(string ip, Action<bool, IFError> callback, float timeout = 10f)
	{
		float accumulatedTime = 0f;
		IFError preformattedErrorMessage = new IFError(Localization.Localize("Internet Connection Failed"), Localization.Localize("Cannot access ip address") + " " + ip + " " + Localization.Localize("on the Internet. Please check your connection."));

		Ping ping = new Ping(ip);
		while(!ping.isDone) {
			accumulatedTime += Time.deltaTime;
			if(accumulatedTime >= timeout) {
				if(callback != null) callback(false, preformattedErrorMessage);
				yield break;
			}
			yield return null;
		}
		if(callback != null) callback(true, IFError.Null);
	}
}
