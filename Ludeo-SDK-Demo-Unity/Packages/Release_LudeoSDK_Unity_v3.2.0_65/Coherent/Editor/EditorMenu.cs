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

using cohtml.InputSystem;
using UnityEngine;
using UnityEditor;

namespace cohtml
{
public class EditorMenu
{
	#if !ENABLE_INPUT_SYSTEM
	[MenuItem("Gameface/Populate gamepad controls", priority = 10)]
	public static void PopulateGamepadControls()
	{
		GamepadButtonsInserter.PopulateGamepadControls();
	}
	#endif

	[MenuItem("Gameface/Update Cohtml scripting backend", priority = 15)]
	public static void UpdateCohtmlScriptingBackend()
	{
		ScriptingBackendAdjuster.UpdateCohtmlScriptingBackend();

		InfoWindow.Popup("Cohtml", "Plugin forced manually to sync with Unity3D scripting backend to " +
		                           $"{PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone)}." +
		                           "That process will be done automatically on Prebuilt step.", "Ok");
	}

	[MenuItem("Gameface/Configure Library", priority = 20)]
	static void ShowLibraryConfigWindow()
	{
		LibraryConfigurationWindow.Popup();
	}

	[MenuItem("Gameface/Samples/Remove all", priority = 25)]
	static void RemoveSamples()
	{
		ResourcesImporter.RemoveSamples();
	}

	[MenuItem("Gameface/Samples/Import Frontend Samples", priority = 25)]
	static void ImportFrontendSamples()
	{
		ResourcesImporter.ImportFrontendSamples();
	}

	[MenuItem("GameObject/Gameface/Add Screen View", priority = 10)]
	[MenuItem("Gameface/Add Screen View", priority = 100)]
	static void AddScreenView()
	{
		if (Camera.main == null)
		{
			GameObject go = new GameObject("Main Camera");
			go.tag = "MainCamera";
			go.AddComponent<Camera>();
			go.AddComponent<AudioListener>();
		}

		if (Camera.main.GetComponent<CohtmlView>() != null)
		{
			LogHandler.Log("CohtmlView already added on the main camera.");
		}
		else
		{
			Camera.main.gameObject.AddComponent<CohtmlView>();
			LogHandler.Log("CohtmlView added on the main camera.");
		}
	}

	[MenuItem("GameObject/Gameface/Add Input Handler", priority = 10)]
	[MenuItem("Gameface/Add Input Handler", priority = 100)]
	static void AddInputEventSystem()
	{
		CohtmlInputHandler.GetOrInitInstance();
	}

	[MenuItem("GameObject/Gameface/Add World View", priority = 11)]
	[MenuItem("Gameface/Add World View", priority = 101)]
	static void AddWorldView()
	{
		GameObject worldView = new GameObject
		{
			name = "CohtmlWorldView",
		};

		CohtmlView view = worldView.AddComponent<CohtmlView>();
		view.AddMeshToWorldView();
		ResourcesImporter.ImportHelloTemplate();

		LogHandler.Log("CohtmlWorldView GameObject added in the scene.");
	}

	[MenuItem("GameObject/Gameface/Add UI System", priority = 12)]
	[MenuItem("Gameface/Add UI System", priority = 102)]
	static void AddUISystem()
	{
		GameObject go = new GameObject("CohtmlUISystem");
		go.AddComponent<CohtmlUISystem>();

		LogHandler.Log("CohtmlUISystem added in the scene.");
	}

	[MenuItem("Gameface/Open DevTools", priority = 103)]
	static void OpenDevTools()
	{
		CohtmlSystemSettingsEditor.OpenDevTools();
	}

	[MenuItem("Gameface/Open Documentation", priority = 200)]
	static void Documentation()
	{
		Application.OpenURL("https://coherent-labs.com/documentation/");
	}

	[MenuItem(KnownIssuesWindow.ContextMenuKnownIssuesWindow)]
	public static void DisplayKnownIssues()
	{
		KnownIssuesWindow.DisplayContent();
	}
}
}
