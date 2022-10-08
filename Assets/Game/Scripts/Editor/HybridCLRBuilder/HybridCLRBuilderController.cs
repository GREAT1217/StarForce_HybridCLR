using System;
using System.IO;
using Game.Hotfix;
using GameFramework;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor
{
    public class HybridCLRBuilderController
    {
        private const string HotfixDllPath = "Assets/Game/HybridCLR/Dlls";
        private const string HotfixDllName = "Game.Hotfix.dll";

        public string[] PlatformNames { get; }

        public HybridCLRBuilderController()
        {
            PlatformNames = Enum.GetNames(typeof(Platform));
        }

        /// <summary>
        /// 由 UnityGameFramework.Editor.ResourceTools.Platform 得到 BuildTarget。
        /// </summary>
        /// <param name="platformIndex"></param>
        /// <returns>BuildTarget。</returns>
        public BuildTarget GetBuildTarget(int platformIndex)
        {
            Platform platform = (Platform) Enum.Parse(typeof(Platform), PlatformNames[platformIndex]);
            switch (platform)
            {
                case Platform.Windows:
                    return BuildTarget.StandaloneWindows;

                case Platform.Windows64:
                    return BuildTarget.StandaloneWindows64;

                case Platform.MacOS:
#if UNITY_2017_3_OR_NEWER
                    return BuildTarget.StandaloneOSX;
#else
                    return BuildTarget.StandaloneOSXUniversal;
#endif
                case Platform.Linux:
                    return BuildTarget.StandaloneLinux64;

                case Platform.IOS:
                    return BuildTarget.iOS;

                case Platform.Android:
                    return BuildTarget.Android;

                case Platform.WindowsStore:
                    return BuildTarget.WSAPlayer;

                case Platform.WebGL:
                    return BuildTarget.WebGL;

                default:
                    throw new GameFrameworkException("Platform is invalid.");
            }
        }

        /// <summary>
        /// 将 dll 文件拷贝至项目目录，用于 GameFramework 资源模块的编辑和打包。
        /// </summary>
        /// <param name="buildTarget"></param>
        public void CopyDllAssets(BuildTarget buildTarget)
        {
            IOUtility.CreateDirectoryIfNotExists(HotfixDllPath);
            string importSuffix = ".bytes";

            // Copy Hotfix Dll
            string oriFileName = Path.Combine(SettingsUtil.GetHotFixDllsOutputDirByTarget(buildTarget), HotfixDllName);
            string desFileName = Path.Combine(HotfixDllPath, HotfixDllName + importSuffix);
            File.Copy(oriFileName, desFileName, true);

            // Copy AOT Dll
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            foreach (var dllName in GameHotfixEntry.AOTDllNames)
            {
                oriFileName = Path.Combine(aotDllPath, dllName);
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOT dll.");
                    continue;
                }
                desFileName = Path.Combine(HotfixDllPath, dllName + importSuffix);
                File.Copy(oriFileName, desFileName, true);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
