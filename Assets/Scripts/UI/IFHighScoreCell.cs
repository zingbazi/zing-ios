// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;
using System;

public class IFHighScoreCell : MonoBehaviour {
	
	public UIWidget background;
	public UISysFontLabel scoreLabel;
	public UISysFontLabel rankLabel;
	public UISysFontLabel usernameLabel;
	
	public bool needsUpdate = false;
	
	private string mUsername = "";
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
				needsUpdate = true;
			}
		}
	}
	
	private int mRank = 0;
	public int Rank
	{
		get
		{
			return mRank;
		}
		set
		{
			int newValue = Mathf.Clamp(value, 0, int.MaxValue);
			if(mRank != newValue) {
				mRank = newValue;
				needsUpdate = true;
			}
		}
	}
	
	private int mScore = int.MaxValue;
	public int Score
	{
		get
		{
			return mScore;
		}
		set
		{
			int newValue = Mathf.Clamp(value, 0, int.MaxValue);
			if(mScore != newValue) {
				mScore = newValue;
				needsUpdate = true;
			}
		}
	}
	
	void Start()
	{
		needsUpdate = true;
	}
	
	
	void Update()
	{
		if(needsUpdate) {
			if(Rank > 0 && Rank < int.MaxValue) {
				rankLabel.Text = Rank.ToString();
			} else {
				rankLabel.Text = "";
			}
			
			if(Score < int.MaxValue) {
				scoreLabel.Text = Score.ToString();
			} else {
				scoreLabel.Text = "";
			}
			
			if(Username != null) {
				usernameLabel.Text = Username;
			} else {
				usernameLabel.Text = "";
			}
			
			needsUpdate = false;
		}
	}
}
