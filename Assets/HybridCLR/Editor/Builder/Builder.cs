using System;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Builder
{
    public class Builder : EditorWindow
    {
        private BuilderController m_Controller;
        private int m_VersionIndex;
        private int m_HotfixPlatformIndex;

        private int VersionIndex {
            get { return EditorPrefs.GetInt("HybridCLRVersion", 0); }
            set
            {
                m_VersionIndex = value;
                EditorPrefs.SetInt("HybridCLRVersion", m_VersionIndex);
            }
        }

        private int HotfixPlatformIndex {
            get { return EditorPrefs.GetInt("HybridCLRPlatform", 2); }
            set
            {
                m_HotfixPlatformIndex = value;
                EditorPrefs.SetInt("HybridCLRPlatform", m_HotfixPlatformIndex);
            }
        }

        [MenuItem("HybridCLR/HybridCLR Builder", false, 0)]
        private static void Open()
        {
            Builder window = GetWindow<Builder>("HybridCLR Builder", true);
            window.minSize = new Vector2(800f, 500f);
        }

        private void OnEnable()
        {
            m_Controller = new BuilderController();
            m_VersionIndex = VersionIndex;
            m_HotfixPlatformIndex = HotfixPlatformIndex;
        }

        private void OnGUI()
        {
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Install HybridCLR：(git and network required)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                GUISelectUnityDirectory();
                int versionIndex = EditorGUILayout.Popup("Unity版本", m_VersionIndex, m_Controller.VersionNames);
                if (versionIndex != m_VersionIndex)
                {
                    VersionIndex = versionIndex;
                }
                EditorGUILayout.LabelField("安装HybridCLR需要git和网络。点击Start开始执行命令行，务必检查运行结果，确保输出了success ，而不是其他错误，才表示安装成功。");
                GUIItem("初始化HybridCLR仓库并安装到到本项目。", "Start", InitHybridCLR);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Method Bridge", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("HybridCLR已经扫描过Unity核心库和常见的第三方库生成了默认的桥接函数集。");
                EditorGUILayout.LabelField("相关代码文件为hybridclr/interpreter/MethodBridge_{abi}.cpp，其中{abi}Universal32、Universal64、Arm64。");
                EditorGUILayout.LabelField("实践项目中总会遇到一些aot函数的共享桥接函数不在默认桥接函数集中。因此提供了Editor工具，根据程序集自动生成所有桥接函数。");
                GUIItem("暂时没有仔细扫描泛型，如果运行时发现有生成缺失，需要自定义配置。", "Edit", m_Controller.EditMethodBridgeConfig);
                GUIGenerateMethodBridge();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                GUIItem("由于ab包依赖裁剪后的dll，在编译hotfix.dl前需要build工程。", "Build", BuildPlayerWindow.ShowBuildPlayerWindow);
                int hotfixPlatformIndex = EditorGUILayout.Popup("选择hotfix平台。", m_HotfixPlatformIndex, m_Controller.PlatformNames);
                if (hotfixPlatformIndex != m_HotfixPlatformIndex)
                {
                    HotfixPlatformIndex = hotfixPlatformIndex;
                }
                GUIItem("编译hotfix.dll。", "Compile", CompileHotfixDll);
                GUIResourcesTool();
            }
            EditorGUILayout.EndVertical();
        }

        private void GUIItem(string content, string button, Action onClick)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(content);
                if (GUILayout.Button(button, GUILayout.Width(100)))
                {
                    onClick?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUISelectUnityDirectory()
        {
            EditorGUILayout.BeginHorizontal();
            m_Controller.UnityInstallDirectory = EditorGUILayout.TextField("Unity安装目录的il2cpp", m_Controller.UnityInstallDirectory);
            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                string temp = EditorUtility.OpenFolderPanel("Unity安装目录的il2cpp", m_Controller.UnityInstallDirectory, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    m_Controller.UnityInstallDirectory = temp;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUIGenerateMethodBridge()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("根据程序集自动生成所有桥接函数。");
                if (GUILayout.Button("Universal32", GUILayout.Width(100)))
                {
                    m_Controller.MethodBridge_Universal32();
                }
                if (GUILayout.Button("Universal64", GUILayout.Width(100)))
                {
                    m_Controller.MethodBridge_Universal64();
                }
                if (GUILayout.Button("Arm64", GUILayout.Width(100)))
                {
                    m_Controller.MethodBridge_Arm64();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUIResourcesTool()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("编辑hotfix.dll等资源，并打包。");
                if (GUILayout.Button("Edit", GUILayout.Width(100)))
                {
                    EditorWindow window = GetWindow(Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceEditor,UnityGameFramework.Editor"));
                    window.Show();
                }
                if (GUILayout.Button("Build", GUILayout.Width(100)))
                {
                    EditorWindow window = GetWindow(Type.GetType("UnityGameFramework.Editor.ResourceTools.ResourceBuilder,UnityGameFramework.Editor"));
                    window.Show();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void InitHybridCLR()
        {
            m_Controller.InitHybridCLR(m_VersionIndex);
        }

        private void CompileHotfixDll()
        {
            m_Controller.CompileHotfixDll(m_HotfixPlatformIndex);
        }
    }
}
