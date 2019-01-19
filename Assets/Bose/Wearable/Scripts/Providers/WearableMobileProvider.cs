using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// A provider that simulates a Wearable Device using the IMU built into a mobile device. In addition to on-device
	/// builds, this also works in the editor via the Unity Remote app for easier prototyping. All coördinates are
	/// relative to the same frame and have the same units as the Wearable Device.
	/// </summary>
	[Serializable]
	public sealed class WearableMobileProvider : WearableProviderBase
	{
		#region Provider Unique

		public void SimulateDisconnect()
		{
			DisconnectFromDevice();
		}

		#endregion

		#region WearableProvider Implementation

		internal override void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			if (onDevicesUpdated != null)
			{
				onDevicesUpdated.Invoke(new []{_virtualDevice});
			}
		}

		internal override void StopSearchingForDevices()
		{

		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			DisconnectFromDevice();

			if (device != _virtualDevice)
			{
				Debug.LogWarning(WearableConstants.DebugProviderInvalidConnectionWarning);
				return;
			}


			OnDeviceConnecting(_virtualDevice);

			_virtualDevice.isConnected = true;
			_connectedDevice = _virtualDevice;

			if (onSuccess != null)
			{
				onSuccess.Invoke();
			}

			OnDeviceConnected(_virtualDevice);
		}

		internal override void DisconnectFromDevice()
		{
			if (_connectedDevice == null)
			{
				return;
			}

			_virtualDevice.isConnected = false;
			OnDeviceDisconnected(_connectedDevice.Value);

			_connectedDevice = null;
		}

		internal override void SetSensorUpdateInterval(SensorUpdateInterval updateInterval)
		{
			if (_connectedDevice == null)
			{
				Debug.LogWarning(WearableConstants.SetUpdateRateWithoutDeviceWarning);
				return;
			}

			_sensorUpdateInterval = updateInterval;
		}

		internal override SensorUpdateInterval GetSensorUpdateInterval()
		{
			return _sensorUpdateInterval;
		}

		internal override void StartSensor(SensorId sensorId)
		{
			if (_connectedDevice == null)
			{
				_sensorStatus[sensorId] = false;
				Debug.LogWarning(WearableConstants.StartSensorWithoutDeviceWarning);
				return;
			}

			if (_sensorStatus[sensorId])
			{
				return;
			}

			_sensorStatus[sensorId] = true;
			_nextSensorUpdateTime = Time.unscaledTime;
		}

		internal override void StopSensor(SensorId sensorId)
		{
			if (!_sensorStatus[sensorId])
			{
				return;
			}

			_sensorStatus[sensorId] = false;
		}

		internal override bool GetSensorActive(SensorId sensorId)
		{
			return (_connectedDevice != null) && _sensorStatus[sensorId];
		}

		internal override void EnableGesture(GestureId gestureId)
		{
			if (_connectedDevice == null)
			{
				_gestureStatus[gestureId] = false;
				Debug.LogWarning(WearableConstants.EnableGestureWithoutDeviceWarning);
				return;
			}

			if (_gestureStatus[gestureId])
			{
				return;
			}

			_gestureStatus[gestureId] = true;
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			if (!_gestureStatus[gestureId])
			{
				return;
			}

			_gestureStatus[gestureId] = false;
		}

		internal override bool GetGestureEnabled(GestureId gestureId)
		{
			return (_connectedDevice != null) && _gestureStatus[gestureId];
		}

		internal override void OnInitializeProvider()
		{
			base.OnInitializeProvider();

			// Must be done here not in the constructor to avoid a serialization error.
			_gyro = Input.gyro;
		}

		internal override void OnEnableProvider()
		{
			base.OnEnableProvider();

			_wasGyroEnabled = _gyro.enabled;
			_gyro.enabled = true;
			_lastSensorFrame.timestamp = Time.unscaledTime;
		}

		internal override void OnDisableProvider()
		{
			base.OnDisableProvider();

			_gyro.enabled = _wasGyroEnabled;
		}

		internal override void OnUpdate()
		{
			if (!_enabled)
			{
				return;
			}

			if (Time.unscaledTime >= _nextSensorUpdateTime)
			{
				_nextSensorUpdateTime += WearableTools.SensorUpdateIntervalToSeconds(_sensorUpdateInterval);

				// Update all active sensors
				if (_sensorStatus[SensorId.Accelerometer])
				{
					UpdateAccelerometerData();
				}

				if (_sensorStatus[SensorId.Gyroscope])
				{
					UpdateGyroscopeData();
				}

				if (_sensorStatus[SensorId.Rotation] || _sensorStatus[SensorId.GameRotation])
				{
					UpdateRotationSensorData();
				}

				UpdateGestureData();

				// Update the timestamp and delta-time, then emit the frame
				_lastSensorFrame.deltaTime = Time.unscaledTime - _lastSensorFrame.timestamp;
				_lastSensorFrame.timestamp = Time.unscaledTime;

				_currentSensorFrames.Clear();
				_currentSensorFrames.Add(_lastSensorFrame);
				OnSensorsUpdated(_lastSensorFrame);
			}
			else
			{
				// Otherwise, the list should be empty. _lastSensorFrame will retain its previous value.
				if (_currentSensorFrames.Count > 0)
				{
					_currentSensorFrames.Clear();
				}
			}
		}

		#endregion

		#region Private

		private Gyroscope _gyro;
		private bool _wasGyroEnabled;

		private Dictionary<SensorId, bool> _sensorStatus;
		private SensorUpdateInterval _sensorUpdateInterval;
		private float _nextSensorUpdateTime;

		// Gestures
		private Dictionary<GestureId, bool> _gestureStatus;

		private Device _virtualDevice;

		internal WearableMobileProvider()
		{
			_virtualDevice = new Device
			{
				isConnected = false,
				name = WearableConstants.MobileProviderDeviceName,
				productId = WearableConstants.MobileProviderProductId,
				variantId = WearableConstants.MobileProviderVariantId,
				rssi = 0,
				uid = WearableConstants.EmptyUID
			};

			_sensorStatus = new Dictionary<SensorId, bool>();
			_sensorUpdateInterval = WearableConstants.DefaultUpdateInterval;
			_nextSensorUpdateTime = 0.0f;

			_sensorStatus.Add(SensorId.Accelerometer, false);
			_sensorStatus.Add(SensorId.Gyroscope, false);
			_sensorStatus.Add(SensorId.Rotation, false);
			_sensorStatus.Add(SensorId.GameRotation, false);

			// All gestures start disabled.
			_gestureStatus = new Dictionary<GestureId, bool>();
			GestureId[] gestures = WearableConstants.GestureIds;
			for (int i = 0; i < gestures.Length; ++i)
			{
				if (gestures[i] != GestureId.None)
				{
					_gestureStatus.Add(gestures[i], false);
				}
			}
		}

		/// <summary>
		/// Copy over the mobile device's acceleration to the cached sensor frame, switching from right- to left-handed coördinates
		/// </summary>
		private void UpdateAccelerometerData()
		{
			Vector3 raw = Input.acceleration;
			_lastSensorFrame.acceleration.value.Set(-raw.x, -raw.y, raw.z);
			_lastSensorFrame.acceleration.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Copy over the mobile device's angular velocity to the cached sensor frame, switching from right- to left-handed coördinates
		/// </summary>
		private void UpdateGyroscopeData()
		{
			Vector3 raw = _gyro.rotationRate;
			_lastSensorFrame.angularVelocity.value.Set(-raw.x, -raw.y, raw.z);
			_lastSensorFrame.angularVelocity.accuracy = SensorAccuracy.High;
		}

		/// <summary>
		/// Copy over the mobile device's orientation data to the cached sensor frame, changing frames of reference as needed.
		/// Game rotation is set to the same value as rotation.
		/// </summary>
		private void UpdateRotationSensorData()
		{
			// This is based on an iPhone 6, but should be cross-compatible with other devices.
			Quaternion raw = _gyro.attitude;
			const float InverseRootTwo = 0.7071067812f; // 1 / sqrt(2)
			_lastSensorFrame.rotation.value = new Quaternion(
				InverseRootTwo * (raw.w - raw.x),
				InverseRootTwo * -(raw.y + raw.z),
				InverseRootTwo * (raw.z - raw.y),
				InverseRootTwo * (raw.w + raw.x)
			);
			_lastSensorFrame.rotation.measurementUncertainty = 0.0f;

			_lastSensorFrame.gameRotation = _lastSensorFrame.rotation;
		}


		/// <summary>
		/// Copy any enabled gestures to the cached sensor frame. 
		/// </summary>
		private void UpdateGestureData()
		{
			// NOTE: Gestures are not currently implemented within the WearableMobileProvider.
			_lastSensorFrame.gestureId = GestureId.None;
		}

		#endregion
	}
}
