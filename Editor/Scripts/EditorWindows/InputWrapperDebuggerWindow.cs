using NPTP.InputSystemWrapper.Data;
using UnityEditor;
using UnityEngine;
using FontStyle = UnityEngine.FontStyle;

namespace NPTP.InputSystemWrapper.Editor.EditorWindows
{
	public class InputWrapperDebuggerWindow : EditorWindow
	{
		private const string EMPTY = "";

		private OfflineInputData offlineInputData;
		private OfflineInputData OfflineInputData
		{
			get
			{
				if (offlineInputData == null) offlineInputData = Helper.OfflineInputData;
				return offlineInputData;
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
			ShowActiveMaps();
			GUILayout.EndVertical();
		}

		private void ShowActiveMaps()
		{
			ShowDebugInfoField("Active Maps");
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical();
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

			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
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