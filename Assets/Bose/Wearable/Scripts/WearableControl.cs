using System;
using System.Collections.Generic;
using Bose.Wearable.Proxy;
using UnityEngine;

namespace Bose.Wearable
{
	public sealed class WearableControl : Singleton<WearableControl>
	{
		/// <summary>
		/// Represents a sensor available to the WearablePlugin.
		/// </summary>
		public sealed class WearableSensor
		{
			/// <summary>
			/// Returns true or false depending on whether or not the sensor is enabled and
			/// retrieving updates.
			/// </summary>
			public bool IsActive
			{
				get { return _wearableControl.GetSensorActive(_id); }
			}

			private readonly WearableControl _wearableControl;
			private readonly SensorId _id;

			internal WearableSensor(WearableControl wearableControl, SensorId id)
			{
				_wearableControl = wearableControl;
				_id = id;
			}

			public void Start()
			{
				_wearableControl.StartSensor(_id);
			}

			public void Stop()
			{
				_wearableControl.StopSensor(_id);
			}
		}

		/// <summary>
		/// Represents a Gesture available to the WearablePlugin.
		/// </summary>
		public sealed class WearableGesture
		{
			/// <summary>
			/// Returns true or false depending on whether or not the Gesture is enabled and
			/// retrieving updates.
			/// </summary>
			public bool IsActive
			{
				get { return _wearableControl.GetGestureEnabled(_gestureId); }
			}

			private readonly WearableControl _wearableControl;
			private readonly GestureId _gestureId;

			internal WearableGesture(WearableControl wearableControl, GestureId gestureId)
			{
				_wearableControl = wearableControl;
				_gestureId = gestureId;
			}

			public void Enable()
			{
				_wearableControl.EnableGesture(_gestureId);
			}

			public void Disable()
			{
				_wearableControl.DisableGesture(_gestureId);
			}
		}

		#region Public API

		/// <summary>
		/// Invoked when an attempt is made to connect to a device
		/// </summary>
		public event Action<Device> DeviceConnecting;

		/// <summary>
		/// Invoked when a device has been successfully connected.
		/// </summary>
		public event Action<Device> DeviceConnected;

		/// <summary>
		/// Invoked when a device has disconnected.
		/// </summary>
		public event Action<Device> DeviceDisconnected;

		/// <summary>
		/// Invoked when there are sensor updates from the Wearable device.
		/// </summary>
		public event Action<SensorFrame> SensorsUpdated;

		/// <summary>
		/// Invoked when a sensor frame includes a gesture.
		/// </summary>
		public event Action<GestureId> GestureDetected;

		/// <summary>
		/// Invoked when a double-tap gesture has completed
		/// </summary>
		public event Action DoubleTapDetected;

		/// <summary>
		/// The last reported value for the sensor.
		/// </summary>
		public SensorFrame LastSensorFrame
		{
			get { return _activeProvider.LastSensorFrame; }
		}

		/// <summary>
		/// An list of SensorFrames returned from the plugin bridge in order from oldest to most recent.
		/// </summary>
		public List<SensorFrame> CurrentSensorFrames
		{
			get { return _activeProvider.CurrentSensorFrames; }
		}

		/// <summary>
		/// The Accelerometer sensor on the Wearable device.
		/// </summary>
		public WearableSensor AccelerometerSensor
		{
			get { return _accelerometerSensor; }
		}

		private WearableSensor _accelerometerSensor;

		/// <summary>
		/// The Gyroscope sensor on the Wearable device.
		/// </summary>
		public WearableSensor GyroscopeSensor
		{
			get { return _gyroscopeSensor; }
		}

		private WearableSensor _gyroscopeSensor;

		/// <summary>
		/// The rotation sensor on the Wearable device.
		/// </summary>
		public WearableSensor RotationSensor
		{
			get { return _rotationSensor; }
		}

		private WearableSensor _rotationSensor;

		/// <summary>
		/// The game rotation sensor on the Wearable device.
		/// </summary>
		public WearableSensor GameRotationSensor
		{
			get { return _gameRotationSensor; }
		}

		private WearableSensor _gameRotationSensor;


		/// <summary>
		/// Get object for double-tap gesture.
		/// </summary>
		public WearableGesture DoubleTapGesture
		{
			get { return _wearableGestures[GestureId.DoubleTap]; }
		}

