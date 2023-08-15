/*
This file is part of Cohtml, Gameface and Prysm - modern user interface technologies.

Copyright (c) 2012-2023 Coherent Labs AD and/or its licensors. All
rights reserved in all media.

The coded instructions, statements, computer programs, and/or related
material (collectively the "Data") in these files contain confidential
and unpublished information proprietary Coherent Labs and/or its
licensors, which is protected by United States of America federal
copyright law and by international treaties.

This software or source code is supplied under the terms of a license
agreement and nondisclosure agreement with Coherent Labs AD and may
not be copied, disclosed, or exploited except in accordance with the
terms of that agreement. The Data may not be disclosed or distributed to
third parties, in whole or in part, without the prior written consent of
Coherent Labs AD.

COHERENT LABS MAKES NO REPRESENTATION ABOUT THE SUITABILITY OF THIS
SOURCE CODE FOR ANY PURPOSE. THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT
HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY, NONINFRINGEMENT, AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER, ITS AFFILIATES,
PARENT COMPANIES, LICENSORS, SUPPLIERS, OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
ANY WAY OUT OF THE USE OR PERFORMANCE OF THIS SOFTWARE OR SOURCE CODE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Diagnostics;
using cohtml;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CohtmlSystemSettings))]
public class CohtmlSystemSettingsEditor : CohtmlEditor
{
	SerializedProperty m_EnableDebugger;
	SerializedProperty m_EnableDebuggerInBuild;
	SerializedProperty m_DebuggerPort;

	public override void OnInspectorGUI()
	{
		ExposeProperties.AddCohtmlLayer();
		serializedObject.Update();
		m_EnableDebugger = serializedObject.FindProperty("m_EnableDebugger");
		m_EnableDebugger.boolValue = EditorGUILayout.BeginToggleGroup(
			new GUIContent("Enable DevTools Debugger",
				"Enable DevTools debugging tools in Unity3D Editor and development builds. You can access the Devtool through Context menu Gameface/Open Devtools. Require Google Chrome browser installed"),
			m_EnableDebugger.boolValue);

		if (m_EnableDebugger.boolValue)
		{
			m_DebuggerPort = serializedObject.FindProperty("m_DebuggerPort");
			m_EnableDebuggerInBuild = serializedObject.FindProperty("m_EnableDebuggerInBuild");
			m_DebuggerPort.intValue = EditorGUILayout.IntField(new GUIContent("Debug Port", "The port where the system will listen for the debugger."), Mathf.Clamp(m_DebuggerPort.intValue, 1, ushort.MaxValue));
			m_EnableDebuggerInBuild.boolValue =
				GUILayout.Toggle(m_EnableDebuggerInBuild.boolValue,
					new GUIContent("Enable debugging in build",
						"Enable DevTools debugging in all built applications. You can access the Devtools through Google Chrome browser with machine IP and opened port. Example:\"[IP]:[DebuggerPort]\")."));
		}

		EditorGUILayout.EndToggleGroup();

		DrawPropertiesExcluding(serializedObject, "m_DebuggerPort", "m_EnableDebugger", "m_EnableDebuggerInBuild", "m_Script");
		serializedObject.ApplyModifiedProperties();
	}

	public static void OpenDevTools()
	{
		CohtmlUISystem system = CohtmlUISystem.DefaultUISystem;

		string chromePath = EditorUtils.FindBrowserExecutablePath();

		if (!Application.isEditor || !Application.isPlaying)
		{
			InfoWindow.Popup("Cohtml", "Your game must be running in the Editor in order to use the DevTools!", "Ok");
		}
		else if (system == null)
		{
			InfoWindow.Popup("Cohtml", "A CohtmlUISystem must be present in the game in order to use the DevTools!", "Ok");
		}
		else if (!system.Settings.m_EnableDebugger)
		{
			InfoWindow.Popup("Cohtml", "Debugging is currently disabled. Enable the Cohtml Devtools debugger of your CohtmlUISystem", "Ok");
		}
		#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
		else if(string.IsNullOrEmpty(chromePath) || !System.IO.File.Exists(chromePath))
		{
			InfoWindow.Popup("Cohtml", "You must have Google Chrome installed in order to use the DevTools!", "Ok");
		}
		else
		{
			Process.Start(chromePath, $"http://localhost:{system.Settings.DebuggerPort}");
		}
		#else
		else
		{
			InfoWindow.Popup("Cohtml", string.Format("You must open Google Chrome manually and navigate to \"http://localhost:{0}\"", system.Settings.DebuggerPort), "Ok");
		}
		#endif
	}
}