using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UISysFontInput))]
public class IFMakeVisibleAboveKeyboard : MonoBehaviour
{
	public Transform container;
	public float adjustmentPadding = 10f;
	private UISysFontInput input;
	private Transform cachedTransform;
	
	private Vector3 originalPosition;
	private bool wasAdjusted = false;
	
	void Start()
	{
		input = GetComponent<UISysFontInput>();
		cachedTransform = GetComponent<Transform>();
		if(container == null) {
			container = UIPanel.Find(cachedTransform).cachedTransform;
		}
		originalPosition = container.localPosition;
	}
	
	Vector3 GetWorldKeyboardTop()
	{
		if(Application.platform == RuntimePlatform.IPhonePlayer) {
			Debug.Log("KB height: "+TouchScreenKeyboard.area.height+", KB width:"+TouchScreenKeyboard.area.width);
			Vector3 screenTop = new Vector3(TouchScreenKeyboard.area.xMin, TouchScreenKeyboard.area.yMin, 0f);
			return UICamera.currentCamera.ScreenToWorldPoint(screenTop);
		}
		return Vector3.zero;
	}
	
	void OnSelect(bool isSelected)
	{
		if(enabled && input != null && input.enabled && NGUITools.GetActive(gameObject)) {
			if(isSelected) {
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(cachedTransform);
				Vector3 localControlBottom = new Vector3(bounds.center.x, bounds.min.y, 0f);
				Vector3 controlBottom = cachedTransform.TransformPoint(localControlBottom);
				Vector3 keyboardTop = GetWorldKeyboardTop();
				if(keyboardTop.y > controlBottom.y) {
					Vector3 worldDirection = new Vector3(0f, keyboardTop.y - controlBottom.y, 0f);
					Vector3 worldEnd = container.position + worldDirection;
					Vector3 end = container.InverseTransformPoint(worldEnd);
					end.y += adjustmentPadding;
					TweenPosition.Begin(container.gameObject, .3f, end);
					wasAdjusted = true;
				}
			} else {
				if(wasAdjusted) {
					wasAdjusted = false;
					TweenPosition.Begin(container.gameObject, .3f, originalPosition);
				}
			}
		}
	}

}