		/// <summary>
		/// Returns a <see cref="WearableGesture"/> based on the passed <see cref="GestureId"/>.
		/// <paramref name="gestureId"/>
		/// </summary>
		/// <param name="gestureId"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public WearableGesture GetWearableGestureById(GestureId gestureId)
		{
			if (gestureId == GestureId.None)
			{
				throw new Exception(WearableConstants.GestureIdNoneInvalidError);
			}

			WearableGesture wearableGesture;
			if (_wearableGestures.TryGetValue(gestureId, out wearableGesture))
			{
				return wearableGesture;
			}

			throw new Exception(string.Format(WearableConstants.WearableGestureNotYetSupported, gestureId));
		}

		private Dictionary<GestureId, WearableGesture> _wearableGestures;

		/// <summary>
		/// The Wearable device that is currently connected in Unity.
		/// </summary>
		public Device? ConnectedDevice
		{
			get
			{
				// Safeguard against uninitialized active provider
				return _activeProvider == null ? null : _activeProvider.ConnectedDevice;
			}
		}

		/// <summary>
		/// Searches for all Wearable devices that can be connected to.
		/// </summary>
		/// <param name="onDevicesUpdated"></param>
		public void SearchForDevices(Action<Device[]> onDevicesUpdated)
		{
			_activeProvider.SearchForDevices(onDevicesUpdated);
		}

		/// <summary>
		/// Stops searching for Wearable devices that can be connected to.
		/// </summary>
		public void StopSearchingForDevices()
		{
			_activeProvider.StopSearchingForDevices();
		}

		/// <summary>
		/// Connects to a specified device and invokes either <paramref name="onSuccess"/> or <paramref name="onFailure"/>
		/// depending on the result.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="onSuccess"></param>
		/// <param name="onFailure"></param>
		public void ConnectToDevice(Device device, Action onSuccess = null, Action onFailure = null)
		{
			_activeProvider.ConnectToDevice(device, onSuccess, onFailure);
		}

		/// <summary>
		/// Stops all attempts to connect to or monitor a device and disconnects from a device if connected.
		/// </summary>
		public void DisconnectFromDevice()
		{
			_activeProvider.DisconnectFromDevice();
		}

		/// <summary>
		/// The update interval of all sensors on the active provider
		/// </summary>
		public SensorUpdateInterval UpdateInterval
		{
			get { return _activeProvider.GetSensorUpdateInterval(); }
		}

		/// <summary>
		/// Sets the update interval for all sensors
		/// </summary>
		public void SetSensorUpdateInterval(SensorUpdateInterval interval)
		{
			_activeProvider.SetSensorUpdateInterval(interval);
		}

		/// <summary>
		/// Set the update mode, determining when SensorFrame updates are polled and made available.
		/// </summary>
		/// <param name="unityUpdateMode"></param>
		public void SetUnityUpdateMode(UnityUpdateMode unityUpdateMode)
		{
			_updateMode = unityUpdateMode;
		}

		/// <summary>
		/// The Unity Update method sensor updates should be retrieved and dispatched on.
		/// </summary>
		public UnityUpdateMode UpdateMode
		{
			get { return _updateMode; }
		}

		[SerializeField]
		private UnityUpdateMode _updateMode;

		/// <summary>
		/// An instance of the currently-active provider for configuration
		/// </summary>
		public WearableProviderBase ActiveProvider
		{
			get { return _activeProvider; }
		}

		private WearableProviderBase _activeProvider;

		/// <summary>
		/// Set the active provider to a specific provider instance
		/// </summary>
		/// <param name="provider"></param>
		public void SetActiveProvider(WearableProviderBase provider)
		{
			// Uninitialized providers should never have OnEnable/Disable called
			if (_activeProvider != null)
			{
				if (_activeProvider.Initialized)
				{
					_activeProvider.OnDisableProvider();
				}

				// Unsubscribe after disabling in case OnDisableProvider invokes an event
				// Using an invocation method here rather than the event proper ensures that any events added or removed
				// after setting the provider will be accounted for.
				_activeProvider.DeviceConnecting -= OnDeviceConnecting;
				_activeProvider.DeviceConnected -= OnDeviceConnected;
				_activeProvider.DeviceDisconnected -= OnDeviceDisconnected;
				_activeProvider.SensorsUpdated -= OnSensorsUpdated;
			}

			_activeProvider = provider;

			// Initialize if this is the first time the provider is active
			if (!_activeProvider.Initialized)
			{
				_activeProvider.OnInitializeProvider();
			}

			// Subscribe to the provider's events
			_activeProvider.DeviceConnecting += OnDeviceConnecting;
			_activeProvider.DeviceConnected += OnDeviceConnected;
			_activeProvider.DeviceDisconnected += OnDeviceDisconnected;
			_activeProvider.SensorsUpdated += OnSensorsUpdated;

			// Enable the new provider after subscribing in case enabling the provider invokes an event
			_activeProvider.OnEnableProvider();
		}

