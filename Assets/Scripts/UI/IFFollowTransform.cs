// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IFFollowTransform : MonoBehaviour {

	public Transform target;
	
	public enum Axes { X, Y, XY };
	public Axes axes;
	
	public Vector2 offset = Vector2.zero;

	private Transform cachedTransform;
	private Vector3 lastPosition;
	
	void Start()
	{
		cachedTransform = GetComponent<Transform>();
		if(target != null) {
			lastPosition = target.position;
		}
	}
	
	void OnEnable()
	{
		if(target != null) {
			lastPosition = target.position;
		}
	}
	
	void Update()
	{
		if(target != null) {
			Vector3 pos = cachedTransform.position;
			bool changed = false;
			if((axes == Axes.X || axes == Axes.XY) && lastPosition.x != target.position.x) {
				pos.x = target.position.x;
				lastPosition.x = pos.x + offset.x;
				changed = true;
			}
			
			if((axes == Axes.Y || axes == Axes.XY) && lastPosition.y != target.position.y) {
				pos.y = target.position.y;
				lastPosition.y = pos.y + offset.y;
				changed = true;
			}
			
			if(changed) cachedTransform.position = pos;
		}
	}
}
