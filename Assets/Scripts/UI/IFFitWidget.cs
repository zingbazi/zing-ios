// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IFFitWidget : MonoBehaviour
{
	public UIWidget target;
	public Vector2 padding = Vector2.zero;
	public Vector2 minimum = Vector2.one;
	public enum ConstrainAxis { X, Y, XY };
	public ConstrainAxis constrain = ConstrainAxis.XY;
	
	private Transform cachedTransform;
	private Vector3 lastScale = Vector3.zero;
	
	void Start()
	{
		cachedTransform = GetComponent<Transform>();
	}
	
	void Update()
	{
		Vector3 targetScale = target.cachedTransform.localScale;

		Vector3 targetSize = target.relativeSize;

		float width = targetSize.x * targetScale.x;
		float height = targetSize.y * targetScale.y;
		
		Vector3 scale = cachedTransform.localScale;
		
		padding.x = Mathf.Clamp(padding.x, 0f, float.MaxValue);
		padding.y = Mathf.Clamp(padding.y, 0f, float.MaxValue);
		if(constrain == ConstrainAxis.X || constrain == ConstrainAxis.XY) scale.x = Mathf.Max(minimum.x, width + padding.x);
		if(constrain == ConstrainAxis.Y || constrain == ConstrainAxis.XY) scale.y = Mathf.Max(minimum.y, height + padding.y);
		if(lastScale != scale) {
			cachedTransform.localScale = scale;
			lastScale = scale;
		}
	}
	
}