		/// <summary>
		/// Set the active provider by provider type
		/// </summary>
		public void SetActiveProvider<T>()
			where T : WearableProviderBase
		{
			SetActiveProvider(GetOrCreateProvider<T>());
		}

		/// <summary>
		///  Returns a provider of the specified provider type for manipulation
		/// </summary>
		public T GetOrCreateProvider<T>()
			where T : WearableProviderBase
		{
			if (_debugProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.DebugProvider);
			}
			else if (_deviceProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.WearableDevice);
			}
			else if (_mobileProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.MobileProvider);
			}
			else if (_proxyProvider is T)
			{
				return (T)GetOrCreateProvider(ProviderId.WearableProxy);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region Private

		[SerializeField]
		private WearableDebugProvider _debugProvider;

		[SerializeField]
		private WearableDeviceProvider _deviceProvider;

		[SerializeField]
		private WearableMobileProvider _mobileProvider;

		[SerializeField]
		private WearableProxyProvider _proxyProvider;

		#pragma warning disable CS0414
		[SerializeField]
		private ProviderId _editorDefaultProvider = ProviderId.MobileProvider;

		[SerializeField]
		private ProviderId _runtimeDefaultProvider = ProviderId.WearableDevice;
		#pragma warning restore CS0414

		private WearableProviderBase GetOrCreateProvider(ProviderId providerId)
		{
			switch (providerId)
			{
				case ProviderId.DebugProvider:
					if (_debugProvider == null)
					{
						_debugProvider = new WearableDebugProvider();
					}
					return _debugProvider;

				case ProviderId.WearableDevice:
					if (_deviceProvider == null)
					{
						_deviceProvider = new WearableDeviceProvider();
					}
					return _deviceProvider;

				case ProviderId.MobileProvider:
					if (_mobileProvider == null)
					{
						_mobileProvider = new WearableMobileProvider();
					}
					return _mobileProvider;

				case ProviderId.WearableProxy:
					if (_proxyProvider == null)
					{
						_proxyProvider = new WearableProxyProvider();
					}

					return _proxyProvider;

				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Invokes the <see cref="DeviceConnecting"/> event.
		/// </summary>
		private void OnDeviceConnecting(Device device)
		{
			if (DeviceConnecting != null)
			{
				DeviceConnecting.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="DeviceConnected"/> event.
		/// </summary>
		/// <param name="device"></param>
		private void OnDeviceConnected(Device device)
		{
			if (DeviceConnected != null)
			{
				DeviceConnected.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="DeviceDisconnected"/> event.
		/// </summary>
		/// <param name="device"></param>
		private void OnDeviceDisconnected(Device device)
		{
			if (DeviceDisconnected != null)
			{
				DeviceDisconnected.Invoke(device);
			}
		}

		/// <summary>
		/// Invokes the <see cref="SensorsUpdated"/> event.
		/// If the frame contains a gesture, also invokes the <see cref="GestureDetected"/> event.
		/// </summary>
		/// <param name="frame"></param>
		private void OnSensorsUpdated(SensorFrame frame)
		{
			if (SensorsUpdated != null)
			{
				SensorsUpdated.Invoke(frame);
			}

			if (frame.gestureId != GestureId.None)
			{
				if (GestureDetected != null)
				{
					GestureDetected.Invoke(frame.gestureId);
				}

				switch (frame.gestureId)
				{
					case GestureId.DoubleTap:
						if (DoubleTapDetected != null)
						{
							DoubleTapDetected.Invoke();
						}
						break;
					case GestureId.None:
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Start a sensor with a given interval <see cref="SensorId"/>. Providers should override this method.
		/// </summary>
		/// <param name="sensorId"></param>
		private void StartSensor(SensorId sensorId)
		{
			_activeProvider.StartSensor(sensorId);
		}

		/// <summary>
		/// Stop a sensor with a given <see cref="SensorId"/>. Providers should override this method.
		/// </summary>
		/// <param name="sensorId"></param>
		private void StopSensor(SensorId sensorId)
		{
			_activeProvider.StopSensor(sensorId);
		}

		/// <summary>
		/// Returns whether or not a sensor with a given <see cref="SensorId"/> is active or not.
		/// Providers should override this method.
		/// </summary>
		/// <param name="sensorId"></param>
		/// <returns></returns>
		private bool GetSensorActive(SensorId sensorId)
		{
			return _activeProvider.GetSensorActive(sensorId);
		}

		/// <summary>
		/// Start a gesture with a given interval <see cref="GestureId"/>. Providers should override this method.
		/// </summary>
		/// <param name="gestureId"></param>
		private void EnableGesture(GestureId gestureId)
		{
			_activeProvider.EnableGesture(gestureId);
		}

		/// <summary>
		/// Stop a gesture with a given <see cref="GestureId"/>. Providers should override this method.
		/// </summary>
		/// <param name="GestureId"></param>
		private void DisableGesture(GestureId gestureId)
		{
			_activeProvider.DisableGesture(gestureId);
		}

		/// <summary>
		/// Returns whether or not a gesture with a given <see cref="GestureId"/> is enabled.
		/// Providers should override this method.
		/// </summary>
		/// <param name="GestureId"></param>
		/// <returns></returns>
		private bool GetGestureEnabled(GestureId gestureId)
		{
			return _activeProvider.GetGestureEnabled(gestureId);
		}

		protected override void Awake()
		{
			_accelerometerSensor = new WearableSensor(this, SensorId.Accelerometer);
			_gyroscopeSensor = new WearableSensor(this, SensorId.Gyroscope);
			_rotationSensor = new WearableSensor(this, SensorId.Rotation);
			_gameRotationSensor = new WearableSensor(this, SensorId.GameRotation);

			// populate wearable gesture dictionary
			_wearableGestures = new Dictionary<GestureId, WearableGesture>();
			GestureId[] gestures = WearableConstants.GestureIds;
			for (int i = 0; i < gestures.Length; ++i)
			{
				if (gestures[i] != GestureId.None)
				{
					_wearableGestures[gestures[i]] = new WearableGesture(this, gestures[i]);
				}
			}

			// Activate the default provider depending on the platform
			#if UNITY_EDITOR
			SetActiveProvider(GetOrCreateProvider(_editorDefaultProvider));
			#else
			SetActiveProvider(GetOrCreateProvider(_runtimeDefaultProvider));
			#endif

			base.Awake();
		}

		private void OnValidate()
		{
			// Set using the variable not the method, so the provider doesn't get prematurely initialized
			#if UNITY_EDITOR
			_activeProvider = GetOrCreateProvider(_editorDefaultProvider);
			#else
			_activeProvider = GetOrCreateProvider(_runtimeDefaultProvider);
			#endif
		}

		/// <summary>
		/// When destroyed, stop all sensors and disconnect from the Wearable device.
		/// </summary>
		protected override void OnDestroy()
		{
			_accelerometerSensor.Stop();
			_gyroscopeSensor.Stop();
			_rotationSensor.Stop();
			_gameRotationSensor.Stop();

			DisconnectFromDevice();

			// Clean up providers
			_activeProvider.OnDisableProvider();

			if (_deviceProvider != null && _deviceProvider.Initialized)
			{
				_deviceProvider.OnDestroyProvider();
			}

			if (_debugProvider != null && _debugProvider.Initialized)
			{
				_debugProvider.OnDestroyProvider();
			}

			base.OnDestroy();
		}

		/// <summary>
		/// When enabled, resume monitoring the device session if necessary.
		/// </summary>
		private void OnEnable()
		{
			if (!_activeProvider.Enabled)
			{
				_activeProvider.OnEnableProvider();
			}
		}

		/// <summary>
		/// When disabled, stop actively searching for devices.
		/// </summary>
		private void OnDisable()
		{
			_activeProvider.OnDisableProvider();
		}

		private void Update()
		{
			if (UpdateMode != UnityUpdateMode.Update)
			{
				return;
			}

			_activeProvider.OnUpdate();
		}

		/// <summary>
		/// For each sensor, prompt them to get their buffer of updates from native code per fixed physics update step.
		/// </summary>
		private void FixedUpdate()
		{
			if (UpdateMode != UnityUpdateMode.FixedUpdate)
			{
				return;
			}

			_activeProvider.OnUpdate();
		}

		/// <summary>
		/// For each sensor, prompt them to get their buffer of updates from native code per late update step.
		/// </summary>
		private void LateUpdate()
		{
			if (UpdateMode != UnityUpdateMode.LateUpdate)
			{
				return;
			}

			_activeProvider.OnUpdate();
		}

		#endregion
	}
}
