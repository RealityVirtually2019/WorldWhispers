using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bose.Wearable;
using UnityEngine;
using UnityEngine.Events;

// code inspired from this repo https://github.com/kbryla/rift_unity_scripts/blob/master/RiftGestures/RiftGesture.cs

namespace FrameSynthesis.VR {
    struct Sample
    {
        public float timeStamp;
        public Quaternion orientation;

        public Sample(float timeStamp, Quaternion orientation)
        {
            this.timeStamp = timeStamp;
            this.orientation = orientation;
        }

    }

    public class HeadGestureDetector : MonoBehaviour
    {

        [SerializeField]
		private UnityEvent _onNodGestureDetected;
        [SerializeField]
		private UnityEvent _onHeadShakeGestureDetected;
        /// list of samples of of previous head orientations
        LinkedList<Sample> samples;
        float waitTime = 0f;
        const float detectInterval = 0.5f;

        private WearableControl _wearableControl;
		private RotationMatcher _matcher;

        public HeadGestureDetector() 
        {
            samples = new LinkedList<Sample>();
        }

        private void Awake()
        {
            _matcher = GetComponent<RotationMatcher>();
            _wearableControl = WearableControl.Instance;
            StartSensors();
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
			// Subscribe to DeviceConnected to handle reconnects that happen during play.
			_wearableControl.DeviceConnected += OnDeviceConnected;
		}

		private void OnDisable()
		{
			_wearableControl.DeviceConnected -= OnDeviceConnected;
		}

        private void OnDeviceConnected(Device obj)
		{
			// If a device is reconnected during play, ensures the rotation sensor is still running.
			StartSensors();
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

        public void Update()
        {
            // sample current orientation of frame
            SensorQuaternion q = _wearableControl.LastSensorFrame.rotation;
            samples.AddFirst(new Sample(Time.time, q));

            // max of 60 samples
            if (samples.Count >= 60) {
                samples.RemoveLast();
            }
            if (waitTime > 0) {
                waitTime -= Time.deltaTime;
            } else {
                DetectNod();
                DetectHeadshake();
            }

        }

        /// <summary>
		/// Filters an enumeration of smaples between the given start and end time
		/// </summary>        
        IEnumerable<Sample> Range(float startTime, float endTime)
        {
            return samples.Where(sample => (sample.timeStamp < Time.time - startTime &&
                sample.timeStamp >= Time.time - endTime));
        }

        void DetectNod()
        {
            try {
                float basePos = Range(0.2f, 0.4f).Average(sample => sample.orientation.eulerAngles.x);
                float xMax = Range(0.01f, 0.2f).Max(sample =>  sample.orientation.eulerAngles.x);
                float xMin = Range(0.01f, 0.2f).Min(sample => sample.orientation.eulerAngles.x);
                float current = samples.First().orientation.eulerAngles.x;
		
                if ((xMax - basePos > 10f ||  basePos - xMin > 10f) &&
                    Mathf.Abs(current - basePos) < 5f) {
                        _onNodGestureDetected.Invoke();
                        waitTime = detectInterval;
                }
            } catch (InvalidOperationException) {
                // Range contains no entry
            }
        }

        void DetectHeadshake()
        {
            try {
                float basePos = Range(0.2f, 0.4f).Average(sample => sample.orientation.eulerAngles.y);
                float yMax = Range(0.01f, 0.2f).Max(sample => sample.orientation.eulerAngles.y);
                float yMin = Range(0.01f, 0.2f).Min(sample => sample.orientation.eulerAngles.y);
                float current = samples.First().orientation.eulerAngles.y;

                if ((yMax - basePos > 10f || basePos - yMin > 10f) &&
                    Mathf.Abs(current - basePos) < 5f) {
                        _onHeadShakeGestureDetected.Invoke();
                        waitTime = detectInterval;
                }
            } catch (InvalidOperationException) {
                // Range contains no entry
            }
        }
    }
}
