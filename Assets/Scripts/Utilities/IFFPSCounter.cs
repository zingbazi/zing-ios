// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

public class IFFPSCounter : MonoBehaviour
{
	public float updateInterval = 0.5f;
	public UILabel label;
	
	private int frames = 0;
	
    private float lastInterval;
	
	void Start ()
	{
		if(label == null) {
			label = GetComponent<UILabel>();
		}
		lastInterval = Time.realtimeSinceStartup;
	}
	
	void Update ()
	{
		++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval) {
            float fps = frames / (timeNow - lastInterval);
            frames = 0;
            lastInterval = timeNow;
			label.text = Mathf.FloorToInt(fps).ToString();
        }
	}
}
