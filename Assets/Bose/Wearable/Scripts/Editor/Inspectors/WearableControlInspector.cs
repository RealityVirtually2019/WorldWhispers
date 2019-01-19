using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Bose.Wearable.Editor.Inspectors
{
	[CustomEditor(typeof(WearableControl))]
	public class WearableControlInspector : UnityEditor.Editor
	{
		private SerializedProperty _editorProvider;
		private SerializedProperty _runtimeProvider;

		private const string ProviderUnsupportedEditorWarning = "Provider is not supported in the editor.";
		private const string EditorDefaultTitle = "Editor Default";
		private const string RuntimeDefaultTitle = "Runtime Default";
		private const string TitleSeparator = " - ";
		
		private const string EditorDefaultProviderField = "_editorDefaultProvider";
		private const string RuntimeDefaultProviderField = "_runtimeDefaultProvider";
		private const string UpdateModeField = "_updateMode";
		private const string DebugProviderField = "_debugProvider";
		private const string DeviceProviderField = "_deviceProvider";
		private const string MobileProviderField = "_mobileProvider";
		private const string ProxyProviderField = "_proxyProvider";
		
		private void OnEnable()
		{
			_editorProvider = serializedObject.FindProperty(EditorDefaultProviderField);
			_runtimeProvider = serializedObject.FindProperty(RuntimeDefaultProviderField);
		}
		
		private void DrawProviderBox(string field, ProviderId provider)
		{
			bool isEditorDefault = _editorProvider.enumValueIndex == (int) provider;
			bool isRuntimeDefault = _runtimeProvider.enumValueIndex == (int) provider;

			if (isEditorDefault || isRuntimeDefault)
			{
				GUILayout.Box(String.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));
				
				StringBuilder titleBuilder = new StringBuilder();
				titleBuilder.Append(Enum.GetName(typeof(ProviderId), provider));
				
				if (isEditorDefault)
				{
					titleBuilder.Append(TitleSeparator);
					titleBuilder.Append(EditorDefaultTitle);
				}

				if (isRuntimeDefault)
				{
					titleBuilder.Append(TitleSeparator);
					titleBuilder.Append(RuntimeDefaultTitle);
				}
				
				EditorGUILayout.LabelField(titleBuilder.ToString(), WearableConstants.EmptyLayoutOptions);
				
				EditorGUILayout.PropertyField(serializedObject.FindProperty(field), WearableConstants.EmptyLayoutOptions);
			}
		}
		
		public override void OnInspectorGUI()
		{			
			serializedObject.Update();
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty(UpdateModeField), WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(_editorProvider, WearableConstants.EmptyLayoutOptions);
			EditorGUILayout.PropertyField(_runtimeProvider, WearableConstants.EmptyLayoutOptions);
			
			DrawProviderBox(DebugProviderField, ProviderId.DebugProvider);
			DrawProviderBox(ProxyProviderField, ProviderId.WearableProxy);
			DrawProviderBox(MobileProviderField, ProviderId.MobileProvider);
			DrawProviderBox(DeviceProviderField, ProviderId.WearableDevice);
			if (_editorProvider.enumValueIndex == (int) ProviderId.WearableDevice)
			{
				EditorGUILayout.HelpBox(ProviderUnsupportedEditorWarning, MessageType.Error);
			}
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}
