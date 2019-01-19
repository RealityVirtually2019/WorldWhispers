using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomPropertyDrawer(typeof(WearableMobileProvider))]
	public class WearableMobileProviderDrawer : PropertyDrawer
	{
		private const string DescriptionBox =
			"A provider that simulates a Wearable Device using the IMU built into a mobile device. In addition " +
			"to on-device builds, this also works in the editor via the Unity Remote app for easier prototyping. " +
			"All coördinates are relative to the same frame and have the same units as the Wearable Device.";
		private const string DisconnectLabel = "Simulate Device Disconnect";
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUILayout.HelpBox(DescriptionBox, MessageType.None);
			EditorGUILayout.Space();
			GUI.enabled = EditorApplication.isPlaying;
			bool shouldDisconnect = GUILayout.Button(DisconnectLabel, WearableConstants.EmptyLayoutOptions);
			GUI.enabled = true;
			if (shouldDisconnect)
			{
				WearableMobileProvider provider = fieldInfo.GetValue(property.serializedObject.targetObject) as WearableMobileProvider;
				provider.SimulateDisconnect();
			}
		}
	}
}
