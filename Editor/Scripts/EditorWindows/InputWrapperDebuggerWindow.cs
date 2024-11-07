using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using UnityEditor;
using UnityEngine;
using FontStyle = UnityEngine.FontStyle;

namespace NPTP.InputSystemWrapper.Editor.EditorWindows
{
	public class InputWrapperDebuggerWindow : EditorWindow
	{
		private const string EMPTY = "";
		private const int MAX_SHOWN_RECENT_CONTEXTS = 3;

		private class TimestampedObject<T>
		{
			public T Value { get; }
			public string Timestamp { get; }

			public TimestampedObject(T value, string timestamp)
			{
				Value = value;
				Timestamp = timestamp;
			}
		}
		
		private List<TimestampedObject<InputContext>> mostRecentContexts = new();

		private OfflineInputData offlineInputData;
		private OfflineInputData OfflineInputData
		{
			get
			{
				if (offlineInputData == null) offlineInputData = Helper.OfflineInputData;
				return offlineInputData;
			}
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
			EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
		}

		private void HandlePlayModeStateChanged(PlayModeStateChange state)
		{
			
			switch (state)
			{
				case PlayModeStateChange.EnteredPlayMode:
					mostRecentContexts.Clear();
					mostRecentContexts.Add(new TimestampedObject<InputContext>(Input.Context, 0.ToString()));
					Input.EDITOR_OnPlayerInputContextChanged += HandlePlayerInputContextChanged;
					break;
				case PlayModeStateChange.ExitingPlayMode:
					Input.EDITOR_OnPlayerInputContextChanged -= HandlePlayerInputContextChanged;
					break;
			}
		}

		private void HandlePlayerInputContextChanged(PlayerID playerID, InputContext inputContext)
		{
			NPTPDebug.Log($"Input Context changed for {playerID}: {inputContext}");
			mostRecentContexts.Add(new TimestampedObject<InputContext>(inputContext, Time.frameCount.ToString()));
			if (mostRecentContexts.Count > MAX_SHOWN_RECENT_CONTEXTS)
			{
				mostRecentContexts.RemoveAt(0);
			}
		}

		// Updates the window when in play mode, so it shows up-to-date runtime debug info.
		public void OnInspectorUpdate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			
			Repaint();
		}
		
		private void OnGUI()
		{
			if (!Application.isPlaying)
			{
				EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
				EditorGUILayout.LabelField("You must be in play mode to use the debugger.", new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.BoldAndItalic });
				return;
			}
			
			GUILayout.BeginVertical();
			ShowDebugInfoField("Current Control Scheme", Input.CurrentControlScheme.ToString());
			ShowDebugInfoField("Current Context", Input.Context.ToString());
			ShowIndentedField("Active Maps", ActiveMapLabelFields);
			ShowIndentedField("Most Recent Contexts", MostRecentContextLabelFields);
			GUILayout.EndVertical();
		}

		private void ShowIndentedField(string fieldName, Action showAction)
		{
			ShowDebugInfoField(fieldName);
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical();
			showAction?.Invoke();
			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}

		private void ActiveMapLabelFields()
		{
			foreach (InputContextInfo inputContextInfo in OfflineInputData.InputContexts)
			{
				if (inputContextInfo.Name.AsEnumMember() != Input.Context.ToString())
				{
					continue;
				}

				foreach (string activeMap in inputContextInfo.ActiveMaps)
				{
					EditorGUILayout.LabelField(activeMap);
				}

				break;
			}
		}

		private void MostRecentContextLabelFields()
		{
			for (int i = mostRecentContexts.Count - 1; i >= 0; i--)
			{
				EditorGUILayout.LabelField($"{mostRecentContexts[i].Value} [Frame {mostRecentContexts[i].Timestamp}]");
			}
		}

		private static void ShowDebugInfoField(string boldLabel, string info = EMPTY)
		{
			string labelExtended = boldLabel + ": ";
			GUIStyle boldStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
			Vector2 labelSize = boldStyle.CalcSize(new GUIContent(labelExtended));
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(labelExtended, boldStyle, GUILayout.Width(labelSize.x));
			EditorGUILayout.LabelField(info, EditorStyles.label);
			EditorGUILayout.EndHorizontal();
		}
	}
}