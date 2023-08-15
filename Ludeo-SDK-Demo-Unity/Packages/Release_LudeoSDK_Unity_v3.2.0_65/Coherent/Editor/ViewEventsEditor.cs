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

using UnityEditor;
using UnityEngine;

namespace cohtml
{
[CustomEditor(typeof(ViewEvents))]
public class ViewEventsEditor : Editor
{
	readonly string[] m_EventNames = new string[] {
		"OnBindingsReleased",
		"OnDOMBuilt",
		"OnReadyForBindings",
	};
	SerializedProperty m_TargetViewProperty;
	SerializedProperty[] m_EventsProperties;
	bool[] m_IsEventAdded;
	GUIContent m_IconToolbarMinus;
	GUIContent[] m_EventTypes;
	GUIContent m_AddButonContent;

	protected virtual void OnEnable()
	{
		m_TargetViewProperty = serializedObject.FindProperty("m_TargetView");
		m_EventsProperties = new SerializedProperty[m_EventNames.Length];

		for (int i = 0; i < m_EventsProperties.Length; i++)
		{
			m_EventsProperties[i] = serializedObject.FindProperty(m_EventNames[i]);
		}

		m_IsEventAdded = new bool[m_EventNames.Length];

		m_AddButonContent = new GUIContent("Add New Event Type");
		m_IconToolbarMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"));
		m_IconToolbarMinus.tooltip = "Remove event.";
		m_EventTypes = new GUIContent[m_EventNames.Length];

		for (int i = 0; i < m_EventNames.Length; i++)
		{
			string displayName = m_EventNames[i];
			displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);
			m_EventTypes[i] = new GUIContent(displayName);
		}
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		int toBeRemovedEntry = -1;

		EditorGUILayout.PropertyField(m_TargetViewProperty);
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		Vector2 vector2 = GUIStyle.none.CalcSize(m_IconToolbarMinus);

		for (int i = 0; i < m_EventsProperties.Length; i++)
		{
			if (!m_IsEventAdded[i] && !IsEventInUse(i))
			{
				continue;
			}

			EditorGUILayout.PropertyField(m_EventsProperties[i], m_EventTypes[i], new GUILayoutOption[0]);

			Rect lastRect = GUILayoutUtility.GetLastRect();
			Rect rect = new Rect(lastRect.xMax - vector2.x - 8.0f, lastRect.y + 1f, vector2.x, vector2.y);

			if (GUI.Button(rect, m_IconToolbarMinus, GUIStyle.none))
			{
				toBeRemovedEntry = i;
			}

			EditorGUILayout.Space();
		}

		if (toBeRemovedEntry > -1)
		{
			RemoveEntry(toBeRemovedEntry);
		}

		Rect rect1 = GUILayoutUtility.GetRect(m_AddButonContent, GUI.skin.button);
		rect1.x = rect1.x + ((rect1.width - 200.0f) / 2.0f);
		rect1.width = 200f;

		if (GUI.Button(rect1, m_AddButonContent))
		{
			ShowAddEventMenu();
		}

		serializedObject.ApplyModifiedProperties();
	}

	void RemoveEntry(int index)
	{
		m_IsEventAdded[index] = false;

		switch (index)
		{
		case 0:
			(target as ViewEvents).OnBindingsReleased = new ViewEvents.OnBindingsReleasedEvent();
			break;
		case 1:
			(target as ViewEvents).OnDOMBuilt = new ViewEvents.OnDOMBuiltEvent();
			break;
		case 2:
			(target as ViewEvents).OnReadyForBindings = new ViewEvents.OnReadyForBindingsEvent();
			break;
		default:
			break;
		}
	}

	bool IsEventInUse(int index)
	{
		switch (index)
		{
		case 0:
			return (target as ViewEvents).OnBindingsReleased.GetPersistentEventCount() > 0;
		case 1:
			return (target as ViewEvents).OnDOMBuilt.GetPersistentEventCount() > 0;
		case 2:
			return (target as ViewEvents).OnReadyForBindings.GetPersistentEventCount() > 0;
		default:
			return false;
		}
	}

	void ShowAddEventMenu()
	{
		GenericMenu menu = new GenericMenu();

		for (int i = 0; i < m_EventTypes.Length; i++)
		{
			if (!m_IsEventAdded[i] && !IsEventInUse(i))
			{
				menu.AddItem(m_EventTypes[i], false, OnAddNewSelected, i);
			}
			else
			{
				menu.AddDisabledItem(m_EventTypes[i]);
			}
		}

		menu.ShowAsContext();

		Event.current.Use();
	}

	void OnAddNewSelected(object index)
	{
		m_IsEventAdded[(int)index] = true;
	}
}
}
