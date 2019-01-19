using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{

	[CustomPropertyDrawer(typeof(WearableDebugProvider))]
	public class WearableDebugProviderDrawer : PropertyDrawer
	{
		private const string DeviceNameField = "_name";
		private const string RSSIField = "_rssi";
		private const string UIDField = "_uid";
		private const string ProductIdField = "_productId";
		private const string VariantIdField = "_variantId";
		private const string VerboseField = "_verbose";
		private const string DescriptionBox =
			"Provides a minimal data provider that allows connection to a virtual device, and " + 
			"logs messages when provider methods are called. Never generates data frames.";
		private const string DisconnectLabel = "Simulate Device Disconnect";
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{	
			EditorGUI.BeginProperty(position, label, property);

			EditorGUILayout.HelpBox(DescriptionBox, MessageType.None);
			EditorGUILayout.Space();
			
			EditorGUILayout.PropertyField(property.FindPropertyRelative(DeviceNameField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(RSSIField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(UIDField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(ProductIdField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(VariantIdField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(property.FindPropertyRelative(VerboseField), WearableConstants.EmptyLayoutOptions);
			
			EditorGUILayout.Space();
			GUI.enabled = EditorApplication.isPlaying;
			bool shouldDisconnect = GUILayout.Button(DisconnectLabel, WearableConstants.EmptyLayoutOptions);
			GUI.enabled = true;
			if (shouldDisconnect)
			{
				WearableDebugProvider provider = fieldInfo.GetValue(property.serializedObject.targetObject) as WearableDebugProvider;
				provider.SimulateDisconnect();
			}
			
			EditorGUI.EndProperty();
		}
	}
	
}
