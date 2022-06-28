using System;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor
{
    public class HuatuoBuilder : EditorWindow
    {
        private HuatuoBuilderController m_Controller;
        private int m_UnityVersionIndex;
        private int m_BuildPlatformIndex;

        [MenuItem("Huatuo/HuatuoBuilder", false, 0)]
        private static void Open()
        {
            HuatuoBuilder window = GetWindow<HuatuoBuilder>("HuatuoBuilder", true);
            window.minSize = new Vector2(800f, 500f);
        }

        private void OnEnable()
        {
            m_Controller = new HuatuoBuilderController();
        }

        private void OnGUI()
        {
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Install Huatuo：(git and network required)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            GUISelectUnityDirectory("Unity安装目录的il2cpp", "Select");
            m_UnityVersionIndex = EditorGUILayout.Popup("Unity版本", m_UnityVersionIndex, m_Controller.UnityVersionNames);
            GUIItem("初始化Huatuo仓库，需要git和网络。", "Start", m_Controller.InitHuatuoRepositories);
            GUIItem("设置Huatuo仓库适配的Unity版本分支。", "Start", SetVersion);
            GUIItem("安装Huatuo到本项目。", "Start", m_Controller.InitLocalHuatuo);
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            m_BuildPlatformIndex = EditorGUILayout.Popup("Build Platform", m_BuildPlatformIndex, m_Controller.PlatformNames);
            GUIItem("由于ab包依赖裁剪后的dll，因此首先需要build工程。", "Build", BuildPlayerWindow.ShowBuildPlayerWindow);
            GUIItem("Build hotfix dll.", "Build", BuildHotfixDll);
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Method Bridge", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Huatuo已经扫描过Unity核心库和常见的第三方库生成了默认的桥接函数集。");
            EditorGUILayout.LabelField("相关代码文件为huatuo/interpreter/MethodBridge_{abi}.cpp，其中{abi}为x64或arm64。");
            EditorGUILayout.LabelField("实践项目中总会遇到一些aot函数的共享桥接函数不在默认桥接函数集中。");
            EditorGUILayout.LabelField("因此提供了Editor工具，根据程序集自动生成所有桥接函数。");
            GUIItem("根据程序集自动生成所有桥接函数（x64）", "Generate", m_Controller.MethodBridge_X86_64);
            GUIItem("根据程序集自动生成所有桥接函数（arm64）", "Generate", m_Controller.MethodBridge_Arm64);
            EditorGUILayout.EndVertical();
        }

        private void GUIItem(string content, string button, Action onClick)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content);
            if (GUILayout.Button(button, GUILayout.Width(100)))
            {
                onClick?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUISelectUnityDirectory(string content, string selectButton)
        {
            EditorGUILayout.BeginHorizontal();
            m_Controller.UnityInstallDirectory = EditorGUILayout.TextField(content, m_Controller.UnityInstallDirectory);
            if (GUILayout.Button(selectButton, GUILayout.Width(100)))
            {
                string temp = EditorUtility.OpenFolderPanel(content, m_Controller.UnityInstallDirectory, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    m_Controller.UnityInstallDirectory = temp;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void SetVersion()
        {
            m_Controller.SetVersion(m_UnityVersionIndex);
        }

        private void BuildHotfixDll()
        {
            m_Controller.BuildHotfixDll(m_BuildPlatformIndex);
        }
    }
}
