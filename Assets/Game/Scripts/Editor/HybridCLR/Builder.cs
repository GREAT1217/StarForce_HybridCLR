using System;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Builder
{
    public class Builder : EditorWindow
    {
        private BuilderController m_Controller;
        private int m_UnityVersionIndex;
        private int m_BuildPlatformIndex;

        [MenuItem("HybridCLR/HybridCLR Builder", false, 0)]
        private static void Open()
        {
            Builder window = GetWindow<Builder>("HybridCLR Builder", true);
            window.minSize = new Vector2(800f, 500f);
        }

        private void OnEnable()
        {
            m_Controller = new BuilderController();
        }

        private void OnGUI()
        {
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Install HybridCLR：(git and network required)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            GUISelectUnityDirectory("Unity安装目录的il2cpp", "Select");
            m_UnityVersionIndex = EditorGUILayout.Popup("Unity版本", m_UnityVersionIndex, m_Controller.UnityVersionNames);
            EditorGUILayout.LabelField("安装HybridCLR需要git和网络。点击Start开始执行命令行，务必检查运行结果，确保输出了success ，而不是其他错误，才表示安装成功。");
            GUIItem("初始化HybridCLR仓库并安装到到本项目。", "Start", InitHybridCLR);
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
            EditorGUILayout.LabelField("HybridCLR已经扫描过Unity核心库和常见的第三方库生成了默认的桥接函数集。");
            EditorGUILayout.LabelField("相关代码文件为huatuo/interpreter/MethodBridge_{abi}.cpp，其中{abi}为x64或arm64。");
            EditorGUILayout.LabelField("实践项目中总会遇到一些aot函数的共享桥接函数不在默认桥接函数集中。");
            EditorGUILayout.LabelField("因此提供了Editor工具，根据程序集自动生成所有桥接函数。");
            GUIItem("根据程序集自动生成所有桥接函数（x32）", "Generate", m_Controller.MethodBridge_X32);
            GUIItem("根据程序集自动生成所有桥接函数（x64）", "Generate", m_Controller.MethodBridge_X64);
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

        private void InitHybridCLR()
        {
            m_Controller.InitHybridCLR(m_UnityVersionIndex);
        }

        private void BuildHotfixDll()
        {
            m_Controller.BuildHotfixDll(m_BuildPlatformIndex);
        }
    }
}
