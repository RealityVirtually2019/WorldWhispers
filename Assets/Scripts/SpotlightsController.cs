using Bose.Wearable;
using UnityEngine;

public class SpotlightsController : MonoBehaviour
{
    private WearableControl _wearableControl;
    private RotationMatcher _matcher;

    private SensorQuaternion userCenter;

    public enum Stage {
        INITIAL,
        SELECTING,
        SELECTED
    }
    private Stage currentStage = Stage.INITIAL;

    void Start() {
        // SetCoroutine()
    }

    void Update() {
        if(currentStage == Stage.INITIAL) {
            // SFXController sFXController = GetComponent<SFXController>();
            // sFXController.playIntroAudio();
        }
        else if (currentStage == Stage.SELECTING) {

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

        SensorQuaternion currentQ = _wearableControl.LastSensorFrame.rotation;
        Debug.Log(currentQ.value.eulerAngles.x - userCenter.value.eulerAngles.x);
        SFXController sFXController = GetComponent<SFXController>();
        sFXController.playDing();

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
        this.SetRelativeReference();
        this.setRelativeDirection();
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
}
