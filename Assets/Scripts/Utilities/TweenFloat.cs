// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;

public class TweenFloat : UITweener
{
	public float from = 0f;
	public float to = 1f;
	
	public float CurrentValue { get; private set; }
	
	public delegate void UpdateTween(float current, bool isFinished);
	public event UpdateTween onTweenUpdate;
	
	override protected void OnUpdate(float factor, bool isFinished)
	{
		CurrentValue = Mathf.Lerp(from, to, factor);
		if(onTweenUpdate != null) {
			onTweenUpdate(CurrentValue, isFinished);
		}
	}
	
	static public TweenFloat Begin (GameObject go, float duration, float fromValue, float toValue)
	{
		return TweenFloat.Begin(go, duration, fromValue, toValue, null);
	}

	static public TweenFloat Begin (GameObject go, float duration, float fromValue, float toValue, UpdateTween callback)
	{
		TweenFloat comp = UITweener.Begin<TweenFloat>(go, duration);
		comp.from = fromValue;
		comp.to = toValue;
		if(callback != null) {
			comp.onTweenUpdate += callback;
		}

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}