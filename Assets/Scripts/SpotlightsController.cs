using Bose.Wearable;
using UnityEngine;
using System;
public class SpotlightsController : MonoBehaviour
{
    private WearableControl _wearableControl;
    private RotationMatcher _matcher;

    private SensorQuaternion userCenter;

    // public enum Stage {
    //     INITIAL,
    //     SELECTING,
    //     SELECTED
    // }
    // private Stage currentStage = Stage.INITIAL;

    private bool _calibrating;
    private float _calibrationStartTime;

    private Quaternion _referenceRotation;

    /// <summary>
    /// Invoked when calibration is complete.
    /// </summary>
    public event Action CalibrationCompleted;

    /// <summary>
    /// The audio layer for discovery
    /// </summary>
    [SerializeField]
    protected AudioSource _audioSource;
        [SerializeField]
    protected AudioClip _dingClip;
    void Start() {
        StartCalibration();
    }

    
    public void playDing()
    {
        _audioSource.PlayOneShot(_dingClip);
    }

    public void playLeftAudio()
    {

    }

    public void playRightAudio()
    {

    }

    /// <summary>
    /// Begin the calibration routine. Waits for <see cref="_minCalibrationTime"/>, then until rotational
    /// velocity falls below <see cref="_calibrationMotionThreshold"/> before sampling the rotation sensor.
    /// Will not calibrate for longer than <see cref="_maxCalibrationTime"/>.
    /// </summary>
    private void StartCalibration()
    {
        _calibrating = true;
        _calibrationStartTime = Time.unscaledTime;
    }

    void Update() {
        if (_calibrating) {
            SensorFrame frame = _wearableControl.LastSensorFrame;

            bool didWaitEnough = Time.unscaledTime > _calibrationStartTime + 5;
            bool isStationary = frame.angularVelocity.value.magnitude < 1;
            bool didTimeout = Time.unscaledTime > _calibrationStartTime + 10;
            if ((didWaitEnough && isStationary) || didTimeout)
            {
                _referenceRotation = frame.rotation;
                _calibrating = false;

                // Pass along the reference to the rotation matcher on the widget.
                _matcher.SetRelativeReference(frame.rotation);

                if (CalibrationCompleted != null)
                {
                    CalibrationCompleted.Invoke();
                }

            }
        }
    }

    /// <summary>
    /// Sets center view axis for orienting across spotlights
    /// </summary>
    protected void setRelativeDirection()
    {
        userCenter = _wearableControl.LastSensorFrame.rotation;
    }

    public void onConfirmLookDirection() {
        playDing();

        SensorQuaternion currentQ = _wearableControl.LastSensorFrame.rotation;
        float diff = currentQ.value.eulerAngles.y - userCenter.value.eulerAngles.y;
        // float DotResult = Vector3.Dot(transform., currentQ.value.eulerAngles);
        // Debug.Log(diff);
        // if(diff < 30f && diff > 300f) {
        //     Debug.Log("Center"+ Time.time + " " + diff);
        //     play
        // }
        // else 
        if (diff > 0 && diff < 180f) {
            Debug.Log("Right"+ Time.time+ " " + diff);
            playLeftAudio();
        }
        else {
            Debug.Log("Left" + Time.time+ " " + diff);
            playRightAudio();
        }

    }

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
            _wearableControl.GyroscopeSensor.Stop();
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
    /// Configures the update interval and sets all needed sensors
    /// </summary>
    private void StartSensors()
    {
        if (_wearableControl.ConnectedDevice != null)
        {
            _wearableControl.SetSensorUpdateInterval(SensorUpdateInterval.FortyMs);
            _wearableControl.RotationSensor.Start();
            _wearableControl.GyroscopeSensor.Start();
        }
    }
}
