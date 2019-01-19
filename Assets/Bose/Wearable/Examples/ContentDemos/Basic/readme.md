# Basic Demo
## Summary
This demo provides a simple demonstration of connecting to the hardware, subscribing to data from individual sensors, and interfacing that data with Unity objects. 

The `RotationMatcher` component (on the "Glasses" GameObject) is used to apply the rotation of the Bose AR device to a Unity GameObject. To demonstrate this, the component will be applied to a 3D model that looks similar to the connected Bose AR device. This enables the representation of the glasses to match their physical orientation.

The `GestureDetector` component (also on the "Glasses" GameObject) is used to subscribe a pre-selected gesture and invoke a method when that gesture is detected. To demonstrate this, a particle effect in the scene will play whenever a `DoubleTap` event is detected.

## Functionality
### Absolute Mode
Tapping on the Absolute button transitions to Absolute mode, with the glasses’ rotation set directly to the sensor data provided by the device. When in absolute mode, the virtual glasses directly match the orientation of the physical glasses, with an approximation of magnetic north represented as +Z (away from the camera). 

### Relative Mode
Tapping on the Relative button transitions to Relative mode, which takes the current orientation of the Bose AR device as the new reference rotation. In relative mode, the virtual glasses present a rotation relative to the stored reference rotation.