// Copyright (c) 2013 Empirical Development LLC. All rights reserved.

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu("IFES/Sprite Stretcher")]
public class IFSpriteStretcher : MonoBehaviour {
	
	// The width the sprite was designed for
	public float designWidth = 768f;
	public bool onlyScaleUp = true;
	public float scaleDownIfWidthLessThan = 480f;
	
	private UISprite sprite;
	private UIStretch stretch;
	private UIRoot uiRoot;
	private float screenWidth;
	private Camera cachedCamera;
	private float lastScreenWidth = float.MaxValue;
	
	void Start()
	{
		if(IFConstants.spriteDesignWidth > 0f) {
			designWidth = IFConstants.spriteDesignWidth;
		}
		sprite = GetComponent<UISprite>();
		stretch = GetComponent<UIStretch>();
		if(stretch == null) {
			stretch = gameObject.AddComponent<UIStretch>();
		}
		stretch.enabled = true;
		uiRoot = NGUITools.FindInParents<UIRoot>(gameObject);
		cachedCamera = NGUITools.FindCameraForLayer(gameObject.layer);
	}
	
//	void OnEnable()
//	{
//		if(stretch == null) {
//			stretch = gameObject.GetComponent<UIStretch>();
//			if(stretch == null) {
//				stretch = gameObject.AddComponent<UIStretch>();
//			}
//		}
//		if(sprite == null) sprite = GetComponent<UISprite>();
//		if(cachedCamera == null) cachedCamera = NGUITools.FindCameraForLayer(gameObject.layer);
//		if(uiRoot == null) uiRoot = NGUITools.FindInParents<UIRoot>(gameObject);
//		screenWidth = cachedCamera.pixelWidth * uiRoot.pixelSizeAdjustment;
//		UpdateStretch();
//	}
//	
//	void OnDisable()
//	{
//		if(stretch != null) {
//			stretch.enabled = false;
//		}
//	}
	
	void Update()
	{
		screenWidth = cachedCamera.pixelWidth * uiRoot.pixelSizeAdjustment;
		if(screenWidth != lastScreenWidth) {
			lastScreenWidth = screenWidth;
			UpdateStretch();
			sprite.MarkAsChanged();
		}
	}
	
	void UpdateStretch()
	{
		if(sprite.type == UISprite.Type.Simple) {
			// This calculation only makes sense on simple (aka non-stretchable) sprites.
			if(onlyScaleUp && screenWidth <= designWidth && screenWidth >= scaleDownIfWidthLessThan) {
//				Debug.Log("Making "+ sprite.name +" ("+ sprite.spriteName +") pixel perfect for screen width: "+screenWidth);
				stretch.enabled = false;
				sprite.MakePixelPerfect();
			} else {
				stretch.enabled = true;
//				Debug.Log("Resizing "+ sprite.name +" ("+ sprite.spriteName +") for screen width: "+screenWidth);
				float widthScaleFactor = screenWidth / designWidth;
				Vector2 relativeSize = Vector2.one;
				Vector2 perfectSize = CalculatePixelPerfectSize();

				relativeSize.x = (widthScaleFactor * perfectSize.x) / uiRoot.activeHeight;
				relativeSize.y = (widthScaleFactor * perfectSize.y) / uiRoot.activeHeight;
				stretch.style = UIStretch.Style.BasedOnHeight;
				stretch.relativeSize = relativeSize;
			}
		}
	}
	
	Vector2 CalculatePixelPerfectSize()
	{
		Vector2 size = Vector2.one;
		Texture tex = sprite.mainTexture;
		Vector3 scale = sprite.cachedTransform.localScale;

		if(tex != null) {
			Rect rect = NGUIMath.ConvertToPixels(sprite.outerUV, tex.width, tex.height, true);
			float pixelSize = sprite.atlas.pixelSize;
			scale.x = Mathf.RoundToInt(rect.width * pixelSize) * Mathf.Sign(scale.x);
			scale.y = Mathf.RoundToInt(rect.height * pixelSize) * Mathf.Sign(scale.y);
			scale.z = 1f;
			size.x = scale.x;
			size.y = scale.y;
		}
		return size;
	}
}
