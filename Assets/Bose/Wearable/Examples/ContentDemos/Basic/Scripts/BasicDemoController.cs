using UnityEngine;

namespace Bose.Wearable.Examples
{
	/// <summary>
	/// Main logic for the Basic Demo. Configures the <see cref="RotationMatcher"/> based on UI input, and
	/// fades out the reference glasses based on closeness.
	/// Handles starting and stopping sensors on connect/reconnect/disconnect.
	/// </summary>
	public class BasicDemoController : MonoBehaviour
	{
		/// <summary>
		/// The material applied to the reference glasses.
		/// </summary>
		[SerializeField]
		protected Material _frameFadeMaterial;

		private WearableControl _wearableControl;
		private RotationMatcher _matcher;

		private void Awake()
		{
			_matcher = GetComponent<RotationMatcher>();

			// Grab an instance of the WearableControl singleton. This is the primary access point to the wearable SDK.
			_wearableControl = WearableControl.Instance;

			// Subscribe to DeviceConnected to handle reconnects that happen during play.
			_wearableControl.DeviceConnected += OnDeviceConnected;
		}

		private void OnDestroy()
		{
			// Ensure that the controller is no longer subscribing to connections after it is destroyed.
			if (WearableControl.Instance != null)
			{
				WearableControl.Instance.DeviceConnected -= OnDeviceConnected;
			}
		}

		private void OnEnable()
		{
			// If a device is connected, immediately start the rotation sensor
			// This ensures that we will receive data from the sensor during play.
			StartSensors();
		}

		private void OnDisable()
		{
			// If a device is still connected, stop the rotation sensor when the controller is disabled.
			if (_wearableControl.ConnectedDevice != null)
			{
				_wearableControl.RotationSensor.Stop();
			}
		}

		private void OnDeviceConnected(Device obj)
		{
			// If a device is reconnected during play, ensures the rotation sensor is still running.
			StartSensors();
		}

		/// <summary>
		/// Sets rotation to relative mode using the current orientation.
		/// </summary>
		public void SetRelativeReference()
		{
			_matcher.SetRelativeReference(_wearableControl.LastSensorFrame.rotation);
		}

		/// <summary>
		/// Sets rotation to absolute mode.
		/// </summary>
		public void SetAbsoluteReference()
		{
			_matcher.SetAbsoluteReference();
		}

		/// <summary>
		/// Configures the update interval and sets all needed sensors
		/// </summary>
		private void StartSensors()
		{
			if (_wearableControl.ConnectedDevice != null)
			{
				_wearableControl.SetSensorUpdateInterval(SensorUpdateInterval.FortyMs);
				_wearableControl.RotationSensor.Start();
			}
		}

		private void Update()
		{
			// If no device is connected, skip this frame.
			if (_wearableControl.ConnectedDevice == null)
			{
				return;
			}

			// Get a frame of sensor data. Since no integration is being performed, we can safely ignore all
			// intermediate frames and just grab the most recent.
			SensorFrame frame = _wearableControl.LastSensorFrame;

			// Measure the similarity between the current rotation and the reference rotation from 0 to 1.
			// The absolute value is needed here because q and -q represent the same rotation.
			float similarity = Mathf.Abs(Quaternion.Dot(frame.rotation, _matcher.ReferenceRotation));

			// Scale the alpha along with similarity, so that the reference glasses begin fading out at 0.9 (~35º) and
			// finish fading out at 0.95 (~25º).
			float alpha = 0.25f * Mathf.Clamp01((0.95f - similarity) / 0.05f);
			_frameFadeMaterial.color = new Color(1.0f, 1.0f, 1.0f, alpha);
		}
	}
}
