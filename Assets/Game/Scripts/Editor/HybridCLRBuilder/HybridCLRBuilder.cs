using System;
using HybridCLR.Editor;
using HybridCLR.Editor.Installer;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Editor
{
    public class HybridCLRBuilder : EditorWindow
    {
        private InstallerController m_InstallerController;
        private HybridCLRBuilderController m_HybridClrBuilderController;
        private GUIStyle m_InstallStateStyle;
        private int m_HotfixPlatformIndex;

        private int HotfixPlatformIndex
        {
            get
            {
                return EditorPrefs.GetInt("HybridCLRPlatform", 2);
            }
            set
            {
                m_HotfixPlatformIndex = value;
                EditorPrefs.SetInt("HybridCLRPlatform", m_HotfixPlatformIndex);
            }
        }

        [MenuItem("Game/HybridCLR Builder", false, 0)]
        private static void Open()
        {
            HybridCLRBuilder window = GetWindow<HybridCLRBuilder>("HybridCLR Builder", true);
            window.minSize = new Vector2(800f, 500f);
        }

        private void OnEnable()
        {
            m_InstallerController = new InstallerController();
            m_HybridClrBuilderController = new HybridCLRBuilderController();
            m_InstallStateStyle = new GUIStyle {richText = true, fontStyle = FontStyle.Bold};
            m_HotfixPlatformIndex = HotfixPlatformIndex;
        }

        private void OnGUI()
        {
            // Installer
            GUILayout.Space(5f);
            EditorGUILayout.LabelField($"Install HybridCLR (git and network required) {(m_InstallerController.HasInstalledHybridCLR() ? "<color=green>Installed</color>" : "<color=red>Not Installed</color>")}", m_InstallStateStyle);
            EditorGUILayout.BeginVertical("box");
            {
                GUISelectUnityDirectory();
                EditorGUILayout.LabelField($"当前Unity版本: {Application.unityVersion}，匹配的il2cpp_plus分支: {m_InstallerController.Il2CppBranch}");
                EditorGUILayout.LabelField("安装HybridCLR需要git和网络。点击Start开始执行命令行，务必检查运行结果，确保输出了success ，而不是其他错误，才表示安装成功。");
                GUIItem("初始化HybridCLR仓库并安装到到本项目。", "Start", InitHybridCLR);
            }
            EditorGUILayout.EndVertical();

            // Method Bridge Generator
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Method Bridge", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("HybridCLR已经扫描过Unity核心库和常见的第三方库生成了默认的桥接函数集。");
                EditorGUILayout.LabelField("相关代码文件为hybridclr/interpreter/MethodBridge_{abi}.cpp，其中{abi}Universal32、Universal64、Arm64。");
                EditorGUILayout.LabelField("实践项目中总会遇到一些aot函数的共享桥接函数不在默认桥接函数集中。因此提供了Editor工具，根据程序集自动生成所有桥接函数。");
                GUIItem("暂时没有仔细扫描泛型，如果运行时发现有生成缺失，需要自定义配置。", "Edit", EditMethodBridgeConfig);
                GUIGenerateMethodBridge();
            }
            EditorGUILayout.EndVertical();

            // Builder
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                GUIItem("由于ab包依赖裁剪后的dll，在编译hotfix.dl前需要build工程。", "Build", BuildPlayerWindow.ShowBuildPlayerWindow);
                int hotfixPlatformIndex = EditorGUILayout.Popup("选择hotfix平台。", m_HotfixPlatformIndex, m_HybridClrBuilderController.PlatformNames);
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
            m_InstallerController.Il2CppInstallDirectory = EditorGUILayout.TextField("Unity安装目录的il2cpp", m_InstallerController.Il2CppInstallDirectory);
            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                string temp = EditorUtility.OpenFolderPanel("Unity安装目录的il2cpp", m_InstallerController.Il2CppInstallDirectory, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    m_InstallerController.Il2CppInstallDirectory = temp;
                }
            }
            EditorGUILayout.EndHorizontal();

            InstallErrorCode err = m_InstallerController.CheckValidIl2CppInstallDirectory(m_InstallerController.Il2CppBranch, m_InstallerController.Il2CppInstallDirectory);
            switch (err)
            {
                case InstallErrorCode.Ok:
                {
                    break;
                }
                case InstallErrorCode.Il2CppInstallPathNotExists:
                {
                    EditorGUILayout.HelpBox("li2cpp 路径不存在", MessageType.Error);
                    break;
                }
                case InstallErrorCode.Il2CppInstallPathNotMatchIl2CppBranch:
                {
                    EditorGUILayout.HelpBox($"il2cpp 版本不兼容，最小版本为 {m_InstallerController.GetMinCompatibleVersion(m_InstallerController.Il2CppBranch)}", MessageType.Error);
                    break;
                }
                case InstallErrorCode.NotIl2CppPath:
                {
                    EditorGUILayout.HelpBox($"当前选择的路径不是il2cpp目录（必须类似 xxx/il2cpp）", MessageType.Error);
                    break;
                }
                default: throw new Exception($"not support {err}");
            }
        }

        private void GUIGenerateMethodBridge()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("根据程序集自动生成所有桥接函数。");
                if (GUILayout.Button("Release", GUILayout.Width(100)))
                {
                    MethodBridgeHelper.GenerateMethodBridgeAll(true);
                }
                if (GUILayout.Button("Develop", GUILayout.Width(100)))
                {
                    MethodBridgeHelper.GenerateMethodBridgeAll(false);
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
            m_InstallerController.InitHybridCLR(m_InstallerController.Il2CppBranch, m_InstallerController.Il2CppInstallDirectory);
        }

        private void CompileHotfixDll()
        {
            BuildTarget buildTarget = m_HybridClrBuilderController.GetBuildTarget(m_HotfixPlatformIndex);
            CompileDllHelper.CompileDll(buildTarget);
            m_HybridClrBuilderController.CopyDllAssets(buildTarget);
        }

        private static void EditMethodBridgeConfig()
        {
            string m_MethodBridgeConfigPath = "Assets/HybridCLR/Editor/Generators/GeneratorConfig.cs";
            if (AssetDatabase.CanOpenForEdit(m_MethodBridgeConfigPath))
            {
                var config = AssetDatabase.LoadAssetAtPath<Object>(m_MethodBridgeConfigPath);
                EditorGUIUtility.PingObject(config);
                AssetDatabase.OpenAsset(config);
            }
        }
    }
}
