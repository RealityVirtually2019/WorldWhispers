using System;
using UnityEngine;
using UnityEngine.Events;

namespace Bose.Wearable
{
	/// <summary>
	/// Automatically fires an event if the selected gesture is detected.
	/// </summary>
	[AddComponentMenu("Bose/Wearable/GestureDetector")]
	public class GestureDetector : MonoBehaviour
	{
		[SerializeField] 
		private GestureId _gesture;
		
		[SerializeField]
		private UnityEvent _onGestureDetected;

		private WearableControl _wearableControl;

		private void Awake()
		{
			_wearableControl = WearableControl.Instance;
			_wearableControl.DeviceConnected += OnDeviceConnected;
			_wearableControl.GestureDetected += GestureDetected;
		}

		private void OnDestroy()
		{
			_wearableControl.DeviceConnected -= OnDeviceConnected;
			_wearableControl.GestureDetected -= GestureDetected;
		}

		private void OnEnable()
		{
			EnableGesture();
		}

		private void OnDeviceConnected(Device device)
		{
			EnableGesture();
		}

		private void EnableGesture()
		{
			if (_wearableControl.ConnectedDevice == null)
			{
				return;
			}

			try
			{
				var gesture = _wearableControl.GetWearableGestureById(_gesture);
				gesture.Enable();
			}
			catch (Exception exception)
			{
				Debug.LogError(exception.Message, this);
			}
		}

		private void GestureDetected(GestureId gesture)
		{
			if (gesture != _gesture)
			{
				return;
			}

			_onGestureDetected.Invoke();
		}
	}
}
