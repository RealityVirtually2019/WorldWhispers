using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Provides a minimal data provider that allows connection to a virtual device, and logs messages when provider
	/// methods are called. Never generates data frames.
	/// </summary>
	[Serializable]
	public sealed class WearableDebugProvider : WearableProviderBase
	{
		public string Name
		{
			get { return _name; }
			set {_name = value; }
		}

		public int RSSI
		{
			get { return _rssi; }
			set { _rssi = value; }
		}

		public string UID
		{
			get { return _uid; }
			set { _uid = value; }
		}

		public ProductId ProductId
		{
			get { return _productId; }
			set { _productId = value; }
		}

		public VariantId VariantId
		{
			get { return _variantId; }
			set { _variantId = value; }
		}

		public bool Verbose
		{
			get { return _verbose; }
			set { _verbose = value; }
		}

		#region Provider Unique

		public void SimulateDisconnect()
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderSimulateDisconnect);
			}

			DisconnectFromDevice();
		}

		#endregion

		#region WearableProvider Implementation

		internal override void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderSearchingForDevices);
			}

			UpdateVirtualDeviceInfo();

			if (onDevicesUpdated != null)
			{
				onDevicesUpdated.Invoke(new []{_virtualDevice});
			}
		}

		internal override void StopSearchingForDevices()
		{
			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderStoppedSearching);
			}
		}

		internal override void ConnectToDevice(Device device, Action onSuccess, Action onFailure)
		{
			DisconnectFromDevice();

			UpdateVirtualDeviceInfo();

			if (device != _virtualDevice)
			{
				Debug.LogWarning(WearableConstants.DebugProviderInvalidConnectionWarning);
				return;
			}

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderConnectingToDevice);
			}

			OnDeviceConnecting(_virtualDevice);

			_virtualDevice.isConnected = true;
			_connectedDevice = _virtualDevice;

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderConnectedToDevice);
			}

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

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDisconnectedToDevice);
			}

			OnDeviceDisconnected(_connectedDevice.Value);

			_virtualDevice.isConnected = false;
			_connectedDevice = null;
		}

		internal override SensorUpdateInterval GetSensorUpdateInterval()
		{
			return _sensorUpdateInterval;
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

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderStartSensor, Enum.GetName(typeof(SensorId), sensorId));
			}

			_sensorStatus[sensorId] = true;
		}

		internal override void StopSensor(SensorId sensorId)
		{
			if (!_sensorStatus[sensorId])
			{
				return;
			}

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderStopSensor, Enum.GetName(typeof(SensorId), sensorId));
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

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderEnableGesture, Enum.GetName(typeof(GestureId), gestureId));
			}

			_gestureStatus[gestureId] = true;
		}

		internal override void DisableGesture(GestureId gestureId)
		{
			if (!_gestureStatus[gestureId])
			{
				return;
			}

			if (_verbose)
			{
				Debug.LogFormat(WearableConstants.DebugProviderDisableGesture, Enum.GetName(typeof(GestureId), gestureId));
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

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderInit);
			}
		}

		internal override void OnDestroyProvider()
		{
			base.OnDestroyProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDestroy);
			}
		}

		internal override void OnEnableProvider()
		{
			base.OnEnableProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderEnable);
			}
		}

		internal override void OnDisableProvider()
		{
			base.OnDisableProvider();

			if (_verbose)
			{
				Debug.Log(WearableConstants.DebugProviderDisable);
			}
		}

		internal override void OnUpdate()
		{
			UpdateVirtualDeviceInfo();
		}

		#endregion

		#region Private

		[SerializeField]
		private string _name;

		[SerializeField]
		private int _rssi;

		[SerializeField]
		private ProductId _productId;

		[SerializeField]
		private VariantId _variantId;

		[SerializeField]
		private string _uid;

		[SerializeField]
		private bool _verbose;

		private Dictionary<SensorId, bool> _sensorStatus;
		private SensorUpdateInterval _sensorUpdateInterval;

		private Dictionary<GestureId, bool> _gestureStatus;

		private Device _virtualDevice;

		internal WearableDebugProvider()
		{
			_virtualDevice = new Device
			{
				name = _name,
				rssi = _rssi,
				uid = _uid,
				productId = _productId,
				variantId = _variantId
			};

			_name = WearableConstants.DebugProviderDefaultDeviceName;
			_rssi = WearableConstants.DebugProviderDefaultRSSI;
			_uid = WearableConstants.DebugProviderDefaultUID;
			_productId = WearableConstants.DebugProviderDefaultProductId;
			_variantId = WearableConstants.DebugProviderDefaultVariantId;

			_verbose = true;

			_sensorStatus = new Dictionary<SensorId, bool>();
			_sensorUpdateInterval = WearableConstants.DefaultUpdateInterval;

			_sensorStatus.Add(SensorId.Accelerometer, false);
			_sensorStatus.Add(SensorId.Gyroscope, false);
			_sensorStatus.Add(SensorId.Rotation, false);
			_sensorStatus.Add(SensorId.GameRotation, false);

			// All gestures start disabled.
			_gestureStatus = new Dictionary<GestureId, bool>();
			GestureId[] gestures = WearableConstants.GestureIds;
			for (int i = 0; i < gestures.Length; ++i)
				if (gestures[i] != GestureId.None)
					_gestureStatus.Add(gestures[i], false);
		}

		private void UpdateVirtualDeviceInfo()
		{
			_virtualDevice.name = _name;
			_virtualDevice.rssi = _rssi;
			_virtualDevice.uid = _uid;
			_virtualDevice.productId = _productId;
			_virtualDevice.variantId = _variantId;
		}

		#endregion
	}
}
