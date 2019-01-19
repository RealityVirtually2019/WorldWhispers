using System;

namespace Bose.Wearable
{
	/// <summary>
	/// The VariantId of a hardware device.
	/// </summary>
	[Serializable]
	public enum VariantId : byte
	{
		Undefined = 0,
		BoseFramesAlto = 0x01,
		BoseFramesRondo = 0x02
	}
}
