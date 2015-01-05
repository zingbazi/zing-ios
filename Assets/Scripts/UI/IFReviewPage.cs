// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFReviewPage : MonoBehaviour {
	
	public static int maxAnswerCount = 3;
	
	private bool needsCellUpdate = true;
	public IFReviewCell[] cells;
	
	private IFAnswer[] mAnswers;
	public IFAnswer[] Answers
	{
		get
		{
			return mAnswers;
		}
		set
		{
			int count = Mathf.Min(IFReviewPage.maxAnswerCount, value.Length);
			mAnswers = new IFAnswer[count];
			for(int i = 0; i < count; i++) {
				mAnswers[i] = value[i];
				needsCellUpdate = true;
			}
		}
	}
	
	private IFAnswer[] mOpponentAnswers;
	public IFAnswer[] OpponentAnswers
	{
		get
		{
			return mOpponentAnswers;
		}
		set
		{
			int count = Mathf.Min(IFReviewPage.maxAnswerCount, value.Length);
			mOpponentAnswers = new IFAnswer[count];
			for(int i = 0; i < count; i++) {
				mOpponentAnswers[i] = value[i];
				needsCellUpdate = true;
			}
		}
	}
	
	private string mPlayerUsername;
	public string PlayerUsername
	{
		get
		{
			return mPlayerUsername;
		}
		set
		{
			mPlayerUsername = value;
			needsCellUpdate = true;
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
			mOpponentUsername = value;
			needsCellUpdate = true;
		}
	}


	void Start()
	{
		if(cells == null) {
			cells = GetComponentsInChildren<IFReviewCell>();	
		}
	}

	void Update()
	{
		if(needsCellUpdate) {
			UpdateCells();
		}
	}
	
	void UpdateCells()
	{
		if(Answers == null && OpponentAnswers == null) {
			foreach(IFReviewCell cell in cells) {
				cell.gameObject.SetActive(false);
			}
			return;
		}
		
		for(int i = 0; i < cells.Length; i++) {
			cells[i].PlayerUsername = PlayerUsername;
			cells[i].OpponentUsername = OpponentUsername;
			if(i < Answers.Length) {
				cells[i].Answer = Answers[i];
				if(OpponentAnswers != null && OpponentAnswers.Length > i) {
					cells[i].OpponentAnswer = OpponentAnswers[i];
				} else {
					cells[i].OpponentAnswer = null;
				}
			} else {
				cells[i].Answer = null;
				cells[i].OpponentAnswer = null;
				cells[i].gameObject.SetActive(false);
			}
		}
		needsCellUpdate = false;
	}
}
