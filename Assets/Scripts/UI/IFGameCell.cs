// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFGameCell : MonoBehaviour {

	public UISlider p1ProgressBar;
	public UISlider p2ProgressBar;
	
	public UILabel p1ScoreLabel;
	public UILabel p2ScoreLabel;
	
	public GameObject p1CompleteSprite;
	public GameObject p2CompleteSprite;
	
	public UISysFontLabel p1UsernameLabel;
	public UISysFontLabel p2UsernameLabel;
	
	public delegate void CellWasSelected(IFGameCell cell, IFChallenge challenge);
	public CellWasSelected CellWasSelectedDelegate;
	
	private bool needsUpdate;
	private UIDragPanelContents cachedDragPanelContents;
	
	private IFChallenge mChallenge;
	public IFChallenge Challenge
	{
		get
		{
			return mChallenge;
		}
		set
		{
			if(mChallenge == null || !mChallenge.Equals(value)) {
				mChallenge = value;
				needsUpdate = true;
			}
		}
	}
	
	public UIDraggablePanel DraggablePanel
	{
		get
		{
			return cachedDragPanelContents.draggablePanel;
		}
		set
		{
			if(cachedDragPanelContents == null) {
				cachedDragPanelContents = GetComponent<UIDragPanelContents>();
			}
			cachedDragPanelContents.draggablePanel = value;
		}
	}
	
	void Update()
	{
		if(needsUpdate && Challenge != null) {
			p1ScoreLabel.text = Challenge.UserScore.ToString();
			p2ScoreLabel.text = Challenge.OpponentScore.ToString();
			
			p1UsernameLabel.Text = Challenge.Username;
			p2UsernameLabel.Text = Challenge.OpponentUsername;
			
			
			if(Challenge.UserAnswerCount == Challenge.QuestionCount) {
				p1CompleteSprite.SetActive(true);
				p1ProgressBar.gameObject.SetActive(false);
			} else {
				p1CompleteSprite.SetActive(false);
				p1ProgressBar.gameObject.SetActive(true);
				float percentComplete = (float)Challenge.UserAnswerCount / (float)Mathf.Clamp(Challenge.QuestionCount, 1, int.MaxValue);
				if(percentComplete != p1ProgressBar.sliderValue) {
					TweenFloat tween = TweenFloat.Begin(p1ProgressBar.gameObject, .3f, p1ProgressBar.sliderValue, percentComplete);
					tween.onTweenUpdate += (current, isFinished) => {
						p1ProgressBar.sliderValue = current;
						if(isFinished) Destroy(tween);
					};
					tween.method = UITweener.Method.EaseInOut;
				}
			}

			if(Challenge.OpponentAnswerCount == Challenge.QuestionCount) {
				p2CompleteSprite.SetActive(true);
				p2ProgressBar.gameObject.SetActive(false);
			} else {
				p2CompleteSprite.SetActive(false);
				p2ProgressBar.gameObject.SetActive(true);
				float percentComplete = (float)Challenge.OpponentAnswerCount / (float)Mathf.Clamp(Challenge.QuestionCount, 1, int.MaxValue);
				if(percentComplete != p2ProgressBar.sliderValue) {
					TweenFloat tween = TweenFloat.Begin(p2ProgressBar.gameObject, .3f, p2ProgressBar.sliderValue, percentComplete);
					tween.onTweenUpdate += (current, isFinished) => {
						p2ProgressBar.sliderValue = current;
						if(isFinished) Destroy(tween);
					};
					tween.method = UITweener.Method.EaseInOut;
				}
			}
			needsUpdate = false;
		}
	}
	
	void Start()
	{
		cachedDragPanelContents = GetComponent<UIDragPanelContents>();
	}
	
	void OnEnable()
	{
		needsUpdate = true;
		p1ProgressBar.sliderValue = 0f;
		p2ProgressBar.sliderValue = 0f;
	}
	
	void OnClick()
	{
		if(CellWasSelectedDelegate != null) {
			CellWasSelectedDelegate(this, Challenge);
		}
	}
}
