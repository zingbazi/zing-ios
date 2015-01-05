using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UISlider))]
public class IFToggleSwitch : MonoBehaviour
{

	public float onValue = .77f;
	public float offValue = .22f;
	public float animationDuration = .25f;
	public GameObject eventReceiver;
	public string functionName = "SwitchToggled";

	private bool isOn;
	public bool On
	{
		get
		{
			return isOn;
		}
		set
		{
			if(isOn != value) {
				isOn = value;
				UpdateSlider(false);
			}
		}
	}

	public void SetOn(bool newValue, bool animated)
	{
		if(isOn != newValue) {
			isOn = newValue;
			UpdateSlider(animated);
		}
	}

	private UISlider slider;

	void Awake()
	{
		slider = GetComponent<UISlider>();
		slider.onValueChange += SliderValueChanged;
	}

	void Start()
	{
		if(slider.thumb != null) {
			UIEventListener thumbListener = UIEventListener.Get(slider.thumb.gameObject);
			thumbListener.onPress += SwitchPressed;

			UIEventListener switchListener = UIEventListener.Get(gameObject);
			switchListener.onPress += SwitchPressed;
			NGUITools.AddWidgetCollider(slider.thumb.gameObject);
		}
		NGUITools.AddWidgetCollider(gameObject);
		UpdateSlider(false);
	}

	void SwitchPressed(GameObject sender, bool pressed)
	{
		UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
		if(pressed) {
			UpdateSlider(false);
			return;
		}
		if(UICamera.currentTouch.dragStarted) {
			float sliderValue = slider.sliderValue;
			if(sliderValue != onValue || sliderValue != offValue) {
				isOn = (sliderValue - offValue) > (onValue - sliderValue);
			}
		} else {
			slider.sliderValue = isOn ? onValue : offValue;
			isOn = !isOn;
		}
		UpdateSlider(true);
		if(eventReceiver != null && !string.IsNullOrEmpty(functionName) && Application.isPlaying) {
			eventReceiver.SendMessage(functionName, isOn, SendMessageOptions.DontRequireReceiver);
		}
	}

	void SliderValueChanged(float sliderValue)
	{
		slider.sliderValue = Mathf.Clamp(sliderValue, offValue, onValue);
	}

	void UpdateSlider(bool animated)
	{
		if(slider != null) {
			float toValue = isOn ? onValue : offValue;
			if(!animated || (Application.isEditor && !Application.isPlaying)) {
				slider.sliderValue = toValue;
				slider.ForceUpdate();
			} else {
				float fromValue = slider.sliderValue;
				float maxDistance = onValue - offValue;
				float actualDistance = Mathf.Abs(fromValue - toValue);
				float duration = animationDuration * (actualDistance / maxDistance);
				TweenFloat.Begin(gameObject, duration, fromValue, toValue, (current, isFinished) => {
					slider.sliderValue = current;
				});
			}
		}
	}
}
