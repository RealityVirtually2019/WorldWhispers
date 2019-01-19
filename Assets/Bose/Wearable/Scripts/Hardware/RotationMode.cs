using System;

namespace Bose.Wearable {
	/// <summary>
	/// 	When using SixDof:
	///     	- magnetometer is not used to improve rotation stability.
	///     	- "gameRotation" on the SensorFrame is not updated.
	/// 	When using NineDof:
	/// 		- magnetometer is enabled to improve rotation stability.
	///     	- "gameRotation" on the SensorFrame is not updated.
	/// 	When using Both:
	///		- "rotation" of SensorFrame is equivalent to using NineDof
	///		- "gameRotation" of SensorFrame is equivalent to using SixDof
	/// </summary>
	[Flags]
	public enum RotationMode
	{
		SixDof = (1 << 0),
		NineDof = (1 << 1),
		Both = (SixDof | NineDof)
	}
}
