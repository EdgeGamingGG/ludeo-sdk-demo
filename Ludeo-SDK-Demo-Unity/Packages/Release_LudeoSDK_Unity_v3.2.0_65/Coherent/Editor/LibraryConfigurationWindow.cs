using System;
using System.IO;
using cohtml.Net;
using UnityEditor;
using UnityEngine;

namespace cohtml
{
    public class LibraryConfigurationWindow : EditorWindow
    {
        private Vector2 m_MousePosition = Vector2.zero;
        private Vector2 m_ScrollPosition = Vector2.zero;

        private static bool m_ParamsChanged;

        private const int OptionLayoutHeight = 20;
        private const int OptionInfoLayoutHeight = 50;
        private const int OptionLayoutMaxWidth = 400;
        private const int InputFieldLayoutMaxWidth = 140;
        private const int WindowWidthPadding = 15;
        private const int WindowHeightPadding = 60;
        private const int EnableDebuggerOptionHeight = 100;

        private static ExtendedLibraryParams m_ExtendedLibraryParams;

        public static void Popup()
        {
            m_ExtendedLibraryParams = LibraryParamsManager.GetJsonParamsOrDefault();
            GetWindow(typeof(LibraryConfigurationWindow), true, "Gameface Library Configuration", true);
        }

        public void OnGUI()
        {
            if (m_MousePosition == Vector2.zero)
            {
                m_MousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                int fieldsLength = typeof(ExtendedLibraryParams).GetFields().Length;
                int height = fieldsLength * OptionLayoutHeight + OptionInfoLayoutHeight + WindowHeightPadding + EnableDebuggerOptionHeight;
                int width = InputFieldLayoutMaxWidth + OptionLayoutMaxWidth + WindowWidthPadding;
                position = new Rect(position.x, position.y, width, height);
            }

            GUILayout.Space(10);
            GUI.color = Color.white;

            SetConfigFields();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save"))
            {
                if (m_ParamsChanged)
                {
                    InfoWindow.Popup("Gameface", "Changes will take effect after restarting Unity3D editor.",
                        "Restart now",
                        OnRestartClicked);
                    m_ParamsChanged = false;
                }

                LibraryParamsManager.SaveJson(m_ExtendedLibraryParams);
                Close();
            }

            if (GUILayout.Button("Reset"))
            {
                m_ExtendedLibraryParams = new ExtendedLibraryParams();
                m_ParamsChanged = true;
            }
        }

        private void OnRestartClicked()
        {
            EditorApplication.OpenProject(Directory.GetCurrentDirectory());
        }

        private void SetConfigFields()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Logging Severity", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.LoggingSeverity = SetModelField(
                (Severity)EditorGUILayout.EnumPopup(m_ExtendedLibraryParams.LoggingSeverity, GUILayout.MaxWidth(InputFieldLayoutMaxWidth)),
                m_ExtendedLibraryParams.LoggingSeverity);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Use C# Rendering backend [Experimental]")
            {
                tooltip = "This option will make the plugin to switch from native \n" +
                          "rendering backend to Unity backend which uses Unity's rendering API. \n" +
                          "Read more in the documentation."
            }, GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(20));
            m_ExtendedLibraryParams.UseCSharpRenderingBackend = EditorGUILayout.Toggle(m_ExtendedLibraryParams.UseCSharpRenderingBackend, GUILayout.MaxWidth(InputFieldLayoutMaxWidth));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Default Style Font Family", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.DefaultStyleFontFamily = SetModelField(
                EditorGUILayout.TextField(m_ExtendedLibraryParams.DefaultStyleFontFamily, GUILayout.MaxWidth(InputFieldLayoutMaxWidth)),
                m_ExtendedLibraryParams.DefaultStyleFontFamily);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Path Tessellation Threshold Ratio", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.PathTessellationThresholdRatio = SetModelField(
                EditorGUILayout.FloatField(m_ExtendedLibraryParams.PathTessellationThresholdRatio, GUILayout.MaxWidth(InputFieldLayoutMaxWidth)),
                m_ExtendedLibraryParams.PathTessellationThresholdRatio);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Allow Multiple Rendering Threads", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.AllowMultipleRenderingThreads = SetModelField(
                GUILayout.Toggle(m_ExtendedLibraryParams.AllowMultipleRenderingThreads, ""),
                m_ExtendedLibraryParams.AllowMultipleRenderingThreads);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Text Atlas Dimensions", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.TextAtlasDimensions = SetModelField(
                EditorGUILayout.Vector2IntField("", m_ExtendedLibraryParams.TextAtlasDimensions, GUILayout.Width(InputFieldLayoutMaxWidth)),
                m_ExtendedLibraryParams.TextAtlasDimensions);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Enable Memory Tracking", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.EnableMemoryTracking = SetModelField(
                EditorGUILayout.Toggle(m_ExtendedLibraryParams.EnableMemoryTracking, GUILayout.MaxWidth(InputFieldLayoutMaxWidth)),
                m_ExtendedLibraryParams.EnableMemoryTracking);
            GUILayout.EndHorizontal();

            m_ExtendedLibraryParams.EnableDebugger = SetModelField(
                EditorGUILayout.BeginToggleGroup(new GUIContent("Enable Default UI System DevTools Debugger",
                    "Enable Default UI System DevTools Debugger in Unity3D Editor and development builds. You can access the Devtool through Context menu Gameface/Open Devtools. Require Google Chrome browser installed"), m_ExtendedLibraryParams.EnableDebugger),
                m_ExtendedLibraryParams.EnableDebugger);

            GUILayout.BeginHorizontal();
            GUILayout.Label("DevTools Debugger Port", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.DebuggerPort = SetModelField(
                EditorGUILayout.IntField(Mathf.Clamp(m_ExtendedLibraryParams.DebuggerPort, 1, ushort.MaxValue), GUILayout.MaxWidth(InputFieldLayoutMaxWidth)),
                m_ExtendedLibraryParams.DebuggerPort);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Enable DevTools Debugger in build", GUILayout.MaxWidth(OptionLayoutMaxWidth), GUILayout.Height(OptionLayoutHeight));
            m_ExtendedLibraryParams.EnableDebuggerInBuild = SetModelField(
                GUILayout.Toggle(m_ExtendedLibraryParams.EnableDebuggerInBuild, ""),
                m_ExtendedLibraryParams.EnableDebuggerInBuild);
            GUILayout.EndHorizontal();
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndScrollView();
        }

        private T SetModelField<T>(T newValue, T currentValue)
        {
            try
            {
                if (!currentValue.Equals(newValue))
                {
                    currentValue = newValue;
                    m_ParamsChanged = true;
                }
            }
            catch (NullReferenceException)
            {
                Popup();
            }

            return currentValue;
        }
    }
}