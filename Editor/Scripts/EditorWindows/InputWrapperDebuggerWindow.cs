using System;
using System.Collections.Generic;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Editor.Utilities;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Player;
using UnityEditor;
using UnityEngine;
using FontStyle = UnityEngine.FontStyle;

namespace NPTP.InputSystemWrapper.Editor.EditorWindows
{
	internal class InputWrapperDebuggerWindow : EditorWindow
	{
		private const string EMPTY = "";
		private const int MAX_SHOWN_RECENT_CONTEXTS = 3;

		private class TimestampedObject<T>
		{
			internal T Value { get; }
			internal string Timestamp { get; }

			internal TimestampedObject(T value, string timestamp)
			{
				Value = value;
				Timestamp = timestamp;
			}
		}

		private readonly List<TimestampedObject<string>> mostRecentContexts = new();
		private int selectedPlayerID = 0; // TODO: Make switchable in the debugger UI

		private InputData inputData;
		private InputData InputData
		{
			get
			{
				if (inputData == null) inputData = Helper.InputData;
				return inputData;
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
					mostRecentContexts.Add(new TimestampedObject<string>(ContextName(InputRuntime.Current.EDITOR_GetDefaultContext()), 0.ToString()));
					InputRuntime.EDITOR_OnPlayerInputContextChanged += HandlePlayerInputContextChanged;
					break;
				case PlayModeStateChange.ExitingPlayMode:
					InputRuntime.EDITOR_OnPlayerInputContextChanged -= HandlePlayerInputContextChanged;
					break;
			}
		}

		private static string ContextName(InputContextId inputContextId)
		{
			return InputRuntime.Current == null ? inputContextId.Index.ToString() : InputRuntime.Current.EDITOR_GetContextName(inputContextId);
		}

		private void HandlePlayerInputContextChanged(int playerID, InputContextId inputContextId)
		{
			string contextName = ContextName(inputContextId);
			ISWDebug.Log($"Input Context changed for player {playerID}: {contextName}");
			mostRecentContexts.Add(new TimestampedObject<string>(contextName, Time.frameCount.ToString()));
			if (mostRecentContexts.Count > MAX_SHOWN_RECENT_CONTEXTS)
			{
				mostRecentContexts.RemoveAt(0);
			}
		}

		// Updates the window when in play mode, so it shows up-to-date runtime debug info.
		internal void OnInspectorUpdate()
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

			if (InputRuntime.Current is not { EDITOR_IsInitialized: true })
			{
				EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
				EditorGUILayout.LabelField("Input not yet initialized, waiting...", new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.BoldAndItalic });
				return;
			}

			if (InputRuntime.Current.EDITOR_TryGetPlayer(selectedPlayerID, out InputPlayer player))
			{
				GUILayout.BeginVertical();
				ShowDebugInfoField("Current Control Scheme", player.CurrentControlSchemeId.ToString());
				ShowDebugInfoField("Current Context", ContextName(player.InputContextId));
				ShowIndentedField("Active Maps", ActiveMapLabelFields);
				ShowIndentedField("Most Recent Contexts", MostRecentContextLabelFields);
				GUILayout.EndVertical();
			}
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
			foreach (InputContextInfo inputContextInfo in InputData.AuthoredContexts)
			{
				if (inputContextInfo.Name != ContextName(InputRuntime.Current.GetPlayer(selectedPlayerID).InputContextId))
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