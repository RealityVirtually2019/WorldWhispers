using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bose.Wearable
{
	/// <summary>
	/// Shown when searching for devices
	/// </summary>
	public class SearchingWearableConnectDisplay : WearableConnectDisplayBase
	{
		/// <summary>
		/// The RectTransform that ConnectionDeviceDisplays will be instantiated underneath.
		/// </summary>
		[Header("UX Refs")]
		[SerializeField]
		private RectTransform _displayRootRectTransform;

		[SerializeField]
		private RectTransform _searchIconRectTransform;

		[Header("Animation")]
		[SerializeField]
		private float _spinnerRotationSpeed = 250f;

		/// <summary>
		/// The ConnectionDeviceDisplay prefab that will be used.
		/// </summary>
		[Header("Prefabs"), Space(5)]
		[SerializeField]
		private WearableDeviceDisplay _displayPrefab;

		[Header("Sound Clips"), Space(5)]
		[SerializeField]
		private AudioClip _sfxSearching;

		[SerializeField]
		private AudioClip _sfxFoundDevice;

		// Device Tracking & Button Pool
		private List<Device> _devicesCurrent;
		private List<Device> _devicesAddedOnDiscovery;
		private List<Device> _devicesRemovedOnDiscovery;

		private const int INIT_POOL_SIZE = 5;
		private List<WearableDeviceDisplay> _buttonPool;
		private List<WearableDeviceDisplay> _deviceButtons;

		// Audio
		private AudioSource _srcSearching;
		private const float TIME_BACKGROUND_FADE = 0.5f;

		private Coroutine _searchSpinner;

		protected override void Awake()
		{
			_devicesCurrent = new List<Device>();
			_devicesAddedOnDiscovery = new List<Device>();
			_devicesRemovedOnDiscovery = new List<Device>();

			SetupButtonPool();
			SetupAudio();

			_panel.DeviceSearching += OnDeviceSearching;
			_panel.DeviceConnecting += OnDeviceConnecting;
			_panel.DeviceConnectFailure += OnDeviceConnectEnd;
			_panel.DeviceConnectSuccess += OnDeviceConnectEnd;
			_panel.DevicesFound += OnDevicesFound;

			base.Awake();
		}

		protected override void SetupAudio()
		{
			base.SetupAudio();

			_srcSearching = _audioControl.GetSource(true);
		}

		private void OnDestroy()
		{
			_panel.DeviceSearching -= OnDeviceSearching;
			_panel.DeviceConnecting -= OnDeviceConnecting;
			_panel.DeviceConnectFailure -= OnDeviceConnectEnd;
			_panel.DeviceConnectSuccess -= OnDeviceConnectEnd;
			_panel.DevicesFound -= OnDevicesFound;

			TeardownButtonPool();
			TeardownAudio();
		}

		private void OnDisable()
		{
			TeardownButtonPool();
			TeardownAudio();
		}

		private void OnDeviceSearching()
		{
			Show();
		}

		private void OnDeviceConnecting()
		{
			Hide();
		}

		private void OnDeviceConnectEnd()
		{
			Hide();
		}

		protected override void Show()
		{
			_messageText.text = WearableConstants.DeviceConnectionSearchMessage;

			if (_searchSpinner == null)
			{
				_searchSpinner = StartCoroutine(AnimateIconSpinner());
			}

			PlayBackgroundAudio();

			base.Show();
		}

		private IEnumerator AnimateIconSpinner()
		{
			var waitForEndOfFrame = new WaitForEndOfFrame();
			while (true)
			{
				_searchIconRectTransform.Rotate(0f, 0f, -_spinnerRotationSpeed * Time.unscaledDeltaTime);
				yield return waitForEndOfFrame;
			}
		}

		protected override void Hide()
		{
			_devicesCurrent.Clear();

			if (_searchSpinner != null)
			{
				StopCoroutine(_searchSpinner);
				_searchSpinner = null;
			}

			ReclaimAllButtons();

			base.Hide();
		}

		private void OnDevicesFound(Device[] devices)
		{
			DetermineRemovedDevices(devices);
			DetermineAddedDevices(devices);
			ResolveDeviceChanges();

			if (_devicesAddedOnDiscovery.Count > 0)
			{
				PlayFoundSting();
				StopBackgroundAudio();
			}
			else if (_devicesCurrent.Count == 0)
			{
				PlayBackgroundAudio();
			}
		}

		/// <summary>
		/// Create a list of all devices that are currently being tracked that are no longer reported in the
		/// most recent list of discovered devices.
		/// </summary>
		private void DetermineRemovedDevices(Device[] devices)
		{
			_devicesRemovedOnDiscovery.Clear();

			for (int i = 0; i < _devicesCurrent.Count; ++i)
			{
				var device = _devicesCurrent[i];

				if (Array.IndexOf(devices, device) >= 0)
				{
					continue;
				}

				_devicesRemovedOnDiscovery.Add(device);
			}
		}

		/// <summary>
		/// Create a list of new devices that do not exist in our list of current devices.
		/// </summary>
		private void DetermineAddedDevices(Device[] devices)
		{
			_devicesAddedOnDiscovery.Clear();

			for (int i = 0; i < devices.Length; ++i)
			{
				var device = devices[i];

				if (_devicesCurrent.Contains(device))
				{
					continue;
				}

				_devicesAddedOnDiscovery.Add(device);
			}
		}

		/// <summary>
		/// Ensure that _devicesCurrent is up to date with only the known devices, and that the active buttons
		/// reflect that collection and its ordering.
		/// </summary>
		private void ResolveDeviceChanges()
		{
			// resolve the deltas to the current devices, and resort based on the current RSSI of the devices.
			_devicesCurrent.RemoveAll(device => _devicesRemovedOnDiscovery.Contains(device));
			_devicesCurrent.AddRange(_devicesAddedOnDiscovery);
			_devicesCurrent.Sort((a, b) => b.rssi.CompareTo(a.rssi));

			// reclaim all devices we want to remove.
			for (int i = 0; i < _devicesRemovedOnDiscovery.Count; ++i)
			{
				ReclaimButtonWithDevice(_devicesRemovedOnDiscovery[i]);
			}

			// go through the current devices and either move the ones that already exist in the current list
			// or pull one from the pool to insert into the current space.
			for (int i = 0; i < _devicesCurrent.Count; ++i)
			{
				var device = _devicesCurrent[i];
				var deviceButton = _deviceButtons.Find(button => button.device.uid == device.uid);
				if (deviceButton == null)
				{
					deviceButton = CreateButton(device);
				}

				deviceButton.transform.SetSiblingIndex(i);
			}
		}

		/// <summary>
		/// Sets up the button pool with a constant amount of buttons.
		/// </summary>
		private void SetupButtonPool()
		{
			_buttonPool = new List<WearableDeviceDisplay>();
			_deviceButtons = new List<WearableDeviceDisplay>();

			for (int i = 0; i < INIT_POOL_SIZE; ++i)
			{
				CreateButton();
			}
		}

		/// <summary>
		/// Either instantiates or pulls a button from the pool based on availability. If a <paramref name="device"/>
		/// is provided, the button will be setup and made visible immediately.
		/// </summary>
		/// <returns>The button.</returns>
		/// <param name="device">Device.</param>
		private WearableDeviceDisplay CreateButton(Device? device = null)
		{
			WearableDeviceDisplay button = null;

			if (_buttonPool.Count > 0)
			{
				var idx = _buttonPool.Count - 1;

				button = _buttonPool[idx];
				_buttonPool.RemoveAt(idx);
			}
			else
			{
				button = Instantiate(_displayPrefab, _displayRootRectTransform, false);
			}

			if (device.HasValue)
			{
				button.Set(device.Value);
				button.gameObject.SetActive(true);
				_deviceButtons.Add(button);
			}
			else
			{
				button.gameObject.SetActive(false);
				_buttonPool.Add(button);
			}

			return button;
		}

		/// <summary>
		/// Reclaims the button with device.
		/// </summary>
		/// <param name="device">Device.</param>
		private void ReclaimButtonWithDevice(Device device)
		{
			var idxButton = _deviceButtons.FindIndex(button => button.device.uid == device.uid);

			if (idxButton >= 0)
			{
				var deviceButton = _deviceButtons[idxButton];
				_deviceButtons.RemoveAt(idxButton);
				_buttonPool.Add(deviceButton);

				deviceButton.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Forcibly reclaims all buttons.
		/// </summary>
		private void ReclaimAllButtons()
		{
			for (int i = 0; i < _deviceButtons.Count; ++i)
			{
				var button = _deviceButtons[i];

				_buttonPool.Add(button);
				button.gameObject.SetActive(false);
			}

			_deviceButtons.Clear();
		}

		/// <summary>
		/// Removes all display children from the root rect transform.
		/// </summary>
		private void TeardownButtonPool()
		{
			var childCount = _displayRootRectTransform.childCount;

			if (childCount == 0)
			{
				return;
			}

			for (var i = childCount - 1; i >= 0; i--)
			{
				var child = _displayRootRectTransform.GetChild(i);
				Destroy(child.gameObject);
			}

			if (_deviceButtons != null)
			{
				_deviceButtons.Clear();
			}

			if (_buttonPool != null)
			{
				_buttonPool.Clear();
			}
		}

		private void PlayBackgroundAudio()
		{
			if (!_srcSearching.isPlaying)
			{
				_srcSearching.clip = _sfxSearching;
				_srcSearching.loop = true;

				_audioControl.FadeIn(_srcSearching, TIME_BACKGROUND_FADE);
			}
		}

		private void StopBackgroundAudio()
		{
			if (_srcSearching.isPlaying)
			{
				_audioControl.FadeOut(_srcSearching, TIME_BACKGROUND_FADE);
			}
		}

		private void PlayFoundSting()
		{
			_audioControl.PlayOneShot(_sfxFoundDevice);
		}

		protected override void TeardownAudio()
		{
			if (_srcSearching != null)
			{
				Destroy(_srcSearching.gameObject);
			}
		}
	}
}
