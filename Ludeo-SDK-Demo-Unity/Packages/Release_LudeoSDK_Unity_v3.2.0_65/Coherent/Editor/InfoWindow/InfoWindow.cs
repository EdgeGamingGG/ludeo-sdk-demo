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

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace cohtml
{
public class InfoWindow : EditorWindow
{
	[Serializable]
	public class Data
	{
		[SerializeField]
		public string OkButtonText;

		[SerializeField]
		public string Message;
	}

	private const string Directory = "Editor/InfoWindow";
	private const string DataSessionKey = "Cohtml_InfoWindow_Data";

	public static InfoWindow m_Instance;
	public static Action m_OnOkEvent;

	public Label messageLabel;
	public Button okButton;

	public static InfoWindow Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = (InfoWindow)GetWindow(typeof(InfoWindow));
			}

			return m_Instance;
		}
		set => m_Instance = value;
	}

	public static void Popup(string title, string message, string okButtonText, Action onOkEvent = null)
	{
		Data data = new Data
		{
			Message = message,
			OkButtonText = okButtonText
		};
		SessionState.SetString(DataSessionKey, JsonUtility.ToJson(data));

		m_OnOkEvent = onOkEvent;
		Instance.titleContent = new GUIContent(title);
		Instance.minSize = new Vector2(400, 200);
	}

	public void OnEnable()
	{
		Data data = JsonUtility.FromJson<Data>(SessionState.GetString(DataSessionKey, null));

		VisualElement root = Instance.rootVisualElement;

		VisualTreeAsset uXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Utils.CombinePaths(Utils.CohtmlUpmPath, Directory, "Content.uxml"));
		TemplateContainer ui = uXML.CloneTree();
		root.Add(ui.ElementAt(0));

		StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(Utils.CombinePaths(Utils.CohtmlUpmPath, Directory, "Style.uss"));
		root.styleSheets.Add(styleSheet);

		messageLabel = Instance.rootVisualElement.Q<Label>("message");
		messageLabel.text = data.Message;

		okButton = Instance.rootVisualElement.Q<Button>("ok-button");
		okButton.text = data.OkButtonText;
		okButton.clickable.clicked += CloseWindow;

		if (m_OnOkEvent != null)
		{
			okButton.clickable.clicked += m_OnOkEvent;
		}
	}

	public void CloseWindow()
	{
		Instance.Close();
		m_Instance = null;
		SessionState.EraseString(DataSessionKey);
	}
}
}
